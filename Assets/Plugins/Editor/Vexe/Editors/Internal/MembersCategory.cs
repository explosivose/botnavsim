//#define PROFILE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Vexe.Editor.GUIs;
using Vexe.Runtime.Extensions;
using Vexe.Runtime.Types;
using UnityObject = UnityEngine.Object;

namespace Vexe.Editor.Internal
{
	public class MembersCategory
	{
		private readonly string fullPath;
		private readonly string name;
		private readonly string id;
		private readonly BetterPrefs prefs;
		private MembersDisplay display;
		private UnityObject prevTarget;

		public List<MemberInfo> Members { get; set; }
		public List<MembersCategory> NestedCategories { get; set; }

		public string Name { get { return name; } }
		public string FullPath { get { return fullPath; } }
		public BaseGUI gui { get; set; }
		public float DisplayOrder { get; set; }
		public bool ForceExpand { get; set; }
		public bool HideHeader { get; set; }
		public bool IsExpanded { get; set; }
		public bool Indent { get; set; }

		public MembersDisplay Display
		{
			get { return display; }
			set
			{
				if (display != value)
				{
					display = value;
					Members.OfType<MembersCategory>().Foreach(c => c.Display = display);
				}
			}
		}

		public MembersCategory(string fullPath, List<MemberInfo> members, float displayOrder, string id, BetterPrefs prefs)
		{
			Members = members;
			DisplayOrder = displayOrder;
			this.prefs = prefs;
			this.fullPath = fullPath;
			this.name = FullPath.Substring(FullPath.LastIndexOf('/') + 1);
			this.id = id + fullPath;
			Indent = true;

			NestedCategories = new List<MembersCategory>();
		}

		public void AddMember(MemberInfo member)
		{
			Members.Add(member);
		}

		// Keys & Foldouts
		#region
		private bool DoHeader()
		{
			bool foldout = false;
			using (gui.Horizontal(EditorStyles.toolbarButton))
			{
				gui.Space(10f);
				foldout = gui.Foldout(name, prefs.Bools.ValueOrDefault(id), Layout.sExpandWidth());
				prefs.Bools[id] = foldout;
			}

			return foldout;
		}
		#endregion

		public void Draw(UnityObject target)
		{
			if (Members.Count == 0 && NestedCategories.Count == 0)
				return;

			IsExpanded = HideHeader || DoHeader();
			if (!(IsExpanded || ForceExpand))
				return;

			bool changedTarget;
			if (target != prevTarget)
			{
				prevTarget = target;
				changedTarget = true;
			}
			else changedTarget = false;

			gui.Space(1f);

			bool showGuiBox   = (Display & MembersDisplay.GuiBox) > 0;
			bool showSplitter = (Display & MembersDisplay.Splitter) > 0;

			using (gui.Indent(showGuiBox ? EditorStyles.textArea : GUIStyle.none, Indent))
			{
				gui.Space(5f);
#if PROFILE
				Profiler.BeginSample(name + " Members");
#endif
				for (int i = 0, imax = Members.Count; i < imax; i++)
				{
					var member = Members[i];

					if (!MemberUtil.IsVisible(member, target, changedTarget))
						continue;

					using (gui.Horizontal())
					{
						gui.Space(10f);

						using (gui.Vertical())
							gui.Member(member, target, target, id, false);
					}

					if (showSplitter)
						gui.Splitter();

					gui.Space(2f);
				}

				for (int i = 0, imax = NestedCategories.Count; i < imax; i++)
				{
					var cat = NestedCategories[i];

					cat.gui = this.gui;

					using (gui.Horizontal())
					{
						if (IsExpanded)
							gui.Space(4f);

						using (gui.Vertical())
							cat.Draw(target);
					}
				}
#if PROFILE
				Profiler.EndSample();
#endif
			}
		}
	}

}

namespace Vexe.Editor
{
	public static class MemberUtil
	{
		static Func<MemberInfo, Attribute[]> _getAttributes;
		static Func<MemberInfo, Attribute[]> getAttributes
		{
			get { return _getAttributes ?? (_getAttributes = new Func<MemberInfo, Attribute[]>(member => member.GetAttributes()).Memoize()); }
		}

		static Func<Tuple<Type, string>, MethodInfo> _getMethod;
		static Func<Tuple<Type, string>, MethodInfo> getMethod
		{
			get { return _getMethod ?? (_getMethod = new Func<Tuple<Type, string>, MethodInfo>(tup => tup.Item1.GetMethod(tup.Item2, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)).Memoize()); }
		}

		static Dictionary<MemberInfo, MethodInfo> _visibleLookup = new Dictionary<MemberInfo, MethodInfo>();

		static public bool IsVisible(MemberInfo member, UnityObject target, bool changedTarget)
		{
			MethodInfo isVisible;
			if (changedTarget || !_visibleLookup.TryGetValue(member, out isVisible))
			{
				var vis = getAttributes(member).GetAttribute<VisibleWhenAttribute>();
				if (vis == null)
					return true;

				var method = getMethod(Tuple.Create(target.GetType(), vis.conditionMethod));
				if (method == null)
				{
					UnityEngine.Debug.LogError("Method not found: " + vis.conditionMethod);
					_visibleLookup[member] = null;
					return true;
				}

				_visibleLookup[member] = isVisible = method;
			}

			return (bool)isVisible.Invoke(target, null);
		}
	}
}
