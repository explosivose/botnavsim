using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fasterflect;
using UnityEngine;
using Vexe.Runtime.Extensions;
using Vexe.Runtime.Helpers;
using Vexe.Runtime.Types;
using UnityObject = UnityEngine.Object;

namespace Vexe.Runtime.Serialization
{
	public class SerializationLogic
	{
		protected readonly SerializationAttributes attributes;

		private static SerializationLogic defaultLogic;
		public static SerializationLogic Default
		{
			get { return defaultLogic ?? (defaultLogic = new SerializationLogic(SerializationAttributes.Default)); }
		}

		public SerializationAttributes Attributes { get { return attributes; } }

		public SerializationLogic(SerializationAttributes attributes)
		{
			this.attributes = attributes;
		}

		private Func<Type, List<RuntimeMember>> getMemoizedSerializableMembers;
		public Func<Type, List<RuntimeMember>> GetMemoizedSerializableMembers
		{
			get
			{
				return getMemoizedSerializableMembers ??
					(getMemoizedSerializableMembers = new Func<Type, List<RuntimeMember>>(type =>
					{
						return GetSerializableMembers(type, null).ToList();
					}).Memoize());
			}
		}

		public IEnumerable<RuntimeMember> GetSerializableMembers(Type type, object target)
		{
			var members = ReflectionUtil.GetMemoizedMembers(type);
			var serializable = members.Where(IsSerializable);
			var result = RuntimeMember.Enumerate(serializable, target);
			return result;
		}

		private Func<Type, List<MemberInfo>> getMemoizedVisibleMembers;
		public Func<Type, List<MemberInfo>> GetMemoizedVisibleMembers()
		{
			return getMemoizedVisibleMembers ?? (getMemoizedVisibleMembers = new Func<Type, List<MemberInfo>>(type =>
						GetVisibleMembers(ReflectionUtil.GetMemoizedMembers(type)).ToList()).Memoize());
		}

		public IEnumerable<MemberInfo> GetVisibleMembers(IEnumerable<MemberInfo> fromMembers)
		{
			Func<MemberInfo, float> FieldsThenPropsThenMethods = member =>
			{
				var attrib = member.GetCustomAttribute<DisplayOrderAttribute>();
				if (attrib != null)
					return attrib.displayOrder;

				switch (member.MemberType)
				{
					case MemberTypes.Field    : return 1000f;
					case MemberTypes.Property : return 2000f;
					case MemberTypes.Method   : return 3000f;
					default : throw new NotSupportedException();
				}
			};

			return fromMembers.Where(SerializationLogic.Default.IsVisibleMember)
							  .OrderBy<MemberInfo, float>(FieldsThenPropsThenMethods);
							  //.ThenBy(m => m.GetDataType().Name)
							  //.ThenBy(m => m.Name);
		}

		/// <summary>
		/// A member is visible if it was serializable or had any of the exposure attributes defined on it
		/// </summary>
		public bool IsVisibleMember(MemberInfo member)
		{
			if (member is MethodInfo)
				return attributes.Exposure.Any(member.IsDefined);

			var field = member as FieldInfo;
			if (field != null)
				return !attributes.Hide.Any(field.IsDefined)
					&& (IsSerializable(field) || attributes.Exposure.Any(field.IsDefined));

			var property = member as PropertyInfo;
			if (property == null || attributes.Hide.Any(property.IsDefined))
				return false;

			var declType = property.DeclaringType;
			bool isValidUnityType = declType.IsA<UnityObject>() && !declType.IsA<MonoBehaviour>() && !declType.IsA<ScriptableObject>();
			bool unityProp = property.CanReadWrite() && isValidUnityType; // ex transform.position, rigidbody.mass, etc exposing unity properties is useful when inlining objects via [Inline]
			if (unityProp) return true;

			bool serializable = IsSerializable(property);
			if (serializable) return true;

			return attributes.Exposure.Any(property.IsDefined);
		}

		private Func<MemberInfo, bool> _memoizedIsVisibleMember;
		public Func<MemberInfo, bool> MemoizedIsVisibleMember
		{
			get
			{
				return _memoizedIsVisibleMember ?? (_memoizedIsVisibleMember = new Func<MemberInfo, bool>(IsVisibleMember).Memoize());
			}
		}

