using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class NavLoader : Singleton<NavLoader> {

	
	string searchDirectory;
	List<string> pluginsFound = new List<string>();
	PluginFactory<INavigation> loader = new PluginFactory<INavigation>();

	void Start() {
		SearchForPlugins();
		Simulation.botscript.navigation = loader.CreatePlugin(
			searchDirectory + "\\Astar.dll");
	}
	
	void SearchForPlugins() {
		pluginsFound.Clear();
		searchDirectory = System.Environment.CurrentDirectory;
#if UNITY_EDITOR
		searchDirectory += "\\Assets\\botnavsim\\INavigation";
#endif
		Debug.Log ("Search dirctory: " + searchDirectory);
		pluginsFound = loader.ListPlugins(searchDirectory);	
		foreach(string s in pluginsFound) {
			Debug.Log ("INavigation Plugin: " + s);
		}
	}

}
