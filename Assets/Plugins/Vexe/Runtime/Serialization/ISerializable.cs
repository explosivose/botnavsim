using System;
using Vexe.Runtime.Serialization;

namespace Vexe.Runtime.Types
{
	public interface ISerializable
	{
		void Serialize();
		void Deserialize();
	}
}