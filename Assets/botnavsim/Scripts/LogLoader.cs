using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class LogLoader {

	
	/// <summary>
	/// Searchs for subfolders in a directory.
	/// </summary>
	/// <returns>List of subfolders.</returns>
	/// <param name="path">Path.</param>
	public static List<string> SearchForSubfolders(string path) {	
		List<string> subfolders = new List<string>();
		foreach(string folder in Directory.GetDirectories(path)) {
			subfolders.Add(Path.GetDirectoryName(folder));
		}
		return subfolders;
	}
	
	/// <summary>
	/// Search for CSV files in directory.
	/// </summary>
	/// <returns>List of CSV files.</returns>
	/// <param name="path">Path.</param>
	public static List<string> SearchForCSV(string path) {
		List<string> logsFound = new List<string>();
		foreach (string file in Directory.GetFiles(path, "*.csv")) {
			logsFound.Add(Path.GetFileNameWithoutExtension(file));
		}
		return logsFound;
	}
	
	
}
