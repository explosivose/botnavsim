//#define DBG

using System;
using UnityEngine;
using Vexe.Runtime.Extensions;
using Vexe.Runtime.Serialization;

namespace Vexe.Runtime.Types
{
	[DefineCategory("Fields", 0f, MemberType = MemberType.Field, Exclusive = false)]
	[DefineCategory("Properties", 1f, MemberType = MemberType.Property, Exclusive = false)]
	[DefineCategory("Methods", 2f, MemberType = MemberType.Method, Exclusive = false)]
	public abstract class BetterScriptableObject : ScriptableObject, ISerializable, IHasUniqueId, ISerializationCallbackReceiver
	{
		[SerializeField, DontSave]
		private SerializationData _serializationData;
		private SerializationData SerializationData
		{
			get { return _serializationData ?? (_serializationData = new SerializationData()); }
		}

		private BetterSerializer _serializer;
		public BetterSerializer Serializer
		{
			get { return _serializer ?? (_serializer = new BetterSerializer()); }
		}

		[SerializeField, HideInInspector, DontSave]
		private string id;
		public string ID
		{
			get
			{
				if (id.IsNullOrEmpty())
					id = Guid.NewGuid().ToString();
				return id;
			}
		}

		public void OnBeforeSerialize()
		{
			Serialize();
		}

		public void OnAfterDeserialize()
		{
			Deserialize();
		}

		public void Serialize()
		{
#if DBG
			Log("Saving " + GetType().Name);
#endif
			SerializationData.Clear();
			Serializer.SerializeTargetIntoData(this, SerializationData);
		}

		public void Deserialize()
		{
#if DBG
			Log("Loading " + GetType().Name);
#endif
			Serializer.DeseiralizeDataIntoTarget(this, SerializationData);
		}

		public bool dbg;
		protected void dbgLog(string msg, params object[] args)
		{
			if (dbg) Log(msg, args);
		}

		protected void dbgLog(object obj)
		{
			if (dbg) Log(obj);
		}

		protected static void Log(string msg, params object[] args)
		{
			Debug.Log(string.Format(msg, args));
		}

		protected static void Log(object obj)
		{
			Debug.Log(obj);
		}

		public virtual void Reset()
		{
			RuntimeUtils.ResetTarget(this);
		}
	}
}