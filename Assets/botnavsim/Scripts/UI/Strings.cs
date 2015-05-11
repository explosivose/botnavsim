using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

/// <summary>
/// A utility class for strings used in this project.
/// </summary>
public static class Strings  {

	/// <summary>
	/// The project title.
	/// </summary>
	public const string projectTitle = "BotNavSim";
	
	/// <summary>
	/// The project version.
	/// </summary>
	public const string projectVersion = "v0.4.4-dev";
	
	/// <summary>
	/// Project summary.
	/// </summary>
	public const string projectAbout = 
		"BotNavSim is a master's research project exploring the use of Unity3D for " + 
		"developing robot simulations. The project is developed at Loughborough University " + 
		"by undergraduate Matt Blickem.";
	
	
	/// <summary>
	/// The csv delimiter character. Avoid using comma because some
	/// data is serialized to include commas, and LogLoader cannot yet
	/// handle cases where commas are not just a delimiter character.
	/// </summary>
	public const char csvDelimiter = '\t';
	
	/// <summary>
	/// The csv comment character denotes lines which are not CSV data. 
	/// </summary>
	public const string csvComment = "#";
	
	/// <summary>
	/// The csv xml comment tag identifies which line the associated XML
	/// settings file name is stored. 
	/// </summary>
	public const string csvXmlCommentTag = " XML:";
	
	/// <summary>
	/// Gets the log file directory.
	/// </summary>
	/// <value>The log file directory.</value>
	public static string logFileDirectory {
		get { return System.Environment.CurrentDirectory + "\\Logs"; }
	}
	
	/// <summary>
	/// Gets the navigation plugin directory.
	/// </summary>
	/// <value>The navigation plugin directory.</value>
	public static string navigationPluginDirectory {
		get { return System.Environment.CurrentDirectory + "\\INavigation"; }
	} 

	/// <summary>
	/// Gets the invalid file name chars.
	/// </summary>
	/// <value>The invalid file name chars.</value>
	public static char[] invalidFileNameChars {
		get {
			if (_invalidFileNameChars == null) {
				_invalidFileNameChars = System.IO.Path.GetInvalidFileNameChars();
			}
			return _invalidFileNameChars;
		}
	}
	
	/// <summary>
	/// Gets the invalid path chars.
	/// </summary>
	/// <value>The invalid path chars.</value>
	public static char[] invalidPathChars {
		get {
			if (_invalidPathChars == null) {
				_invalidPathChars = System.IO.Path.GetInvalidPathChars();
			}
			return _invalidPathChars;
		}
	}
	
	private static char[] _invalidFileNameChars;
	private static char[] _invalidPathChars;

	/// <summary>
	/// Gets the newline. 
	/// </summary>
	/// <value>The newline.</value>
	public static string newline {
		get { return System.Environment.NewLine; }
	}

	/// <summary>
	/// Determines if is digits only the specified str.
	/// </summary>
	/// <returns><c>true</c> if is digits only the specified str; otherwise, <c>false</c>.</returns>
	/// <param name="str">String.</param>
	public static bool IsDigitsOnly(string str) {
		foreach (char c in str) {
			if (c < '0' || c > '9') {
				return false;
			}
		}
		return true;
	}
	
}
