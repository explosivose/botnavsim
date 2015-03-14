#define PROFILE
//#define DBG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Fasterflect;
using UnityEditor;
using UnityEngine;
using Vexe.Editor.Helpers;
using Vexe.Runtime.Extensions;
using Vexe.Runtime.Types;
using UnityObject = UnityEngine.Object;

namespace Vexe.Editor.Drawers
{
	public class DictionaryDrawer<TKey, TValue> : ObjectDrawer<Dictionary<TKey, TValue>> where TKey : new()
	{
		private List<ElementMember<TKey>> keyElements;
		private List<ElementMember<TValue>> valueElements;
		private KVPList<TKey, TValue> kvpList;
		private string dictionaryName;
		private string pairFormatPattern;
		private MethodInvoker pairFormatMethod;
		private bool perKeyDrawing, perValueDrawing;
		private bool shouldRead = true, shouldWrite, invalidKeyType;
		private Color dupKeyColor, shouldWriteColor;

		public bool Readonly { get; set; }

		protected override void OnSingleInitialization()
		{
			shouldWriteColor = GUIHelper.OrangeColorDuo.FirstColor;
			dupKeyColor = GUIHelper.RedColorDuo.FirstColor;

			var kt = typeof(TKey);

			if (kt.IsAbstract || kt.IsA<UnityObject>())
			{ 
				//Log("key type is abstract or a unityobject");
				invalidKeyType = true;
			}

			if (!invalidKeyType && !kt.IsValueType && kt != typeof(string))
			{
				try
				{
					kt.ActivatorInstance();
				}
				catch (MissingMemberException)
				{
					//Log("key type is not newable");
					invalidKeyType = true;
				}
			}

			if (invalidKeyType)
				return;

			keyElements   = new List<ElementMember<TKey>>();
			valueElements = new List<ElementMember<TValue>>();

			perKeyDrawing   = attributes.AnyIs<PerKeyAttribute>();
			perValueDrawing = attributes.AnyIs<PerValueAttribute>();
			Readonly		= attributes.AnyIs<ReadonlyAttribute>();

			var formatMember = attributes.OfType<FormatMemberAttribute>().FirstOrDefault();
			if (formatMember == null || string.IsNullOrEmpty(formatMember.pattern))
			{ 
				dictionaryName  = niceName;
				dictionaryName += " (" + memberType.GetNiceName() + ")";
			}
			else
			{
				dictionaryName = formatMember.Format(niceName, memberType.GetNiceName());
			}

			var pairFormat = attributes.GetAttribute<FormatPairAttribute>();
			if (pairFormat != null)
			{
				if (!string.IsNullOrEmpty(pairFormat.Method))
					pairFormatMethod = rawTarget.GetType().DelegateForCallMethod(pairFormat.Method, Flags.InstanceAnyVisibility, typeof(TKey), typeof(TValue));
				else if (!string.IsNullOrEmpty(pairFormat.Pattern))
					pairFormatPattern = pairFormat.Pattern;
			}

			if (Readonly)
				dictionaryName += " (Readonly)";

			#if DBG
			Log("Dictionary drawer Initialized (" + dictionaryName + ")");
			#endif
		}

		public class AddInfo
		{
			public TKey key;
			public TValue value;
		}

