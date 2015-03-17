using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class LogLoader {
	
	static LogLoader() {
		paths = new List<BotPath>();
	}
	
	/// <summary>
	/// The BotPaths loaded from file.
	/// </summary>
	public static List<BotPath> paths;
	
	private GameObject _environment;
	
	/// <summary>
	/// Loads the paths stored in a CSV file. Also attempts to load
	/// the relevant environment. 
	/// </summary>
	/// <returns>List of paths found in CSV file.</returns>
	/// <param name="csvFilePath">Path to CSV file.</param>
	public static void LoadPaths(string csvFilePath) {
		string csvPath = Path.GetDirectoryName(csvFilePath);
		string csvFileName = Path.GetFileName(csvFilePath);
		
		StreamReader sr;
		try {
			sr = new StreamReader(csvFilePath);
		}
		catch (Exception e) {
			Debug.LogException(e);
			return;
		}
		
		
		// inspect comments 
		string line;
		string xmlFileName = null;
		while ((line = sr.ReadLine()) != null) {
			// stop inspecting when comments are no longer found
			if (!line.StartsWith(Strings.csvComment)) {
				break;
			}
			// find XML filename stored in CSV
			if (line.Contains(Strings.csvXmlCommentTag)) {
				xmlFileName = line.Substring(
					line.IndexOf(Strings.csvXmlCommentTag) + 
					Strings.csvXmlCommentTag.Length);
				Debug.Log("XML filename from CSV is: " + xmlFileName);
			}
		}
		
		Simulation.Settings settings;
		
		// if xml filename was not found in csv...
		if (xmlFileName == null) {
			// prompt user whether to browse for XML
			// or to choose an environment to load
			// (not yet implemented)
			settings = new Simulation.Settings();
		}
		else {
			// try loading environment
			settings = ObjectSerializer.DeSerializeObject<Simulation.Settings>(csvPath + "\\" + xmlFileName);
			// if environment is different to the currently loaded environment
			// prompt user for action  (discard other paths, or load new env and paths?)
			// (not yet implemented)
			EnvLoader.SearchForEnvironments();
			if (_environment) _environment.transform.Recycle();
			_environment = EnvLoader.LoadEnvironment(settings.environmentName);
			// prompt user about browsing for an environment if one couldn't be loaded
		}
		
		// load paths from CSV and display them
		
		// go to line with SimulationTime as first string
		while ((line = sr.ReadLine()) != null) {
			if (line.StartsWith(Log.Parameters.SimulationTime.ToString())) {
				break;
			}
		}
		
		// extract headings and store column indexes for path types
		int robotPositionIndex;
		string[] row = line.Split(Strings.csvDelimiter); 
		for(robotPositionIndex = 0; robotPositionIndex < row.Length; robotPositionIndex++) {
			if (row[robotPositionIndex] == Log.Parameters.RobotPosition.ToString())
				break;
		}
		
		
		BotPath botpos = new BotPath();
		botpos.csvName = csvFileName;

		// Build BotPath objects for each path type columns found
		while((line = sr.ReadLine()) != null) {
			row = line.Split(Strings.csvDelimiter);
			if (robotPositionIndex > row.Length) {
				Debug.LogWarning("LogLoader: row length too short?");
				continue;
			}
			// try deserializing this vector3 data
			// (catch parsing errors not yet implemented)
			Vector3 pos = ParseVector3(row[robotPositionIndex]);
			// remove " chars from "12.3", for example
			// (catch parsing errors not yet implemented)
			float time = float.Parse(row[0].Substring(1,row[0].Length-2));
			botpos.AddNode(pos, time);
		}
		
		List<BotPath> pathsLoaded = new List<BotPath>();
		pathsLoaded.Add(botpos);
		paths.AddRange(pathsLoaded);
	}
	
	// parse a vector3 object from a string like "(1.0,2.0,3.0)"
	private static Vector3 ParseVector3(string strv) {
		// remove " and bracket chars then split by commas
		string[] split = strv.Substring(2,strv.Length-4).Split(',');
		float x = float.Parse(split[0]);
		float y = float.Parse(split[1]);
		float z = float.Parse(split[2]);
		return new Vector3(x, y, z);
	}
	
}
