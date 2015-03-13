using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class LogLoader {

	
	/// <summary>
	/// Loads the paths stored in a CSV file. Also attempts to load
	/// the relevant environment. 
	/// </summary>
	/// <returns>List of paths found in CSV file.</returns>
	/// <param name="csvFilePath">Path to CSV file.</param>
	public static List<BotPath> LoadPaths(string csvFilePath) {
		string csvPath = Path.GetDirectoryName(csvFilePath);
		string csvFileName = Path.GetFileName(csvFilePath);
		
		StreamReader sr;
		try {
			sr = new StreamReader(csvFilePath);
		}
		catch (Exception e) {
			Debug.LogException(e);
			return null;
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
			settings = new Simulation.Settings();
		}
		else {
			// try loading environment
			settings = ObjectSerializer.DeSerializeObject<Simulation.Settings>(csvPath + "\\" + xmlFileName);
			EnvLoader.SearchForEnvironments();
			Simulation.environment = EnvLoader.LoadEnvironment(settings.environmentName);
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
		
		List<BotPath> paths = new List<BotPath>();
		BotPath botpos = new BotPath();
		botpos.csvName = csvFileName;
		paths.Add(botpos);
		// Build BotPath objects for each path type columns found
		while((line = sr.ReadLine()) != null) {
			row = line.Split(Strings.csvDelimiter);
			if (robotPositionIndex > row.Length) {
				Debug.LogWarning("LogLoader: row length too short?");
				continue;
			}
			// try deserializing this vector3 data
			Vector3 pos = ParseVector3(row[robotPositionIndex]);
			// remove " chars from "12.3", for example
			float time = float.Parse(row[0].Substring(1,row[0].Length-2));
			botpos.AddNode(pos, time);
		}
		
		return paths;
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
