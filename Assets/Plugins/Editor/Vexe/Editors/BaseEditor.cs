//#define PROFILE
//#define DBG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fasterflect;
using UnityEditor;
using UnityEngine;
using Vexe.Editor.GUIs;
using Vexe.Editor.Internal;
using Vexe.Runtime.Exceptions;
using Vexe.Runtime.Extensions;
using Vexe.Runtime.Helpers;
using Vexe.Runtime.Serialization;
using Vexe.Runtime.Types;
using UnityObject = UnityEngine.Object;

namespace Vexe.Editor.Editors
{
	using Editor = UnityEditor.Editor;

	public abstract class BaseEditor : Editor
	{
		/// <summary>
		/// A value used from the editor to determine what view to use for the behaviour
		/// 0 means to use FullView unless annotated with something different
		/// 1 means to use BasicView unless annotated with something different
		/// 2 means to use MinimalView unless annotated with something different
		/// </summary>
		public int EditorView = 1;

		/// <summary>
		/// Whether or not to show the script header
		/// </summary>
		public bool ShowHeader = true;

		protected BaseGUI gui;

		private string _id;
		private Type _targetType;
		private static Foldouts _foldouts;

		private List<MembersCategory> _baseCategories;
		private List<MemberInfo> _visibleMembers;
		private SerializedProperty _script;
		private EditorMember _serializationData, _debug;
		private bool _minimalView, _previousGUI;
		private int _repaintCount;
		private MembersDisplay _display;
		private UnityObject _prevTarget;

		private static bool useUnityGUI
		{
			get { return prefs.Bools.ValueOrDefault("UnityGUI"); }
			set { prefs.Bools["UnityGUI"] = value; }
		}

		protected GameObject gameObject
		{
			get
			{
				var comp = target as Component;
				return comp == null ? null : comp.gameObject;
			}
		}

		protected static BetterPrefs prefs
		{
			get { return BetterPrefs.EditorInstance; }
		}

		protected string id
		{
			get { return !string.IsNullOrEmpty(_id) ? _id : (_id = RTHelper.GetTargetID(target)); }
		}

		protected bool foldout
		{
			get { return foldouts[id]; }
			set { foldouts[id] = value; }
		}

		protected static Foldouts foldouts
		{
			get { return _foldouts ?? (_foldouts = new Foldouts(prefs)); }
		}

		protected Type targetType
		{
			get { return _targetType ?? (_targetType = target.GetType()); }
		}

		protected TTarget TargetAs<TTarget>() where TTarget : UnityObject
		{
			return target as TTarget;
		}

		private void OnEnable()
		{
			_previousGUI = useUnityGUI;
			gui = _previousGUI ? (BaseGUI)new TurtleGUI() : new RabbitGUI();
			Initialize();
		}

		public void OnInlinedGUI(BaseGUI otherGui)
		{
			this.gui = otherGui;
			GUICode();
		}

		public sealed override void OnInspectorGUI()
		{
			if (_previousGUI != useUnityGUI)
			{
				_previousGUI = useUnityGUI;
				gui = _previousGUI ? (BaseGUI)new TurtleGUI() : new RabbitGUI();
			}

			gui.OnGUI(GUICode, new Vector2(0f, 35f));
		}

		protected static void LogFormat(string msg, params object[] args)
		{
			Debug.Log(string.Format(msg, args));
		}

		protected static void Log(object msg)
		{
			Debug.Log(msg);
		}

		private static MembersDisplay defaultDisplay
		{
			get { return MembersDisplay.GuiBox; }
		}

		protected virtual void OnBeforeInitialized() { }
		protected virtual void OnAfterInitialized() { }
		protected virtual void OnGUI() { }

