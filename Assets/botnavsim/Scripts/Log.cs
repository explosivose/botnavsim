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
	public static bool logging {
		get; private set;
	}
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
		if (logging) {
			Debug.LogWarning("Already logging!");
			return;
		}
		Debug.Log("Log Started.");
		string br = Strings.newline + Strings.csvComment;
		char d = Strings.csvDelimiter;
		Simulation.Settings info = Simulation.settings;
		header = Strings.csvComment + Strings.projectTitle + " " + Strings.projectVersion + " - Data Log " + d +
			DateTime.Now.ToShortDateString() + d + DateTime.Now.ToShortTimeString();
		header += br + info.title + d + info.date + " " + info.time;
		header += br + Strings.csvXmlCommentTag + info.fileName;
		header += br + "Test number" + d + Simulation.testNumber + d + "of" + d + info.numberOfTests;
		header += br + "Robot" + d + info.robotName;
		header += br + "Navigation Assembly" + d + info.navigationAssemblyName;
		header += br + "Environment" + d + info.environmentName;
		header += br + "Randomize Origin" + d + info.randomizeOrigin;
		header += br + "Randomize Destination" + d + info.randomizeDestination;
		header += br + "Maximum Test Time" + d + info.maximumTestTime;
		header += br + "Continue on NavObjectiveComplete" + d + info.continueOnNavObjectiveComplete;
		header += br + "Continue on RobotIsStuck" + d + info.continueOnRobotIsStuck;
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
		path += "\\" + Simulation.settings.fileName;
		Debug.Log("Settings saved at " + path);
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
			header += Strings.newline + Strings.csvComment + "Test ran for" + Strings.csvDelimiter + Simulation.time;
			header += Strings.csvDelimiter + "and stopped with" + Strings.csvDelimiter + stopcode.ToString() + Strings.newline; 
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
			headings += p.ToString() + Strings.csvDelimiter;
		}
		log.Enqueue(headings);
		while (logging) {
			string line = "";
			foreach(Parameters p in selectedParams) {
				line += "\"" + GetData(p) + "\"" + Strings.csvDelimiter;
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
			return Simulation.robot.rigidbody.worldCenterOfMass.ToString();
		case Parameters.RobotIsStuck:
			return Simulation.robot.stuckpc.ToString();
		case Parameters.DestinationPosition:
			return Simulation.destination.transform.position.ToString();
		case Parameters.NavigationPathFound:
			return Simulation.robot.navigation.pathFound.ToString();
		case Parameters.NavigationMoveDirection:
			return Simulation.robot.navigationCommand.ToString();
		}
		return null;
	}
}
