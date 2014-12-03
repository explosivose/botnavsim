using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class NavLoader {

	public static List<string> pluginsFound = new List<string>();
	public static string activePlugin = "<NONE>";
	private static string _searchDirectory;
	private static PluginFactory<INavigation> _loader = new PluginFactory<INavigation>();
	
	public static void SearchForPlugins() {
		pluginsFound.Clear();
		_searchDirectory = System.Environment.CurrentDirectory;
#if UNITY_EDITOR
		_searchDirectory += "\\Assets\\botnavsim\\INavigation";
#endif
		Debug.Log ("Search directory: " + _searchDirectory);
		pluginsFound = _loader.ListPlugins(_searchDirectory);	
		foreach(string s in pluginsFound) {
			Debug.Log("INavigation Plugin: " + s);
		}
		
	}
	
	public static void SetPlugin(string name) {
		if (!name.Contains(".dll")) name += ".dll";
		INavigation navigation = _loader.CreatePlugin(
			_searchDirectory + "\\" + name);
	 	activePlugin = name;
		Simulation.botscript.navigation = navigation;
		
	}

}
