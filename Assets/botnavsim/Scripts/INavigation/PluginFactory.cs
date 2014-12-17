using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Reflection;
using System.Collections.Generic;


/// <summary>
/// Plugin factory.
/// http://stackoverflow.com/questions/5751844/how-to-reference-a-dll-on-runtime
/// </summary>
public class PluginFactory<T> {

	public T CreatePlugin(string file) {
		
		if (!File.Exists(file)) {
			Debug.LogError("File not found (" + file + ")");
			return default(T);
		}
		Type[] assemblyTypes =  Assembly.LoadFrom(file).GetTypes();
		foreach(Type assemblyType in assemblyTypes) {
			Type interfaceType = assemblyType.GetInterface(typeof(T).FullName);
			// if our interface is found, instantiate and return it!
			if (interfaceType != null) {
				return (T)Activator.CreateInstance(assemblyType);
			}
		}
		Debug.LogError("Interface " + typeof(T).FullName + " not found in file: " + file);
		return default(T);
	}
	
	public List<string> ListPlugins(string path) {
		List<string> list = new List<string>();
		// find .dll files
		foreach (string file in Directory.GetFiles(path, "*.dll")) {
			Assembly assembly = Assembly.LoadFrom(file);
			Type[] types;
			try {
				types = assembly.GetTypes();
			}
			catch {
				Debug.LogError(file + " is not INavigation compatible!");
				continue;
			}
			foreach (Type assemblyType in types) {
				Type interfaceType = assemblyType.GetInterface(typeof(T).FullName);
				if (interfaceType != null) {
					list.Add(Path.GetFileName(file));
				}
			}
		}
		
		return list;
	}
}
