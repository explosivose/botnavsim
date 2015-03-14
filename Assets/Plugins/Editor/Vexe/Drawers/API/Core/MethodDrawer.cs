//#define DBG

using System;
using System.Reflection;
using Fasterflect;
using Vexe.Editor.GUIs;
using Vexe.Runtime.Extensions;
using Vexe.Runtime.Types;
using UnityObject = UnityEngine.Object;

namespace Vexe.Editor.Drawers
{
	public class MethodDrawer
	{
		private ArgMember[] argMembers;
		private MethodInvoker invoke;
		private MethodInfo isVisibleInfo;
		private object[] argValues;
		private string[] argKeys;
		private string niceName;
		private bool initialized;

		private object rawTarget;
		private string id;
		private BaseGUI gui;

		private BetterPrefs prefs { get { return BetterPrefs.EditorInstance; } }

		private bool foldout
		{
			get { return prefs.Bools.ValueOrDefault(id); }
			set { prefs.Bools[id] = value; }
		}

		public void Initialize(MethodInfo method, object rawTarget, UnityObject unityTarget, string id, BaseGUI gui)
		{
			this.gui = gui;
			this.rawTarget = rawTarget;
			this.id = id;

			if (initialized) return;
			initialized = true;

			niceName = method.GetNiceName();

			invoke	     = method.DelegateForCallMethod();
			var argInfos = method.GetParameters();
			int len      = argInfos.Length;
			argValues    = new object[len];
			argKeys      = new string[len];
			argMembers   = new ArgMember[len];

			for (int iLoop = 0; iLoop < len; iLoop++)
			{
				int i = iLoop;
				var argInfo = argInfos[i];
				var argType = argInfo.ParameterType;

				argKeys[i] = id + argType.FullName + argInfo.Name;

				argValues[i] = prefs.ValueOrDefault(argInfos[i].ParameterType, argKeys[i]);

				argMembers[i] = new ArgMember(
						@getter      : () => argValues[i],
						@setter      : x => argValues[i] = x,
						@target	     : rawTarget,
						@unityTarget : unityTarget,
						@attributes  : argInfo.GetCustomAttributes(true) as Attribute[],
						@name        : argInfo.Name,
						@id          : argKeys[i],
						@dataType    : argType
					);
			}

#if DBG
			Log("Method drawer init");
#endif
		}

		public bool OnGUI()
		{
			bool changed = false;
			if (Header() && argMembers.Length > 0)
			{
				using (gui.Indent())
				{
					for (int i = 0; i < argMembers.Length; i++)
					{
						bool argChange = gui.Member(argMembers[i], false);
						changed |= argChange;
						if (argChange)
							prefs.TryAdd(argValues[i], argKeys[i]);
					}
				}
			}
			return changed;
		}

		private bool Header()
		{
			using (gui.Horizontal())
			{
				if (gui.Button(niceName, styles.Mini))
					invoke(rawTarget, argValues);

				gui.Space(12f);
				if (argMembers.Length > 0)
				{ 
					foldout = gui.Foldout(foldout);
					gui.Space(-11.5f);
				}
			}
			return foldout;
		}
	}
}