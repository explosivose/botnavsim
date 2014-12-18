using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Log  {

	public class List {
		public bool robotPosition;
		public bool destinationPosition;
		public bool robotDistanceToDestination;
	}
	
	public static float rate = 0.05f;
	
	private static bool logging = false;
	private static string header;
	private static Queue<string> log = new Queue<string>();
	
	public static void Start() {
		Simulation.Settings info = Simulation.settings;
		header = info.title + ", " + info.date + " " + info.time;
		header += "\n" + info.summary;
		header += "\n" + "Test number: " + Simulation.testNumber;
		logging = true;
		Simulation.Instance.StartCoroutine(LogRoutine());
	}
	
	public static void Stop() {
		if (logging) {
			logging = false;
			string path = System.Environment.CurrentDirectory + "\\Logs\\";
			path += System.DateTime.Now.ToString("yyyyMMdd-HHmmss-");
			path += Simulation.settings.title + "_" + Simulation.testNumber;
			path += ".dat";
			string data = header + "\n";
			while(log.Count > 0) {
				data += log.Dequeue() + "\n";
			}
			File.WriteAllText(path, data);
			Debug.Log("Log created at: " + path);
		}
		log.Clear();
	}
	
	private static IEnumerator LogRoutine() {
		string headings = "\"Time\",\"";
		headings += "TimeScale\",\"";
		headings += "Robot Position\",\"";
		headings += "Destination Position\",\"";
		headings += "Path Found\",\"";
		headings += "Navigation Vector\",\"";
		headings += "Robot Stuck\",\"";
		log.Enqueue(headings);
		while (logging) {
			string line = "\"" + Simulation.time.ToString() + "\",\"";
			line += Simulation.timeScale.ToString() + "\",\"";
			line += Simulation.robot.transform.position.ToString() + "\",\"";
			line += Simulation.destination.transform.position.ToString() + "\",\"";
			line += Simulation.robot.navigation.pathFound + "\",\"";
			line += Simulation.robot.moveCommand + "\",\"";
			line += Simulation.robot.stuckpc + "\",\"";
			log.Enqueue(line);
			yield return new WaitForSeconds(rate);
		}
	}
}
