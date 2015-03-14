using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using Vexe.Runtime.Extensions;
using Vexe.Runtime.Types;
using UnityObject = UnityEngine.Object;

namespace Vexe.Editor
{ 
	public class EditorMember : RuntimeMember
	{
		public UnityObject UnityTarget { get; set; }

		public string Id { get; protected set; }

		public EditorMember(MemberInfo member, object rawTarget, UnityObject unityTarget, string id) : base(member, rawTarget)
		{
			this.UnityTarget = unityTarget;
			if (id != null)
				this.Id = id + TypeNiceName + NiceName;
		}

		public override void Set(object value)
		{
			bool sameValue = value.GenericEqual(this.Value);
			if (sameValue)
				return;

			if (UnityTarget != null)
				Undo.RecordObject(UnityTarget, "member set");

			base.Set(value);

			if (UnityTarget != null)
				EditorUtility.SetDirty(UnityTarget);
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var member = obj as EditorMember;
			return member != null && member.Id == Id;
		}
	}

	public static class EditorMemberExtensions
	{
		public static bool IsNull(this EditorMember member)
		{
			object value;
			return (member == null || member.Equals(null)) || ((value = member.Value) == null || value.Equals(null));
		}
	}

	public class ArgMember : EditorMember
	{
		private readonly Func<object> getter;
		private readonly Action<object> setter;

		public ArgMember(Func<object> getter, Action<object> setter, object target, UnityObject unityTarget, Type dataType, Attribute[] attributes, string name, string id)
			: base(null, null, unityTarget, null)
		{
			this.getter      = getter;
			this.setter      = setter;
			this.Target      = target;
			this.attributes  = attributes;
			this.Name        = name;
			this.Type        = dataType;
			this.Id			 = id + name;
		}

		public override object Get()
		{
			return getter();
		}

		public override void Set(object value)
		{
			setter(value);
		}
	}

	public class ElementMember<T> : ArgMember
	{
		public IList<T> List { get; set; }
		public int Index     { get; set; }
		public bool Readonly { get; set; }

		public ElementMember(Attribute[] attributes, string name, string id)
			: base(null, null, null, null, typeof(T), attributes, name, id)
		{
		}

		public void Initialize(IList<T> list, int idx, object rawTarget, UnityObject unityTarget)
		{
			this.List = list;
			this.Index = idx;
			this.Target = rawTarget;
			this.UnityTarget = unityTarget;
		}

		public override object Get()
		{
			return List[Index];
		}

		public override void Set(object value)
		{
			if (!Readonly)
				List[Index] = (T)value;
		}

		public override string ToString()
		{
			return base.ToString() + Index;
		}
	}
}
