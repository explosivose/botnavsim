using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// File browser utility class for finding files and folders.
/// </summary>
public class FileBrowser {

	/// <summary>
	/// Lists folder names in directory.
	/// </summary>
	/// <returns>List of folder names.</returns>
	/// <param name="path">Path to search.</param>
	public static List<string> ListFolders(string path) {
		List<string> subfolders = new List<string>();
		foreach(string folder in Directory.GetDirectories(path)) {
			subfolders.Add(folder);
		}
		return subfolders;
	}
	
	/// <summary>
	/// Lists file names in directory.
	/// </summary>
	/// <returns>List of file names.</returns>
	/// <param name="path">Path to search.</param>
	/// <param name="pattern">Search pattern (i.e. "*.xml").</param>
	public static List<string> ListFiles(string path, string pattern = null) {
		List<string> files = new List<string>();
		if (pattern == null) {
			foreach(string file in Directory.GetFiles(path)) 
				files.Add(Path.GetFileName(file));
		}
		else {
			foreach(string file in Directory.GetFiles(path, pattern)) 
				files.Add(Path.GetFileName(file));
		}
		return files;
	}
}
