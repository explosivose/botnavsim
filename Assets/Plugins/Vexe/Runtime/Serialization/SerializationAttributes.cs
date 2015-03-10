using System;
using UnityEngine;
using Vexe.Runtime.Types;

#pragma warning disable 0618

namespace Vexe.Runtime.Serialization
{
	public class SerializationAttributes
	{
		public Type[] SerializeMember     { get; set; }
		public Type[] DontSerializeMember { get; set; }
		public Type[] SerializableType    { get; set; }
		public Type[] Exposure            { get; set; }
		public Type[] Hide                { get; set; }

		private static SerializationAttributes defaultAttributes;
		public static SerializationAttributes Default
		{
			get
			{
				return defaultAttributes ?? (defaultAttributes = new SerializationAttributes
				{
					SerializeMember = new[]
					{
						typeof(SerializeField),
						typeof(SerializeAttribute),
						typeof(SaveAttribute)
					},

					DontSerializeMember = new[]
					{
						typeof(NonSerializedAttribute),
						typeof(DontSerializeAttribute),
						typeof(DontSaveAttribute)
					},

					// since we're only using FullSerializer now, we don't need to annotate types with special attributes to serialize them
					//SerializableType = new[]
					//{
						//typeof(SerializableAttribute)
					//},

					Exposure = new[]
					{
						typeof(ShowAttribute)
					},

					Hide = new[]
					{
						typeof(HideInInspector),
						typeof(HideAttribute)
					},
				});
			}
		}
	}
}