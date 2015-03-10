using System.Text.RegularExpressions;
using Fasterflect;
using UnityEngine;
using Vexe.Editor.Helpers;
using Vexe.Runtime.Extensions;
using Vexe.Runtime.Types;
using System.Linq;

namespace Vexe.Editor.Drawers
{
	public class AnimatorVariableAttributeDrawer : AttributeDrawer<string, AnimVarAttribute>
	{
		private string[] variables;
		private int current;

		private Animator _animator;
		private Animator animator
		{
			get
			{
				if (_animator == null)
				{
					string getter = attribute.GetAnimator;
					if (getter.IsNullOrEmpty())
					{
						_animator = gameObject.GetComponent<Animator>();
					}
					else
					{
						_animator = targetType.GetMethod(getter, Flags.InstanceAnyVisibility)
											  .Invoke(rawTarget, null) as Animator;
					}
				}
				return _animator;
			}
		}

		protected override void OnSingleInitialization()
		{
			if (memberValue == null)
				memberValue = "";

			if (animator != null && animator.runtimeAnimatorController != null)
				FetchVariables();
		}

		private void FetchVariables()
		{
#if UNITY_5
			variables = animator.parameters.Select(x => x.name).ToArray();
#else
			variables = EditorHelper.GetAnimatorVariableNames(animator);
#endif
			if (variables.IsEmpty())
				variables = new[] { "N/A" };
			else
			{
				if (!attribute.AutoMatch.IsNullOrEmpty())
				{
					string match = niceName.Remove(niceName.IndexOf(attribute.AutoMatch));
					match = Regex.Replace(match, @"\s+", "");
					if (variables.ContainsValue(match))
						memberValue = match;
				}
				current = variables.IndexOfZeroIfNotFound(memberValue);
			}
		}

		public override void OnGUI()
		{
			if (animator == null || animator.runtimeAnimatorController == null)
			{
				memberValue = gui.Text(niceName, memberValue);
			}
			else
			{
				if (variables.IsNullOrEmpty())
				{
					FetchVariables();
				}

				var x = gui.Popup(niceName, current, variables);
				{
					if (current != x || memberValue != variables[x])
						memberValue = variables[current = x];
				}
			}
		}
	}
}