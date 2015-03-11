using UnityEngine;
using System.Collections;

public static class Strings  {

	public const string projectTitle = "BotNavSim";
	public const string projectVersion = "v0.4.0-dev";
	
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
	
	public static string logFileDirectory = System.Environment.CurrentDirectory + "\\Logs";
	public static string simulationFileDirectory = System.Environment.CurrentDirectory + "\\Simulations";
	
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
