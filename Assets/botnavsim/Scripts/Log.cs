using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Log  {

	// list of parameters that can be logged
	public enum Parameters {
		SimulationTime,
		SimulationTimeScale,
		RobotPosition,
		RobotIsStuck,
		DestinationPosition,
		NavigationPathFound,
		NavigationMoveDirection
	}
	
	// static members
	public static float rate = 0.05f;
	public static List<Parameters> availableParams = new List<Parameters>();
	public static List<Parameters> selectedParams = new List<Parameters>();
	
	private static bool logging = false;
	private static string header;
	private static Queue<string> log = new Queue<string>();
	
	// static constructor initialises static members
	static Log() {
		foreach(Parameters p in (Parameters[])Enum.GetValues(typeof(Parameters))) {
			availableParams.Add(p);
		}
		// parameters logged by default
		LogParameter(Parameters.SimulationTime, true);
		LogParameter(Parameters.RobotPosition, true);
		LogParameter(Parameters.NavigationMoveDirection, true);
		LogParameter(Parameters.DestinationPosition, true);
	}
	
	// move a parameter between available and selected
	public static void LogParameter(Parameters parameter, bool log) {
		if (log) {
			if (!availableParams.Contains(parameter)) {
				Debug.LogWarning("Attempted to log unavailable parameter.");
				return;
			}
			availableParams.Remove(parameter);
			if (selectedParams.Contains(parameter)) {
				Debug.LogWarning("Attempted to log parameter twice.");
				return;
			}
			selectedParams.Add(parameter);
		}
		else {
			selectedParams.Remove(parameter);
			availableParams.Add(parameter);
		}
	}
	
	// begin logging
	public static void Start() {
		Debug.Log("Log Started.");
		Simulation.Settings info = Simulation.settings;
		header = Strings.projectTitle + " " + Strings.projectVersion + " - Data Log";
		header += Strings.newline + info.title + ", " + info.date + " " + info.time;
		header += Strings.newline + "Test number, " + Simulation.testNumber + ", of, " + info.numberOfTests;;
		header += Strings.newline + "Robot, " + info.robotName;
		header += Strings.newline + "Navigation Assembly, " + info.navigationAssemblyName;
		header += Strings.newline + "Environment, " + info.environmentName;
		header += Strings.newline + "Randomize Origin, " + info.randomizeOrigin;
		header += Strings.newline + "Randomize Destination, " + info.randomizeDestination;
		header += Strings.newline + "Maximum Test Time, " + info.maximumTestTime;
		header += Strings.newline + "Continue on NavObjectiveComplete, " + info.continueOnNavObjectiveComplete;
		header += Strings.newline + "Continue on RobotIsStuck, " + info.continueOnRobotIsStuck;
		logging = true;
		Simulation.Instance.StartCoroutine(LogRoutine());
	}
	
	// stop logging, save to file
	public static void Stop(Simulation.StopCode stopcode) {
		if (logging) {
			logging = false;
			string path = System.Environment.CurrentDirectory + "\\Logs";
			if (!Directory.Exists(path)) {
				Directory.CreateDirectory(path);
			}
			path += "\\" + System.DateTime.Now.ToString("yyyyMMdd-HHmmss-");
			path += Simulation.settings.title + "_" + Simulation.testNumber;
			path += ".csv";
			header += Strings.newline + "Test ran for, " + Simulation.time + ",";
			header += " and stopped with, " + stopcode.ToString() + Strings.newline; 
			string data = header + Strings.newline;
			while(log.Count > 0) {
				data += log.Dequeue() + Strings.newline;
			}
			File.WriteAllText(path, data);
			Debug.Log("Log created at: " + path);
		}
		
		log.Clear();
	}
	
	// logging routine
	private static IEnumerator LogRoutine() {
		string headings = "";
		foreach(Parameters p in selectedParams) {
			headings += p.ToString() + ",";
		}
		log.Enqueue(headings);
		while (logging) {
			string line = "";
			foreach(Parameters p in selectedParams) {
				line += "\"" + GetData(p) + "\",";
			}
			log.Enqueue(line);
			yield return new WaitForSeconds(rate);
		}
	}
	
	// get data for parameter 
	private static string GetData(Parameters parameter) {
		switch (parameter) {
		case Parameters.SimulationTime:
			return Simulation.time.ToString();
		case Parameters.SimulationTimeScale:
			return Simulation.timeScale.ToString();
		case Parameters.RobotPosition:
			return Simulation.robot.transform.position.ToString();
		case Parameters.RobotIsStuck:
			return Simulation.robot.stuckpc.ToString();
		case Parameters.DestinationPosition:
			return Simulation.destination.transform.position.ToString();
		case Parameters.NavigationPathFound:
			return Simulation.robot.navigation.pathFound.ToString();
		case Parameters.NavigationMoveDirection:
			return Simulation.robot.moveCommand.ToString();
		}
		return null;
	}
}
