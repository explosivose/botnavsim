using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Environment loader searches for and loads environment GameObjects from 
/// the Unity Resources folder.
/// </summary>
public class EnvLoader  {

	public static List<GameObject> environmentsFound = new List<GameObject>();
	
	/// <summary>
	/// Searchs for environments.
	/// </summary>
	public static void SearchForEnvironments() {
		environmentsFound.Clear();
		GameObject[] envs = Resources.LoadAll<GameObject>("Environments/");
		foreach (GameObject e in envs) {
			environmentsFound.Add(e);
		}
	}
	
	/// <summary>
	/// Loads the environment.
	/// </summary>
	/// <returns>The environment.</returns>
	/// <param name="name">Name.</param>
	public static GameObject LoadEnvironment(string name) {
		foreach (GameObject e in environmentsFound) {
			if (e.name == name) {
				Transform env = e.transform.Spawn();
				env.position = e.transform.position;
				return env.gameObject;
			}
		}
		Debug.LogError(name + " environment not found.");
		return null;
	}
	
	/// <summary>
	/// Loads a random environment.
	/// </summary>
	/// <returns>The environment.</returns>
	public static GameObject RandomEnvironment() {
		SearchForEnvironments();
		int index = Random.Range(0, environmentsFound.Count);
		return environmentsFound[index];
	}
	
	/// <summary>
	/// Selects a random environment name.
	/// </summary>
	/// <returns>The environment name.</returns>
	public static string RandomEnvironmentName() {
		SearchForEnvironments();
		int index = Random.Range(0, environmentsFound.Count);
		return environmentsFound[index].name;
	}
}
