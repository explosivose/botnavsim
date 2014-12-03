using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BotLoader {

	public static List<GameObject> robotsFound = new List<GameObject>();
	
	public static void SearchForRobots() {
		GameObject[] robots = Resources.LoadAll<GameObject>("Robots/");
		foreach (GameObject r in robots) {
			if (r.GetComponent<Robot>()) {
				robotsFound.Add(r);
			}
		}
	}
	
	public static void SetRobot(int index) {
		Transform robot = robotsFound[index].transform.Spawn();
		Simulation.robot = robot.gameObject;
	}
}
