using System;
using UnityEngine;
using Vexe.Runtime.Extensions;

namespace Vexe.Editor.Drawers
{
	public abstract class ObjectDrawer<T> : BaseDrawer
	{
		protected T memberValue
		{
			get
			{
				try { 
				return (T)member.Value;
				}
				catch
				{
					//var target = member.Target;
					//var value1 = member.getter(member._target);
					//var value2 = member.getter(target);
					//var from = value2.GetType().Name;
					//Debug.LogError("From {0} to {1}".FormatWith(member.Type.Name, typeof(T).Name));
					throw;
				}
			}
			set { member.Value = value; }
		}

		public void MemberField()
		{
			MemberField(member);
		}

		public void MemberField(EditorMember dm)
		{
			gui.Member(dm, false);
		}

		public sealed override void OnLeftGUI()
		{
		}
		public sealed override void OnRightGUI()
		{
		}
		public sealed override void OnUpperGUI()
		{
		}
		public sealed override void OnLowerGUI()
		{
		}
		public sealed override void OnMemberDrawn(Rect area)
		{
		}
	}
}