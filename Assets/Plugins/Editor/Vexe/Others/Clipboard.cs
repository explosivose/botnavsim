using System;
using UnityEngine;
using Vexe.Runtime.Types;

namespace Vexe.Editor.Helpers
{
	[Serializable, BasicView]
	public class Clipboard : BetterScriptableObject
	{
		private static string instancePath;

		static Clipboard()
		{
			instancePath = EditorHelper.ScriptableAssetsPath + "/Clipboard.asset";

		}

		private static Clipboard instance;
		public static Clipboard Instance
		{
			get { return EditorHelper.LazyLoadScriptableAsset<Clipboard>(ref instance, instancePath, true); }
		}

		public Vector3 Vector3       { get; set; }
		public Vector2 Vector2       { get; set; }
		public int Int               { get; set; }
		public float Float           { get; set; }
		public string String         { get; set; }
		public bool Bool             { get; set; }
		public Color Color           { get; set; }
		public Quaternion Quaternion { get; set; }
	}
}