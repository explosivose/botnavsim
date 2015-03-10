using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fasterflect;
using UnityEditor;
using UnityEngine;
using Vexe.Editor.GUIs;
using Vexe.Editor.Helpers;
using Vexe.Runtime.Extensions;
using Vexe.Runtime.Helpers;
using Vexe.Runtime.Serialization;
using Vexe.Runtime.Types;
using UnityObject = UnityEngine.Object;

namespace Vexe.Editor.Drawers
{
	public class RecursiveDrawer : ObjectDrawer<object>
	{
		private bool isToStringImpl;
		private string nullString;
		private Type polymorphicType;

		protected override void OnSingleInitialization()
		{
			nullString = string.Format("null ({0})", memberTypeName);

			if (memberValue != null)
			{ 
				polymorphicType = memberValue.GetType();
				isToStringImpl = polymorphicType.IsMethodImplemented("ToString");
			}
			else 
				isToStringImpl = memberType.IsMethodImplemented("ToString");
		}

		public override void OnGUI()
		{
			if (!DrawField())
				return;

			if (member.IsNull())
			{
				gui.HelpBox("Member value is null");
				return;
			}

			if (polymorphicType == null || polymorphicType == memberType)
			{
				//object wrap = member.Value.UnwrapIfWrapped(); // A *MUST* for structs!
				//object wrap = member.getter(member.Target.UnwrapIfWrapped());
				object wrap = member.Value;
				DrawRecursive(wrap, gui, id, unityTarget);
				//DrawRecursive(member.Value, gui, id, unityTarget);
				member.Value = wrap;
			}
			else
			{
				var handler = MemberDrawersHandler.Instance;
				var drawer = handler.GetMemoObjDrawer(polymorphicType);
				var drawerType = drawer.GetType();
				if (drawerType == typeof(RecursiveDrawer))
				{
					object wrap = member.Value;
					DrawRecursive(wrap, gui, id, unityTarget);
					member.Value = wrap;
				}
				else
				{
					drawer.Initialize(member, attributes, gui);
					gui.Member(member, attributes, drawer, false);
				}
			}
		}

		/// <summary>
		/// if memberNames was null or empty, draws members in 'obj' recursively. Members are fetched according to the default serializaiton logic
		/// otherwise, draws only the specified members by memberNames
		/// </summary>
		public static bool DrawRecursive(object obj, BaseGUI gui, string id, UnityObject unityTarget, params string[] memberNames)
		{
			List<MemberInfo> members;
			var objType = obj.GetType();
			if (memberNames.IsNullOrEmpty())
			{ 
				members = SerializationLogic.Default.GetMemoizedVisibleMembers().Invoke(objType);
			}
			else
			{
				members = new List<MemberInfo>();
				for (int i = 0; i < memberNames.Length; i++)
				{
					var name = memberNames[i];
					var member = ReflectionUtil.GetMemoizedMember(objType, name);
					if (member == null)
					{
						LogFormat("RecursiveDrawer: Couldn't find member {0} in {1}", name, objType.Name);
						continue;
					}
					if (SerializationLogic.Default.IsVisibleMember(member))
						members.Add(member);
				}
			}

			if (members.Count == 0)
			{
				gui.HelpBox("Object doesn't have any visible members");
				return false;
			}

			bool changed = false;
			using (gui.Indent())
			{
				for (int i = 0; i < members.Count; i++)
				{
					//var test = (ItemSize)obj;
					//test.area = 20;
					//gui.Label(test.area.ToString());
					//obj = test;
					var member = members[i];
					changed |= gui.Member(member, obj, unityTarget, id, false);
				}
			}

			return changed;
		}

