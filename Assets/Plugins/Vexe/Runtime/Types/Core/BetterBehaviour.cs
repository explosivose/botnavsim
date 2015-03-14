//#define DBG

using System;
using UnityEngine;
using Vexe.Runtime.Extensions;
using Vexe.Runtime.Serialization;

namespace Vexe.Runtime.Types
{
	/// <summary>
	/// Inherit from this instead of MonoBehaviour to live in a better world!
	/// </summary>
	[DefineCategory("Fields", 0f, MemberType = MemberType.Field, Exclusive = false)]
	[DefineCategory("Properties", 1f, MemberType = MemberType.Property, Exclusive = false)]
	[DefineCategory("Methods", 2f, MemberType = MemberType.Method, Exclusive = false)]
	[DefineCategory("Debug", 3f, Pattern = "^dbg")]
	public abstract class BetterBehaviour : MonoBehaviour, ISerializable, IHasUniqueId, ISerializationCallbackReceiver
	{
		/// <summary>
		/// Use this to include members to the "Debug" categories
		/// Ex:
		/// [Category(Dbg)]
		/// public Color gizmosColor;
		/// </summary>
		protected const string Dbg = "Debug";

		/// <summary>
		/// Used for debugging/logging
		/// </summary>
		[DontSerialize] public bool dbg;

		// SerializationData, Serializer
		#region
		/// <summary>
		/// It's important to let *only* unity serialize our serialization data
		/// </summary>
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

		#endregion

		// IHasUniqueId implementation
		#region
		/// <summary>
		/// A unique identifier used primarly from editor scripts to have editor data persist
		/// I've had some runtime usages for it too
		/// </summary>
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
		#endregion

		// [De]serialization
		#region
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
			Log("Serializing " + GetType().Name);
#endif
			SerializationData.Clear();
			Serializer.SerializeTargetIntoData(this, SerializationData);
		}

		public void Deserialize()
		{
#if DBG
			Log("Deserializing " + GetType().Name);
#endif
			Serializer.DeseiralizeDataIntoTarget(this, SerializationData);
		}
		#endregion

		// Logging
		#region
		protected void dbgLog(string msg, params object[] args)
		{
			if (dbg) Log(msg, args);
		}

		protected void dbgLog(object obj)
		{
			if (dbg) Log(obj);
		}

		protected void Log(string msg, params object[] args)
		{
			if (args.IsNullOrEmpty()) args = new object[0];
			Debug.Log(string.Format(msg, args), gameObject); // passing gameObject as context will ping the gameObject that we logged from when we click the log entry in the console!
		}

		protected void Log(object obj)
		{
			Log(obj.ToString(), null);
		}

		// static logs are useful when logging in nested system.object classes
		protected static void sLog(string msg, params object[] args)
		{
			if (args.IsNullOrEmpty()) args = new object[0];
			Debug.Log(string.Format(msg, args));
		}

		protected static void sLog(object obj)
		{
			Debug.Log(obj);
		}

		#endregion

		public virtual void Reset()
		{
			RuntimeUtils.ResetTarget(this);
		}
	}
}
