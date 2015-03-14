using System.Collections.Generic;
using UnityEngine;
using Vexe.Runtime.Extensions;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Vexe.Runtime.Types
{
	[DefineCategories("Plural", "Single")]
	public class BetterPrefs : BetterScriptableObject
	{
		private const string Plural = "Plural", Single = "Single";

		[Category(Single), Serialize] private Dictionary<string, int>     _Ints;
		[Category(Single), Serialize] private Dictionary<string, string>  _Strings;
		[Category(Single), Serialize] private Dictionary<string, float>   _Floats;
		[Category(Single), Serialize] private Dictionary<string, bool>    _Bools;
		[Category(Single), Serialize] private Dictionary<string, Vector3> _Vector3s;
		[Category(Single), Serialize] private Dictionary<string, Color>   _Colors;

		[Category(Plural), Serialize] private Dictionary<string, List<int>>     _IntLists;
		[Category(Plural), Serialize] private Dictionary<string, List<string>>  _StringLists;
		[Category(Plural), Serialize] private Dictionary<string, List<float>>   _FloatLists;
		[Category(Plural), Serialize] private Dictionary<string, List<bool>>    _BoolLists;
		[Category(Plural), Serialize] private Dictionary<string, List<Vector3>> _Vector3Lists;
		[Category(Plural), Serialize] private Dictionary<string, List<Color>>   _ColorLists;

		public Dictionary<string, int> Ints         { set { _Ints     = value; } get { return Lazy(ref _Ints);     } }
		public Dictionary<string, string> Strings   { set { _Strings  = value; } get { return Lazy(ref _Strings);  } }
		public Dictionary<string, float> Floats     { set { _Floats   = value; } get { return Lazy(ref _Floats);   } }
		public Dictionary<string, bool> Bools       { set { _Bools    = value; } get { return Lazy(ref _Bools);    } }
		public Dictionary<string, Vector3> Vector3s { set { _Vector3s = value; } get { return Lazy(ref _Vector3s); } }
		public Dictionary<string, Color> Colors     { set { _Colors   = value; } get { return Lazy(ref _Colors);   } }

		public Dictionary<string, List<int>> IntLists         { set { _IntLists     = value; } get { return Lazy(ref _IntLists);     } }
		public Dictionary<string, List<string>> StringLists   { set { _StringLists  = value; } get { return Lazy(ref _StringLists);  } }
		public Dictionary<string, List<float>> FloatLists     { set { _FloatLists   = value; } get { return Lazy(ref _FloatLists);   } }
		public Dictionary<string, List<bool>> BoolLists       { set { _BoolLists    = value; } get { return Lazy(ref _BoolLists);    } }
		public Dictionary<string, List<Vector3>> Vector3Lists { set { _Vector3Lists = value; } get { return Lazy(ref _Vector3Lists); } }
		public Dictionary<string, List<Color>> ColorLists     { set { _ColorLists   = value; } get { return Lazy(ref _ColorLists);   } }

		private Dictionary<string, T> Lazy<T>(ref Dictionary<string, T> dictionary)
		{
			return dictionary ?? (dictionary = new Dictionary<string, T>());
		}

		[Show, Category(Single)] void ClearSingles()
		{
			Ints.Clear();
			Strings.Clear();
			Floats.Clear();
			Bools.Clear();
			Vector3s.Clear();
			Colors.Clear();
		}

		[Show, Category(Plural)] void ClearPlurals()
		{
			IntLists.Clear();
			StringLists.Clear();
			FloatLists.Clear();
			BoolLists.Clear();
			Vector3Lists.Clear();
			ColorLists.Clear();
		}

#if UNITY_EDITOR
		private const string EditorPrefsPath  = "Assets/Plugins/Editor/Vexe/ScriptableAssets/BetterEditorPrefs.asset";
		private const string RuntimePrefsPath = "Assets/Plugins/Vexe/Runtime/ScriptableAssets/BetterPrefs.asset";

		private static BetterPrefs editorInstance;
		public static BetterPrefs EditorInstance
		{
			get
			{
				if (editorInstance == null)
				{
					editorInstance = AssetDatabase.LoadAssetAtPath(EditorPrefsPath, typeof(BetterPrefs)) as BetterPrefs;
					if (editorInstance == null)
					{
						editorInstance = ScriptableObject.CreateInstance<BetterPrefs>();
						AssetDatabase.CreateAsset(editorInstance, EditorPrefsPath);
						AssetDatabase.ImportAsset(EditorPrefsPath, ImportAssetOptions.ForceUpdate);
					}
				}
				return editorInstance;
			}
		}

		public static class BetterPrefsMenus
		{
			[MenuItem("Tools/Vexe/BetterPrefs/CreateAsset")]
			public static void CreateBetterPrefsAsset()
			{
				AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<BetterPrefs>(), RuntimePrefsPath);
			}
		}
#endif

		public void TryAdd(object obj, string key)
		{
			if (obj == null) return;

			var type = obj.GetType();
			if (type == typeof(int))
				 Ints[key] = (int)obj;
			if (type == typeof(string))
				 Strings[key] = (string)obj;
			if (type == typeof(float))
				 Floats[key] = (float)obj;
			if (type == typeof(bool))
				 Bools[key] = (bool)obj;
		}

		public object ValueOrDefault(Type type, string key)
		{
			if (type == typeof(int))
				return Ints.ValueOrDefault(key);
			if (type == typeof(string))
				return Strings.ValueOrDefault(key);
			if (type == typeof(float))
				return Floats.ValueOrDefault(key);
			if (type == typeof(bool))
				return Bools.ValueOrDefault(key);
			return type.GetDefaultValue();
		}
	}
}

#if UNITY_EDITOR
namespace Vexe.Editor
{
	using Vexe.Runtime.Extensions;
	using Vexe.Runtime.Types;

	public class Foldouts
	{
		private readonly BetterPrefs prefs;

		public Foldouts(BetterPrefs prefs)
		{
			this.prefs = prefs;
		}

		public bool this[string key]
		{
			get { return prefs.Bools.ValueOrDefault(key); }
			set
			{
				prefs.Bools[key] = value;
				EditorUtility.SetDirty(prefs);
			}
		}
	}
}
#endif
