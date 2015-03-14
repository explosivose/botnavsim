//#define DBG

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Vexe.Runtime.Extensions;
using Vexe.Runtime.Types;
using UnityObject = UnityEngine.Object;

#pragma warning disable 0168

namespace Vexe.Editor.Drawers
{
	using System.Reflection;
using Fasterflect;
using Vexe.Editor.GUIs;
using Vexe.Runtime.Serialization;
using Editor = UnityEditor.Editor;

	public class InlineAttributeDrawer : CompositeDrawer<UnityObject, InlineAttribute>
	{
		private bool hideTarget;
		private List<bool> expandValues;

		// Add the types you want to support here...
		private Dictionary<Type, Action<UnityObject, BaseGUI>> customEditors;
		
		public InlineAttributeDrawer ()
		{
			customEditors = new Dictionary<Type, Action<UnityObject, BaseGUI>>
			{
				{
					typeof(Transform), (target, gui) =>
					{
						RecursiveDrawer.DrawRecursive(target, gui, id, target,
							"localPosition", "localRotation", "localScale");
					}
				},
				{
					typeof(Rigidbody), (target, gui) =>
					{
						RecursiveDrawer.DrawRecursive(target, gui, id, target,
							"mass", "drag", "angularDrag", "useGravity", "isKinematic", "interpolation", "collisionDetectionMode");

						using (gui.Indent())
						{ 
							var rb = target as Rigidbody;
							rb.constraints = (RigidbodyConstraints)gui.BunnyMask("Constraints", rb.constraints);
						}
					}
				},
				{
					typeof(BoxCollider), (target, gui) =>
					{
						RecursiveDrawer.DrawRecursive(target, gui, id, target,
							"isTrigger", "sharedMaterial", "center", "size");
					}
				},
				{
					typeof(SphereCollider), (target, gui) =>
					{
						RecursiveDrawer.DrawRecursive(target, gui, id, target,
							"isTrigger", "sharedMaterial", "center", "radius");
					}
				},
				{
					typeof(Animator), (target, gui) =>
					{
						RecursiveDrawer.DrawRecursive(target, gui, id, target,
							"runtimeAnimatorController", "avatar", "applyRootMotion", "updateMode", "cullingMode");
					}
				},
				{
					typeof(Camera), (target, gui) =>
					{
						RecursiveDrawer.DrawRecursive(target, gui, id, target,
							"clearFlags", "backgroundColor", "cullingMask", "nearClipPlane", "farClipPlane",
							"rect", "depth", "renderingPath", "targetTexture", "useOcclusionCulling", "hdr",
							"isOrthoGraphic");

						// TODO: improve cullingMask drawing. right now it's a float, gui.Layer doesn't cut it too

						using (gui.Indent())
						{
							var c = target as Camera;
							if (c.isOrthoGraphic)
								c.orthographicSize = gui.Float("Size", c.orthographicSize);
							else c.fieldOfView = gui.FloatSlider("FOV", c.fieldOfView, 1, 179);
						}
					}
				},
				{
					typeof(MeshRenderer), (target, gui) =>
					{
						RecursiveDrawer.DrawRecursive(target, gui, id, target,
							"castShadows", "receiveShadows", "sharedMaterials", "useLightProbes");
					}
				},
			};
		}

		public bool GuiBox { get; set; }

		public bool HideTarget
		{
			get { return hideTarget; }
			set
			{
				if (hideTarget != value)
				{
					memberValue.SetVisibility(!value);
					hideTarget = value;
				}
			}
		}

		protected override void OnSingleInitialization()
		{
			expandValues = new List<bool>();
			GuiBox       = attribute.GuiBox;
			HideTarget   = attribute.HideTarget;
#if DBG
			Log("Initialized InlineDrawer " + niceName);
#endif
		}

		public override void OnLeftGUI()
		{
			Foldout();
			gui.Space(-10f);
		}

		public override void OnLowerGUI()
		{
			var objRef = memberValue;
			if (foldout)
			{
				if (objRef == null)
				{
					gui.HelpBox("Please assign a target to inline");
					return;
				}

				var go = objRef as GameObject;
				if (go != null)
				{
					using (gui.Horizontal())
					{
						go.SetActive(gui.Toggle(string.Empty, go.activeSelf, Layout.sWidth(20f)));
						go.name = gui.Text(go.name);
					}

					using (gui.LabelWidth(45f))
					using (gui.Horizontal())
					{
						go.tag = gui.Tag("Tag", go.tag);
						gui.Space(5f);
						go.layer = gui.Layer("Layer", go.layer);
					}

					go.GetAllComponents().Foreach(DrawExpandableHeader);
				}
				else
				{
					using (gui.Vertical(GuiBox ? GUI.skin.box : GUIStyle.none))
					{
						DrawTarget(objRef);
					}
				}
			}
		}

		private void DrawTarget(UnityObject target)
		{
			Action<UnityObject, BaseGUI> draw;
			if (customEditors.TryGetValue(target.GetType(), out draw))
			{
				draw(target, gui);
			}
			else
			{
				RecursiveDrawer.DrawRecursive(target, gui, id, unityTarget);
			}
		}

		private void DrawExpandableHeader(Component target, int index)
		{
			bool expanded = gui.InspectorTitlebar(InternalEditorUtility.GetIsInspectorExpanded(target), target);

			bool previous;
			if (index >= expandValues.Count)
			{
				expandValues.Add(expanded);
				previous = !expanded;
			}
			else
			{
				previous = expandValues[index];
			}
			if (expanded != previous)
			{
				expandValues[index] = expanded;
				InternalEditorUtility.SetIsInspectorExpanded(target, expanded);
			}
			if (expanded)
			{
				gui.Space(5f);
				using (gui.Indent())
				{
					DrawTarget(target);
				}
			}
		}
	}
}