		private bool DrawField()
		{
			using (gui.Horizontal())
			{
				var isEmpty = string.IsNullOrEmpty(niceName);
				var display = isEmpty ? string.Empty : niceName + " " + (foldout ? "^" : ">");
				var value   = member.Value;
				string field;
				
				if (value == null)
				{
					field = nullString;
				}
				else
				{
					if (isToStringImpl)
						field = value.ToString();
					else
						field = string.Format("{0} ({1})", value.ToString(), value.GetType().GetNiceName());
				}

				if (isEmpty)
					Foldout();

				gui.Text(display, field);

				var fieldRect = gui.LastRect;
				gui.Cursor(fieldRect, MouseCursor.Link);
				if (!isEmpty && fieldRect.Contains(Event.current.mousePosition))
				{
					if (EventsHelper.MouseEvents.IsLMB_MouseDown())
						foldout = !foldout;
				}

				var drop = gui.RegisterFieldForDrop<UnityObject>(fieldRect, objs => objs.Select(x =>
				{
					if (x == null)
						return null;

					var go = x as GameObject;
					if (go != null)
						return go.GetComponent(memberType);

					return x.GetType().IsA(memberType) ? x : null;

				}).FirstOrDefault());

				if (drop != null)
					value = memberValue = drop;

				SelectionButton();
			}

			return foldout;
		}

		protected virtual void SelectionButton()
		{
			var tabs = new List<Tab>();

			Action<Func<Type[]>, Action<Type>, string> newTypeTab = (getValues, create, title) =>
				tabs.Add(new Tab<Type>(
					@getValues: getValues,
					@getCurrent: () => { var x = memberValue; return x == null ? null : x.GetType(); },
					@setTarget: newType => { if (newType == null) memberValue = memberType.GetDefaultValueEmptyIfString(); else create(newType); },
					@getValueName: type => type.Name,
					@title: title
				));

			if (memberType.IsInterface)
			{
				Action<Func<UnityObject[]>, string> newUnityTab = (getValues, title) =>
					tabs.Add(new Tab<UnityObject>(
						@getValues: getValues,
						@getCurrent: member.As<UnityObject>,
						@setTarget: member.Set,
						@getValueName: obj => obj.name + " (" + obj.GetType().Name + ")",
						@title: title
					));

				newUnityTab(() => UnityObject.FindObjectsOfType<UnityObject>()
											 .OfType(memberType)
											 .ToArray(), "Scene");

				newUnityTab(() => PrefabHelper.GetComponentPrefabs(memberType)
											  .ToArray(), "Prefabs");

				newTypeTab(() => ReflectionHelper.GetAllUserTypesOf(memberType)
												 .Where(t => t.IsA<MonoBehaviour>())
												 .Where(t => !t.IsAbstract)
												 .ToArray(), TryCreateInstanceInGO, "MonoBehaviours");
			}

			newTypeTab(() => ReflectionHelper.GetAllUserTypesOf(memberType)
											 .Disinclude(memberType.IsAbstract ? memberType : null)
											 .Where(t => !t.IsA<UnityObject>() && !t.IsAbstract)
											 .ToArray(), TryCreateInstance, "System types");

			var click = Event.current.button;
			if (gui.SelectionButton("Left click: select type. Right click: try instantiate"))
			{
				if (click == 0)
				{
					SelectionWindow.Show("Select a `" + memberTypeName + "` object", tabs.ToArray());
				}
				else if (click == 1)
				{
					try
					{
						memberValue = memberType.ActivatorInstance();
					}
					catch(Exception e)
					{
						Debug.Log("Error. Couldn't create instance: " + e.Message);
					}
				}
			}
		}

		private void TryCreateInstanceInGO(Type newType)
		{
			TryCreateInstance(() => new GameObject("(new) " + newType.Name).AddComponent(newType));
		}

		private void TryCreateInstance(Type newType)
		{
			TryCreateInstance(() => newType.Instance());
		}

		private void TryCreateInstance(Func<object> create)
		{
			try
			{
				var inst = create();
				member.Set(inst);

				if (memberValue != null)
					polymorphicType = memberValue.GetType();

				EditorHelper.RepaintAllInspectors();
			}
			catch (TargetInvocationException e)
			{
				Debug.LogError(string.Format("Couldn't create instance of type {0}. Make sure the type has an empty constructor. Message: {1}, Stacktrace: {2}", memberTypeName, e.Message, e.StackTrace));
			}
		}
	}
}
