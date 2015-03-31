using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

/// <summary>
/// Log loader state machine. 
/// </summary>
public class LogLoader : MonoBehaviour  {
	
	public static LogLoader Instance;
	
	static LogLoader() {
		paths = new List<BotPath>();
	}
	
	/// <summary>
	/// The BotPaths loaded from file.
	/// </summary>
	public static List<BotPath> paths {
		get; private set;
	}
	
	public static GameObject environment {
		get; private set;
	}
	
	public static Bounds bounds {
		get; private set;
	}
	
	public static void Exit() {
		environment.transform.Recycle();
		paths.Clear();
	}
	
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
			if (environment) environment.transform.Recycle();
			environment = EnvLoader.LoadEnvironment(settings.environmentName);
			Debug.Log(environment);
			bounds = new Bounds(Vector3.zero, Vector3.zero);
			foreach(Renderer r in environment.GetComponentsInChildren<Renderer>())
				bounds.Encapsulate(r.bounds);
			Debug.Log (bounds);
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
		UpdatePathColors();
	}
	
	/// <summary>
	/// Updates the path colors with evenly spaced hues.
	/// </summary>
	public static void UpdatePathColors() {
		float d = 1f/(float)paths.Count;
		for(int i = 0; i < paths.Count; i++) {
			float h = i*d;
			//Color c = UnityEditor.EditorGUIUtility.HSVToRGB(h,1f,1f);
			Color c = HSBColor.ToColor(new HSBColor(h,1f,1f));
			paths[i].color = c;
		}
	}
	
	/// <summary>
	/// Parses the vector3 object from a string like "(1.0,2.0,3.0)"
	/// </summary>
	/// <returns>The vector3.</returns>
	/// <param name="strv">Vector3 string i.e. "(1.0,2.0,3.0)".</param>
	private static Vector3 ParseVector3(string strv) {
		// remove " and bracket chars then split by commas
		string[] split = strv.Substring(2,strv.Length-4).Split(',');
		float x = float.Parse(split[0]);
		float y = float.Parse(split[1]);
		float z = float.Parse(split[2]);
		return new Vector3(x, y, z);
	}
	
	/** Instance Methods **/
	
	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake() {
		if (Instance == null) {
			Instance = this;
		} else {
			Destroy(this);
		}
	}
	
	/// <summary>
	/// Raises the GUI event.
	/// </summary>
	void OnGUI() {
		foreach(BotPath p in paths) {
			 if (p.visible) p.DrawPath();
		}
	}
}
