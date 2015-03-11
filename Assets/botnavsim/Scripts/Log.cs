using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Provides data logging capabilities. Data is logged in CSV format.
/// </summary>
public class Log  {

	
	public enum Parameters {
		/// <summary>
		/// The simulation time (float).
		/// </summary>
		SimulationTime,
		/// <summary>
		/// The simulation time scale (float).
		/// </summary>
		SimulationTimeScale,
		/// <summary>
		/// The robot current position (Vector3).
		/// </summary>
		RobotPosition,
		/// <summary>
		/// Indication of robot stuck detection as percentage (0 to 100).
		/// </summary>
		RobotIsStuck,
		/// <summary>
		/// The destination position (Vector3).
		/// </summary>
		DestinationPosition,
		/// <summary>
		/// Bool output from INavigation.pathFound
		/// </summary>
		NavigationPathFound,
		/// <summary>
		/// Vector3 output from INavigation.PathDirection
		/// </summary>
		NavigationMoveDirection
	}
	
	/// <summary>
	/// The time between log entries.
	/// </summary>
	public static float timeStep = 0.05f;
	
	/// <summary>
	/// List of parameters that haven't been selected for logging.
	/// </summary>
	public static List<Parameters> availableParams = new List<Parameters>();
	
	/// <summary>
	/// List of parameters selected for logging.
	/// </summary>
	public static List<Parameters> selectedParams = new List<Parameters>();
	
	
	// flag used for running log routine
	private static bool logging = false;
	// metadata to write to the top of the CSV file
	private static string header;
	// FIFO timeframe data buffer to be written to file 
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
	
	/// <summary>
	/// Moves parameters between availableParams and selectedParams lists.
	/// </summary>
	/// <param name="parameter">Parameter.</param>
	/// <param name="log">If set to <c>true</c> parameter is added to selectedParams for logging.</param>
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
	
	/// <summary>
	/// Start logging.
	/// </summary>
	public static void Start() {
		Debug.Log("Log Started.");
		Simulation.Settings info = Simulation.settings;
		header = "# " + Strings.projectTitle + " " + Strings.projectVersion + " - Data Log, " +
		DateTime.Now.ToShortDateString() + "," + DateTime.Now.ToShortTimeString();
		header += Strings.newline + "# " + info.title + ", " + info.date + " " + info.time;
		header += Strings.newline + "# Test number, " + Simulation.testNumber + ", of, " + info.numberOfTests;;
		header += Strings.newline + "# Robot, " + info.robotName;
		header += Strings.newline + "# Navigation Assembly, " + info.navigationAssemblyName;
		header += Strings.newline + "# Environment, " + info.environmentName;
		header += Strings.newline + "# Randomize Origin, " + info.randomizeOrigin;
		header += Strings.newline + "# Randomize Destination, " + info.randomizeDestination;
		header += Strings.newline + "# Maximum Test Time, " + info.maximumTestTime;
		header += Strings.newline + "# Continue on NavObjectiveComplete, " + info.continueOnNavObjectiveComplete;
		header += Strings.newline + "# Continue on RobotIsStuck, " + info.continueOnRobotIsStuck;
		logging = true;
		Simulation.Instance.StartCoroutine(LogRoutine());
	}
	
	/// <summary>
	/// Serialize the current simulation settings and write to XML file. 
	/// </summary>
	public static void Settings() {
		string path = Strings.logFileDirectory;
		path += "\\" + System.DateTime.Now.ToString("yyyy_MM_dd");
		if (!System.IO.Directory.Exists(path)) {
			System.IO.Directory.CreateDirectory(path);
		}
		path += "\\" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss_");
		path += Simulation.settings.title;
		path += ".xml";
		Debug.Log(path);
		ObjectSerializer.SerializeObject(Simulation.settings, path);
	}
	
	/// <summary>
	/// Stop logging with Simulation.Stopcode and write log to CSV file.
	/// </summary>
	/// <param name="stopcode">Stopcode.</param>
	public static void Stop(Simulation.StopCode stopcode) {
		if (logging) {
			logging = false;
			string path = Strings.logFileDirectory;
			path += "\\" + System.DateTime.Now.ToString("yyyy_MM_dd");
			if (!Directory.Exists(path)) {
				Directory.CreateDirectory(path);
			}
			path += "\\" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss_");
			path += Simulation.settings.title + "_" + Simulation.testNumber;
			path += ".csv";
			header += Strings.newline + "# Test ran for, " + Simulation.time + ",";
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
			yield return new WaitForSeconds(timeStep);
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
