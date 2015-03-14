using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class NavLoader {

	public static List<string> pluginsFound = new List<string>();
	private static PluginFactory<INavigation> _loader = new PluginFactory<INavigation>();
	
	public static void SearchForPlugins() {
		pluginsFound.Clear();
		if (!Directory.Exists(Strings.navigationPluginDirectory))
			Directory.CreateDirectory(Strings.navigationPluginDirectory);
		pluginsFound = _loader.ListPlugins(Strings.navigationPluginDirectory);	
		Debug.Log ("Found " + pluginsFound.Count + " plugins at " + Strings.navigationPluginDirectory);
	}
	
	public static INavigation LoadPlugin(string name) {
		if (!name.Contains(".dll")) name += ".dll";
		return _loader.CreatePlugin(Strings.navigationPluginDirectory + "\\" + name);
	}
	
	public static INavigation RandomPlugin() {
		SearchForPlugins();
		int index = UnityEngine.Random.Range(0, pluginsFound.Count);
		return LoadPlugin(pluginsFound[index]);
	}
	
	public static string RandomPluginName() {
		SearchForPlugins();
		int index = UnityEngine.Random.Range(0, pluginsFound.Count);
		return pluginsFound[index];
	}
}
