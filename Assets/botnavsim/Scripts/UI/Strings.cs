using UnityEngine;
using System.Collections;

public static class Strings  {

	public const string projectTitle = "BotNavSim";
	public const string projectVersion = "v0.4.0-dev";
	
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
