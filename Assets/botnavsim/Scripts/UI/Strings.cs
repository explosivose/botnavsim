using UnityEngine;
using System.Collections;

public static class Strings  {

	public const string projectTitle = "BotNavSim";
	public const string projectVersion = "v0.2.0";
	
	
	public static bool IsDigitsOnly(string str) {
		foreach (char c in str) {
			if (c < '0' || c > '9') {
				return false;
			}
		}
		return true;
	}
	
}