		private void Initialize()
		{
			OnBeforeInitialized();

			// Init members
			var allMembers = ReflectionUtil.GetMemoizedMembers(targetType);
			_visibleMembers = SerializationLogic.Default.GetVisibleMembers(allMembers).ToList();

			Func<MemberInfo, float> getDisplayOrder = member =>
			{
				var attr = member.GetCustomAttribute<DisplayOrderAttribute>();
				return attr == null ? -1 : attr.displayOrder;
			};

			var basic = targetType.IsDefined<BasicViewAttribute>(true);
			var full = targetType.IsDefined<FullViewAttribute>(true);
			var minimal = targetType.IsDefined<MinimalViewAttribute>(true);

			if (minimal || (!full && !basic && EditorView == 2))
			{
				_minimalView = true;
				_visibleMembers = _visibleMembers.OrderBy(getDisplayOrder).ToList();
			}
			else
			{
				_baseCategories = new List<MembersCategory>();

				Func<string, float,  MembersCategory> newCategory = (path, order) =>
					new MembersCategory(path, new List<MemberInfo>(), order, id, prefs);

				if (basic || (!minimal && !full && EditorView == 1))
				{
					var c = newCategory(string.Empty, 0f);
					c.Indent = false;
					c.HideHeader = true;
					_visibleMembers.Foreach(c.AddMember);
					_baseCategories.Add(c);
				}
				else
				{
					var resolver = new MembersResolution();

					Action<DefineCategoryAttribute, MembersCategory> resolve = (def, category) =>
						resolver.Resolve(_visibleMembers, def).Foreach(category.AddMember);

					var multiple	= targetType.GetCustomAttribute<DefineCategoriesAttribute>(true);
					var ignored		= targetType.GetCustomAttributes<IgnoreCategoriesAttribute>(true);
					var definitions = targetType.GetCustomAttributes<DefineCategoryAttribute>(true);
					if (multiple != null)
						definitions = definitions.Concat(multiple.names.Select(n => new DefineCategoryAttribute(n, 1000)));

					definitions = definitions.Where(d => !ignored.Any(ig => ig.Paths.Contains(d.FullPath)))
											 .ToList();

					Func<string, string[]> ParseCategoryPath = fullPath =>
					{
						int nPaths = fullPath.Split('/').Length;
						string[] result = new string[nPaths];
						for (int i = 0, index = -1; i < nPaths - 1; i++)
						{
							index = fullPath.IndexOf('/', index + 1);
							result[i] = fullPath.Substring(0, index);
						}
						result[nPaths - 1] = fullPath;
						return result;
					};

					// Order by exclusivity then path lengths
					var defs = from d in definitions
							   let paths = ParseCategoryPath(d.FullPath)
							   orderby !d.Exclusive//, paths.Length
							   select new { def = d, paths };

					// Parse paths and resolve definitions
					var categories = new Dictionary<string, MembersCategory>();
					foreach (var x in defs)
					{
						var paths = x.paths;
						var d = x.def;

						MembersCategory parent = null;

						for (int i = 0; i < paths.Length; i++)
						{
							var p = paths[i];

							var current = (parent == null ?
								_baseCategories : parent.NestedCategories).FirstOrDefault(c => c.FullPath == p);

							if (current == null)
							{
								current = newCategory(p, d.DisplayOrder);
								if (i == 0)
									_baseCategories.Add(current);
								if (parent != null)
									parent.NestedCategories.Add(current);
							}
							categories[p] = current;
							parent = current;
						}

						categories[paths.Last()].ForceExpand = d.ForceExpand;
						resolve(d, categories[paths.Last()]);
						categories.Clear();
						parent.Members = parent.Members.OrderBy(getDisplayOrder).ToList();
					}
					_baseCategories = _baseCategories.OrderBy(x => x.DisplayOrder).ToList();
				}
			}

			//_script = serializedObject.FindProperty("m_Script");

			var disInt = prefs.Ints.ValueOrDefault(id + "display", -1);
			_display = disInt == -1 ? defaultDisplay : (MembersDisplay)disInt;

			var field = targetType.Field("_serializationData", Flags.InstancePrivate);
			if (field == null) throw new MemberNotFoundException("_serializationData");

			_serializationData = new EditorMember(field, target, target, id);

			field = targetType.Field("dbg", Flags.InstanceAnyVisibility);
			if (field == null) throw new MemberNotFoundException("dbg");

			_debug = new EditorMember(field, target, target, id);

			OnAfterInitialized();
		}

