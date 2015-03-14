using System;

namespace Vexe.Runtime.Types
{
	/// <summary>
	/// Use this attribute to undefine/ignore (essentially don't show) previously defined categories
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class IgnoreCategoriesAttribute : Attribute
	{
		private string[] paths;

		/// <summary>
		/// The full paths to the ignored categories
		/// </summary>
		public string[] Paths { get { return paths; } }

		public IgnoreCategoriesAttribute(params string[] paths)
		{
			this.paths = paths;
		}
	}
}