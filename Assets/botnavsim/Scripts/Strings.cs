using UnityEngine;
using System.Collections;

public class Strings  {

	public static bool IsDigitsOnly(string str) {
		foreach (char c in str) {
			if (c < '0' || c > '9') {
				return false;
			}
		}
		return true;
	}
}
