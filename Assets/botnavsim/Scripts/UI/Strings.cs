using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public static class Strings  {

	public const string projectTitle = "BotNavSim";
	public const string projectVersion = "v0.4.2-dev";
	
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
	
	public static string logFileDirectory {
		get { return System.Environment.CurrentDirectory + "\\Logs"; }
	}
	public static string navigationPluginDirectory {
		get { return System.Environment.CurrentDirectory + "\\INavigation"; }
	} 
	public static string creditDirectory {
		get { return System.Environment.CurrentDirectory + "\\Credits"; }
	} 
	
	public static char[] invalidFileNameChars {
		get {
			if (_invalidFileNameChars == null) {
				_invalidFileNameChars = System.IO.Path.GetInvalidFileNameChars();
			}
			return _invalidFileNameChars;
		}
	}
	
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

	public static string newline {
		get { return System.Environment.NewLine; }
	}

	public static bool IsDigitsOnly(string str) {
		foreach (char c in str) {
			if (c < '0' || c > '9') {
				return false;
			}
		}
		return true;
	}
	
}