		public bool IsSerializable(MemberInfo member)
		{
			if (member.MemberType == MemberTypes.Method)
				return false;

			var field = member as FieldInfo;
			if (field != null)
				return IsSerializable(field);

			var prop = member as PropertyInfo;
			if (prop != null)
				return IsSerializable(prop);

			return false;
		}

		public bool IsSerializable(RuntimeMember member)
		{
			var field = (FieldInfo)member;
			return field != null ? IsSerializable(field) : IsSerializable((PropertyInfo)member);
		}

		public bool IsSerializable(Type type)
		{
			if (IsSimpleType(type)
				|| type.IsA<UnityObject>()
				|| UnityStructs.ContainsValue(type))
				return true;

			if (type.IsArray)
				return type.GetArrayRank() == 1 && IsSerializable(type.GetElementType());

			if (type.IsInterface)
				return true;

			if (NotSupportedTypes.Any(type.IsA))
				return false;

			if (SupportedTypes.Any(type.IsA))
				return true;

			if (type.IsGenericType)
				return type.GetGenericArguments().All(IsSerializable);

			return attributes.SerializableType.IsNullOrEmpty() || attributes.SerializableType.Any(type.IsDefined);
		}

		public bool IsSerializable(FieldInfo field)
		{
			if (attributes.DontSerializeMember.Any(field.IsDefined))
				return false;

			if (field.IsLiteral)
				return false;

			if (!(field.IsPublic || attributes.SerializeMember.Any(field.IsDefined)))
				return false;

			bool serializable = IsSerializable(field.FieldType);
			return serializable;
		}

		public bool IsSerializable(PropertyInfo property)
		{
			if (attributes.DontSerializeMember.Any(property.IsDefined))
				return false;

			if (!property.IsAutoProperty())
				return false;

			if (!(property.GetGetMethod(true).IsPublic ||
				  property.GetSetMethod(true).IsPublic ||
				  attributes.SerializeMember.Any(property.IsDefined)))
				return false;

			bool serializable = IsSerializable(property.PropertyType);
			return serializable;
		}

		public static readonly Type[] UnityStructs =
		{
			typeof(Vector3),
			typeof(Vector2),
			typeof(Vector4),
			typeof(Rect),
			typeof(Quaternion),
			typeof(Matrix4x4),
			typeof(Color),
			typeof(Color32),
			typeof(LayerMask),
			typeof(Bounds)
		};

		public static readonly Type[] NotSupportedTypes =
		{
			typeof(Delegate)
		};

		public static readonly Type[] SupportedTypes =
		{
			typeof(Type)
		};

		private static bool IsSimpleType(Type type)
		{
			return type.IsPrimitive || type.IsEnum || type == typeof(string);
		}
	}
}

namespace Vexe.Runtime.Helpers
{
	public static class ReflectionUtil
	{
		private static BindingFlags DefaultFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

		public static IEnumerable<MemberInfo> GetMembers(Type type)
		{
			var peak = type.IsA<BetterBehaviour>() ? typeof(BetterBehaviour) : type.IsA<BetterScriptableObject>() ? typeof(BetterScriptableObject) : typeof(object);
			var members = type.GetAllMembers(peak, DefaultFlags);
			return members;
		}

		private static Func<Type, List<MemberInfo>> getMemoizedMembers;
		public static Func<Type, List<MemberInfo>> GetMemoizedMembers
		{
			get { return getMemoizedMembers ?? (getMemoizedMembers = new Func<Type, List<MemberInfo>>(type => GetMembers(type).ToList()).Memoize()); }
		}

		private static Func<Tuple<Type, string>, MemberInfo> _getMemoizedMember;
		private static Func<Tuple<Type,string>, MemberInfo> getMemoizedMember
		{
			get
			{
				return _getMemoizedMember ?? (_getMemoizedMember = new Func<Tuple<Type, string>, MemberInfo>(tup =>
				{
					var members = tup.Item1.GetMember(tup.Item2, Flags.StaticInstanceAnyVisibility);
					if (members.IsNullOrEmpty())
						return null;
					return members[0];
				}).Memoize());
			}
		}

		public static MemberInfo GetMemoizedMember(Type objType, string memberName)
		{
			return getMemoizedMember(Tuple.Create<Type, string>(objType, memberName));
		}
	}
}
