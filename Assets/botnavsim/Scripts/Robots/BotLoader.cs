using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Bot loader provides functions for finding and loading robots. 
/// </summary>
public class BotLoader {

	/// <summary>
	/// List of robots found in the Robots/ resources directory
	/// </summary>
	public static List<GameObject> robotsFound = new List<GameObject>();
	
	
	/// <summary>
	/// Searchs for robots.
	/// </summary>
	public static void SearchForRobots() {
		robotsFound.Clear();
		GameObject[] robots = Resources.LoadAll<GameObject>("Robots/");
		foreach (GameObject r in robots) {
			if (r.GetComponent<Robot>()) {
				robotsFound.Add(r);
			}
		}
	}
	
	/// <summary>
	/// Load a robot by index in robotsFound list.
	/// </summary>
	/// <returns>The robot.</returns>
	/// <param name="index">Index.</param>
	public static Robot LoadRobot(int index) {
		Transform robot = robotsFound[index].transform.Spawn();
		return robot.GetComponent<Robot>();
	}
	
	/// <summary>
	/// Loads a robot from robotsFound list by name. 
	/// </summary>
	/// <returns>The robot.</returns>
	/// <param name="name">Name.</param>
	public static Robot LoadRobot(string name) {
		foreach (GameObject r in robotsFound) {
			if (r.name == name) {
				Transform robot = r.transform.Spawn();
				return robot.GetComponent<Robot>();
			}
		}
		Debug.LogError(name + " robot not found.");
		return null;
	}
	
	/// <summary>
	/// Loads a random robot from the robotsFound list.
	/// </summary>
	/// <returns>The robot.</returns>
	public static Robot RandomRobot() {
		SearchForRobots();
		int index = Random.Range(0, robotsFound.Count);
		return LoadRobot(index);
	}
	
	/// <summary>
	/// Returns a random robot name from the robotsFound list.
	/// </summary>
	/// <returns>The robot name.</returns>
	public static string RandomRobotName() {
		SearchForRobots();
		int index = Random.Range(0, robotsFound.Count);
		return robotsFound[index].name;
	}
}