		public override void OnGUI()
		{
			if (invalidKeyType)
			{
				gui.HelpBox("Key type {0} must either be a ValueType or a 'new'able ReferenceType (not an abstract type/a UnityEngine.Object and has an empty or implicit/compiler-generated constructor)".FormatWith(typeof(TKey).Name),
					MessageType.Error);
				return;
			}

			if (memberValue == null)
			{ 
				#if DBG
				Log("Dictionary null " + dictionaryName);
				#endif
				memberValue = new Dictionary<TKey, TValue>();
			}

			shouldRead |= (kvpList == null || (!shouldWrite && memberValue.Count != kvpList.Count));

			if (shouldRead)
			{
				#if DBG
				Log("Reading " + dictionaryName);
				#endif
				kvpList = memberValue.ToKVPList();
				shouldRead = false;
			}


			#if PROFILE
			Profiler.BeginSample("DictionaryDrawer Header");
			#endif

			using (gui.Horizontal())
			{
				foldout = gui.Foldout(dictionaryName, foldout, Layout.sExpandWidth());

				if (!Readonly)
				{
					gui.FlexibleSpace();

					using (gui.State(kvpList.Count > 0))
					{
						if (gui.ClearButton("dictionary"))
						{
							kvpList.Clear();
							shouldWrite = true;
						}

						if (gui.RemoveButton("last dictionary pair"))
						{
							kvpList.RemoveFirst();
							shouldWrite = true;
						}
					}

					if (gui.AddButton("pair"))
					{
						AddNewPair();
						shouldWrite = true;
					}

					Color col;
					if (!kvpList.Keys.IsUnique())
						col = dupKeyColor;
					else if (shouldWrite)
						col = shouldWriteColor;
					else col = Color.white;

					using (gui.ColorBlock(col))
					if (gui.MiniButton("w", "Write dictionary (Orange means you modified the dictionary and should write, Red means you have a duplicate key and must address it before writing)", MiniButtonStyle.ModRight))
					{
						#if DBG
						Log("Writing " + dictionaryName);
						#endif
						try
						{
							memberValue = kvpList.ToDictionary();
						}
						catch (ArgumentException e)
						{
							Log(e.Message);
						}

						shouldWrite = false;
					}
				}
			}

			#if PROFILE
			Profiler.EndSample();
			#endif

			if (!foldout)
				return;

			if (kvpList.Count == 0)
			{
				gui.HelpBox("Dictionary is empty");
			}
			else
			{ 
				#if PROFILE
				Profiler.BeginSample("DictionaryDrawer Pairs");
				#endif
				using (gui.Indent())
				{
					for (int i = 0; i < kvpList.Count; i++)
					{
						var dKey   = kvpList.Keys[i];
						var dValue = kvpList.Values[i];

						#if PROFILE
						Profiler.BeginSample("DictionaryDrawer KVP assignments");
						#endif

						var pairStr        = FormatPair(dKey, dValue);
						var entryKey       = id + i + "entry";
						foldouts[entryKey] = gui.Foldout(pairStr, foldouts[entryKey], Layout.sExpandWidth());

						#if PROFILE
						Profiler.EndSample();
						#endif

						if (!foldouts[entryKey])
							continue;

						#if PROFILE
						Profiler.BeginSample("DictionaryDrawer SinglePair");
						#endif
						using (gui.Indent())
						{
							var keyMember = GetElement(keyElements, kvpList.Keys, i, entryKey + "key");
							shouldWrite |= gui.Member(keyMember, !perKeyDrawing);

							var valueMember = GetElement(valueElements, kvpList.Values, i, entryKey + "value");
							shouldWrite |= gui.Member(valueMember, !perValueDrawing);
						}
						#if PROFILE
						Profiler.EndSample();
						#endif
					}
				}
				#if PROFILE
				Profiler.EndSample();
				#endif

				shouldWrite |= memberValue.Count > kvpList.Count;
			}

			//if (shouldWrite)
			//{
			//	#if DBG
			//	Log("Writing " + dictionaryName);
			//	#endif
			//	try
			//	{
			//		memberValue = kvpList.ToDictionary();
			//	}
			//	catch (ArgumentException e)
			//	{
			//		Log(e.Message);
			//	}
			//	shouldWrite = false;
			//}
		}

		private ElementMember<T> GetElement<T>(List<ElementMember<T>> elements, List<T> source, int index, string id)
		{
			if (index >= elements.Count)
			{
				var element = new ElementMember<T>(
					@id          : id + index,
					@attributes  : attributes,
					@name        : string.Empty
				);
				element.Initialize(source, index, rawTarget, unityTarget);
				elements.Add(element);
				return element;
			}

			try
			{
				var e = elements[index];
				e.Initialize(source, index, rawTarget, unityTarget);
				return e;
			}
			catch (ArgumentOutOfRangeException)
			{
				Log("DictionaryDrawer: Accessing element out of range. Index: {0} Count {1}. This shouldn't really happen. Please report it with information on how to replicate it".FormatWith(index, elements.Count));
				return null;
			}
		}

		private string FormatPair(TKey key, TValue value)
		{
			return formatPair(new KeyValuePair<TKey, TValue>(key, value));
		}

		private Func<KeyValuePair<TKey, TValue>, string> _formatPair;
		private Func<KeyValuePair<TKey, TValue>, string> formatPair
		{
			get
			{
				return _formatPair ?? (_formatPair = new Func<KeyValuePair<TKey, TValue>, string>(pair =>
				{
					var key = pair.Key;
					var value = pair.Value;

					if (pairFormatPattern == null)
					{ 
						if (pairFormatMethod == null)
							return string.Format("[{0}, {1}]", GetObjectString(key), GetObjectString(value));
						return pairFormatMethod(rawTarget, pair.Key, pair.Value) as string;
					}

					var result = pairFormatPattern;
					result = Regex.Replace(result, @"\$keyType", key == null ? "null" : key.GetType().GetNiceName());
					result = Regex.Replace(result, @"\$valueType", value == null ? "null" : value.GetType().GetNiceName());
					result = Regex.Replace(result, @"\$key", GetObjectString(key));
					result = Regex.Replace(result, @"\$value", GetObjectString(value));
					return result;
				}).Memoize());
			}
		}


		private string GetObjectString(object from)
		{
			if (from.IsObjectNull())
				return "null";
			var obj = from as UnityObject;
			return (obj != null) ? (obj.name + " (" + obj.GetType().Name + ")") : from.ToString();
		}

		Func<TKey> _getNewKey;
		Func<TKey> getNewKey
		{
			get
			{
				if (_getNewKey == null)
				{
					if (typeof(TKey).IsValueType)
					{
						_getNewKey = () => (TKey)typeof(TKey).GetDefaultValue();
					}
					else if (typeof(TKey) == typeof(string))
					{
						_getNewKey = () => (TKey)(object)string.Empty;
					}
					else
					{
						_getNewKey = () => new TKey();
					}
				}
				return _getNewKey;
			}
		}

		private void AddNewPair()
		{
			try
			{
				var key = getNewKey();
				var value = default(TValue); 
				//kvpList.Add(key, value, false);
				kvpList.Insert(0, key, value, false);

				var pkey = id + (kvpList.Count - 1) + "entry";
				foldouts[pkey] = true;
			}
			catch (ArgumentException e)
			{
				Log(e.Message);
			}
		}
	}
}
