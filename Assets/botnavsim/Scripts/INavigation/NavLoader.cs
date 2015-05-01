using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

/// <summary>
/// Used to find and load INavigation implementations from .dll files. 
/// </summary>
public class NavLoader {

	/// <summary>
	/// List of .dll files that implement INavigation.
	/// </summary>
	public static List<string> pluginsFound = new List<string>();
	private static PluginFactory<INavigation> _loader = new PluginFactory<INavigation>();
	
	/// <summary>
	/// Searchs for plugins in the INavigation directory (Strings.navigationPluginDirectory)
	/// Results are stored in pluginsFound.
	/// </summary>
	public static void SearchForPlugins() {
		pluginsFound.Clear();
		if (!Directory.Exists(Strings.navigationPluginDirectory))
			Directory.CreateDirectory(Strings.navigationPluginDirectory);
		pluginsFound = _loader.ListPlugins(Strings.navigationPluginDirectory);	
		Debug.Log ("Found " + pluginsFound.Count + " plugins at " + Strings.navigationPluginDirectory);
	}
	
	/// <summary>
	/// Instantiate the INavigation assembly from a given filename (not filepath).
	/// </summary>
	/// <returns>The plugin.</returns>
	/// <param name="name">Name.</param>
	public static INavigation LoadPlugin(string name) {
		if (!name.Contains(".dll")) name += ".dll";
		return _loader.CreatePlugin(Strings.navigationPluginDirectory + "\\" + name);
	}
	
	/// <summary>
	/// Instantiates a random INavigation plugin from any plugin in the INavigation directory.
	/// </summary>
	/// <returns>The plugin.</returns>
	public static INavigation RandomPlugin() {
		SearchForPlugins();
		int index = UnityEngine.Random.Range(0, pluginsFound.Count);
		return LoadPlugin(pluginsFound[index]);
	}
	
	/// <summary>
	/// Returns a random filename from the pluginsFound list. 
	/// </summary>
	/// <returns>The plugin name.</returns>
	public static string RandomPluginName() {
		SearchForPlugins();
		int index = UnityEngine.Random.Range(0, pluginsFound.Count);
		return pluginsFound[index];
	}
}
