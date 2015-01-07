using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class NavLoader {

	public static List<string> pluginsFound = new List<string>();
	private static string _searchDirectory;
	private static PluginFactory<INavigation> _loader = new PluginFactory<INavigation>();
	
	public static void SearchForPlugins() {
		pluginsFound.Clear();
		_searchDirectory = System.Environment.CurrentDirectory;
#if UNITY_EDITOR
		_searchDirectory += "\\Assets";
#endif
		_searchDirectory += "\\INavigation";
		if (!Directory.Exists(_searchDirectory))
			Directory.CreateDirectory(_searchDirectory);
		pluginsFound = _loader.ListPlugins(_searchDirectory);	
		Debug.Log ("Found " + pluginsFound.Count + " plugins at " + _searchDirectory);
	}
	
	public static INavigation LoadPlugin(string name) {
		if (!name.Contains(".dll")) name += ".dll";
		return _loader.CreatePlugin(_searchDirectory + "\\" + name);
	}
	
	public static INavigation RandomPlugin() {
		SearchForPlugins();
		int index = UnityEngine.Random.Range(0, pluginsFound.Count-1);
		return LoadPlugin(pluginsFound[index]);
	}
	
	public static string RandomPluginName() {
		SearchForPlugins();
		int index = UnityEngine.Random.Range(0, pluginsFound.Count-1);
		return pluginsFound[index];
	}
}
