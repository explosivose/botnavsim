using System;

namespace Vexe.Runtime.Types
{
	/// <summary>
	/// Annotate a BetterBehaviour with this attirbute to ignore all defined categories
	/// and display all visible members in a non-categorized manner
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = true)]
	public class BasicViewAttribute : Attribute
	{
	}

	/// <summary>
	/// Annotate a BetterBehaviour with this attribute to draw in the most minimal way possible
	/// Very default Unity editor-like view
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = true)]
	public class MinimalViewAttribute : Attribute
	{
	}

	/// <summary>
	/// Takes defined categories into consideration
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = true)]
	public class FullViewAttribute : Attribute
	{
	}
}