		private void GUICode()
		{
			#if PROFILE
			Profiler.BeginSample(targetType.Name + " OnInspectorGUI");
			Profiler.BeginSample(targetType.Name + " Header");
			#endif

			if (ShowHeader)
			{
				string scriptKey = id + "script";
				gui.Space(3f);
				using (gui.Horizontal(EditorStyles.toolbarButton))
				{
					gui.Space(10f);
					foldouts[scriptKey] = gui.Foldout(foldouts[scriptKey]);
					gui.Space(-12f);

					if (ScriptField()) // script changed? exit!
						return;
				}

				if (foldouts[scriptKey])
				{
					gui.Space(2f);

					using (gui.Indent(GUI.skin.textField))
					{
						gui.Space(3f);
						if (targetType.IsDefined<HasRequirementsAttribute>())
						{
							using (gui.Horizontal())
							{
								gui.Space(3f);
								if (gui.MiniButton("Resolve Requirements", (Layout)null))
									Requirements.Resolve(target, gameObject);
							}
						}

						gui.Member(_debug);

						var mask = gui.BunnyMask("Display", _display);
						{
							var newValue = (MembersDisplay)mask;
							if (_display != newValue)
							{
								_display = newValue;
								prefs.Ints[id + "display"] = mask;
							}
						}

						gui.Member(_serializationData, true);
					}
				}
			}

			#if PROFILE
			Profiler.EndSample();
			#endif

			gui.BeginCheck();

			#if PROFILE
			Profiler.BeginSample(targetType.Name + " Members");
			#endif

			if (_minimalView)
			{
				gui.Space(4f);
				for (int i = 0; i < _visibleMembers.Count; i++)
				{
					var vim = _visibleMembers[i];

					bool changedTarget;
					if (_prevTarget != target)
					{
						_prevTarget = target;
						changedTarget = true;
					}
					else changedTarget = false;

					if (MemberUtil.IsVisible(vim, target, changedTarget))
						gui.Member(vim, target, target, id, false);
				}
			}
			else
			{
				for (int i = 0; i < _baseCategories.Count; i++)
				{
					var cat     = _baseCategories[i];
					cat.Display = _display;
					cat.gui     = gui;
					cat.Draw(target);
				}
			}

			OnGUI();

			#if PROFILE
			Profiler.EndSample();
			#endif

			if (gui.HasChanged())
			{
				//Log("setting dirty " + target);
				EditorUtility.SetDirty(target);
			}

			#if PROFILE
			Profiler.EndSample();
			#endif

			// Fixes somes cases of editor slugishness
			if (_repaintCount < 2)
			{
				_repaintCount++;
				Repaint();
			}
		}

		private bool ScriptField()
		{
			serializedObject.Update();

			_script = serializedObject.FindProperty("m_Script");

			EditorGUI.BeginChangeCheck();
			_script.objectReferenceValue = gui.Object("Script", _script.objectReferenceValue, typeof(MonoScript), false);
			if (EditorGUI.EndChangeCheck())
			{
				var sel = Selection.objects;
				Selection.objects = new UnityObject[0];
				EditorApplication.delayCall += () => Selection.objects = sel;
				serializedObject.ApplyModifiedProperties();
				return true;
			}

			return false;
		}

	}

	public static class MenuItems
	{
		[MenuItem("Tools/Vexe/GUI/UseUnityGUI")]
		public static void UseUnityGUI()
		{
			BetterPrefs.EditorInstance.Bools["UnityGUI"] = true;
		}

		[MenuItem("Tools/Vexe/GUI/UseRabbitGUI")]
		public static void UseRabbitGUI()
		{
			BetterPrefs.EditorInstance.Bools["UnityGUI"] = false;
		}
	}
}
