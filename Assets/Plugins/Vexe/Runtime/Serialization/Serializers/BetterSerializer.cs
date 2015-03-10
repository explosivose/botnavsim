#define PROFILE

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using FullSerializer;
using UnityEngine;
using Vexe.Runtime.Extensions;
using Vexe.Runtime.Types;
using UnityObject = UnityEngine.Object;

namespace Vexe.Runtime.Serialization
{
	public class BetterSerializer
	{
		public SerializationLogic Logic { set; get; }

		private fsSerializer serializer;

		public BetterSerializer()
		{
			serializer = new fsSerializer();
			serializer.AddConverter<UnityObjectConverter>();

			Logic = SerializationLogic.Default;

			fsSerializer.Config = new fsConfig(
				Logic.Attributes.SerializeMember,
				Logic.Attributes.DontSerializeMember,
				false, Debug.Log, true
			);
		}

		public void SerializeTargetIntoData(object target, SerializationData data)
		{
#if PROFILE
			Profiler.BeginSample("Getting members for: " + target);
#endif
			var members = Logic.GetMemoizedSerializableMembers(target.GetType());
#if PROFILE
			Profiler.EndSample();
#endif
			for (int i = 0; i < members.Count; i++)
			{
				var member    = members[i];
				member.Target = target;
				var value     = member.Value;

				if (value.IsObjectNull())
					continue;

				try
				{
					string memberKey = GetMemberKey(member);
#if PROFILE
					Profiler.BeginSample("Serializing: " + member.Name);
#endif
					string serializedState = Serialize(member.Type, value, data.serializedObjects);
#if PROFILE
					Profiler.EndSample();
#endif
					data.serializedStrings[memberKey] = serializedState;
				}
				catch (Exception e)
				{
					Debug.LogError("Error serializing {0} in {1}. Msg: {2}. Stack: {3}".FormatWith(member.Name, target, e.Message, e.StackTrace));
				}
			}
		}

		public void DeseiralizeDataIntoTarget(object target, SerializationData data)
		{
			var members = Logic.GetMemoizedSerializableMembers(target.GetType());
			for(int i = 0; i < members.Count; i++)
			{
				var member    = members[i];
				var memberKey = GetMemberKey(member);
				member.Target = target;

				try
				{
					string result;
					if (data.serializedStrings.TryGetValue(memberKey, out result))
					{
						var value = Deserialize(member.Type, result, data.serializedObjects);
						member.Value = value;
					}
				}
				catch (Exception e)
				{
					Debug.LogError("Error deserializing member {0} in {1}. Msg: {2}. Stack: {3}".FormatWith(member.Name, target, e.Message, e.StackTrace));
				}
			}
		}

		private Func<RuntimeMember, string> getMemberKey;
		private Func<RuntimeMember, string> GetMemberKey
		{
			get
			{
				return getMemberKey ?? (getMemberKey = new Func<RuntimeMember, string>(member =>
					string.Format("{0}: {1} {2}", member.MemberType.ToString(), member.TypeNiceName, member.Name)
				).Memoize());
			}
		}

		public string Serialize(Type type, object graph, object context)
		{
			// 0- set context
			serializer.Context.Set(context as List<UnityObject>);

			// 1- serialize the data
			fsData data;
			var fail = serializer.TrySerialize(type, graph, out data);
			if (fail.Failed) throw new Exception(fail.FailureReason);

			// 2- emit the data via JSON
			return fsJsonPrinter.CompressedJson(data);
		}

		public string Serialize(object graph, object context)
		{
			return Serialize(graph.GetType(), graph, context);
		}
		
		public string Serialize(object graph)
		{
			return Serialize(graph, null);
		}

		public object Deserialize(Type type, string serializedState, object context)
		{
			// 0- parse the JSON data
			fsData data;
			fsStatus status = fsJsonParser.Parse(serializedState, out data);
			if (status.Failed) throw new Exception(status.FailureReason);

			// 1- set context
			serializer.Context.Set(context as List<UnityObject>);

			// 2- deserialize the data
			object deserialized = null;
			status = serializer.TryDeserialize(data, type, ref deserialized);
			if (status.Failed) throw new Exception(status.FailureReason);
			return deserialized;
		}

		public T Deserialize<T>(string serializedState, object context)
		{
			return (T)Deserialize(typeof(T), serializedState, context);
		}

		public T Deserialize<T>(string serializedState)
		{
			return Deserialize<T>(serializedState, null);
		}

		public object Copy(object obj)
		{
			if (obj == null) return null;
			Type type = obj.GetType();
			string serialized = Serialize(type, obj, null);
			object copy = Deserialize(type, serialized, null);
			return copy;
		}
	}
}
