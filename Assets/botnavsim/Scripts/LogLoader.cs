using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

/// <summary>
/// Log loader state machine. 
/// </summary>
public class LogLoader : MonoBehaviour, IObservable  {
	
	public static LogLoader Instance;
	
	// static class constructor
	static LogLoader() {
		paths = new List<BotPath>();
	}
	
	/// <summary>
	/// The BotPaths loaded from file.
	/// </summary>
	public static List<BotPath> paths {
		get; private set;
	}
	
	public static bool loading {
		get; private set;
	}
	
	/// <summary>
	/// Gets the environment game object.
	/// </summary>
	/// <value>The environment.</value>
	public static GameObject environment {
		get; private set;
	}
	
	/// <summary>
	/// Gets the environment bounds.
	/// </summary>
	/// <value>The bounds.</value>
	public Bounds bounds {
		get; private set;
	}
	
	private static bool _waitingForResponse;
	private static bool _response;
	
	public static void Enter() {
		CamController.AddViewMode(CamController.ViewMode.Birdseye);
		CamController.AddViewMode(CamController.ViewMode.FreeMovement);
		CamController.AddAreaOfInterest(Instance);
	}
	
	/// <summary>
	/// Exit BotNavSim.state behaviour: remove environment, clear paths
	/// </summary>
	public static void Exit() {
		CamController.ClearAreaList();
		CamController.ClearViewModeList();
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
		
		Instance.StartCoroutine( LoadCsvRoutine(csvFilePath) );
		

	}
	
	static void PromptResponse(bool response) {
		_waitingForResponse = false;
		_response = response;
	}
	
	static IEnumerator LoadCsvRoutine(string csvFilePath) {
		loading = true;
		
		string csvPath = Path.GetDirectoryName(csvFilePath);
		string csvFileName = Path.GetFileName(csvFilePath);
		
		// try opening the file with StreamReader
		StreamReader sr;
		try {
			sr = new StreamReader(csvFilePath);
		}
		catch (Exception e) {
			// log exception
			Debug.LogException(e);
			UI_Toolbar.I.additionalWindows.Add( 
			    new UI_Prompt(
				PromptResponse,
				UI_Prompt.Type.Close,
				"File Load Exception!",
				"See log for details"
				)
			);
			// stop loading
			loading = false;
			yield break;
		}

		// inspect CSV header 
		string line;
		string header = "";
		string xmlFileName = null;
		while ((line = sr.ReadLine()) != null) {
			// stop inspecting when comments are no longer found
			if (!line.StartsWith(Strings.csvComment)) {
				break;
			}
			header += line;
			// find XML filename stored in CSV
			if (line.Contains(Strings.csvXmlCommentTag)) {
				xmlFileName = line.Substring(
					line.IndexOf(Strings.csvXmlCommentTag) + 
					Strings.csvXmlCommentTag.Length);
				Debug.Log("XML filename from CSV is: " + xmlFileName);
			}
		}
		
		// temporary settings object for deserialization
		Simulation.Settings settings;
		
		
		// if xml filename was not found in csv...
		if (xmlFileName == null) {
			settings = new Simulation.Settings();
			// prompt user whether to select environment
			_waitingForResponse = true;
			UI_Toolbar.I.additionalWindows.Add( 
			   	new UI_Prompt(
					PromptResponse,
					UI_Prompt.Type.YesNo,
					"XML filename not found in CSV header!",
					header + "\n Select environment to load?"
				)
			);
			while (_waitingForResponse) {
				yield return new WaitForSeconds(0.1f);
			}
			if (_response) {
				/// not yet implemented
				// browse environments and load selection
				Debug.Log("Not yet implemented: browse and load environment");
			}
		}
		else {
			// try loading environment
			settings = ObjectSerializer.DeSerializeObject<Simulation.Settings>(csvPath + "\\" + xmlFileName);
			// if environment is different to the currently loaded environment
			// prompt user for action  (discard other paths, or load new env and paths?)
			// (not yet implemented)
			if (environment) {
				if (environment.name != settings.environmentName) {
					_waitingForResponse = true;
					UI_Toolbar.I.additionalWindows.Add( 
					 	new UI_Prompt(
							PromptResponse,
							UI_Prompt.Type.OkCancel,
							"Load new environment and paths?",
							"CSV log is for a different environment. Load new environment and paths instead?"
						)
					);
					while (_waitingForResponse) {
						yield return new WaitForSeconds(0.1f);
					}
					if (_response) {
						// load environment and clear paths if YES
						paths.Clear();
						CamController.ClearAreaList();
						LoadEnvironment(settings.environmentName);
					} 
					else {
						// stop loading if NO
						loading = false;
						yield break;
					}
				}
			}
			else {
				LoadEnvironment(settings.environmentName);
			}
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
		
		
		AddPath(botpos);
		UpdatePathColors();
		loading = false;
	}
	
	public static void AddPath(BotPath path) {
		if (!paths.Contains(path)) paths.Add(path);
		CamController.AddAreaOfInterest(path);
	}
	
	public static void RemovePath(BotPath path) {
		paths.Remove(path);
		CamController.RemoveAreaOfInterest(path);
	}
	
	/// <summary>
	/// Updates the path colors with evenly spaced hues.
	/// </summary>
	public static void UpdatePathColors() {
		float d = 1f/(float)paths.Count;
		for(int i = 0; i < paths.Count; i++) {
			float h = i*d;
			Color c = HSBColor.ToColor(new HSBColor(h,1f,1f));
			paths[i].color = c;
		}
	}
	
	private static void LoadEnvironment(string name) {
		EnvLoader.SearchForEnvironments();
		if (environment) environment.transform.Recycle();
		environment = EnvLoader.LoadEnvironment(name);
		environment.name = name; // avoids: Unity appending "(Clone)" to instance names
		Debug.Log(environment);
		Bounds b = new Bounds();
		foreach(Renderer r in environment.GetComponentsInChildren<Renderer>())
			b.Encapsulate(r.bounds);
		Instance.bounds = b;
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
		// draw path lines
		foreach(BotPath p in paths) {
			 if (p.visible) p.DrawPath();
		}
	}
	
	void OnDrawGizmos() {
		foreach(BotPath p in paths) {
			if (p.visible) {
				Gizmos.color = p.color;
				Gizmos.DrawCube(p.bounds.center, p.bounds.size);
				Gizmos.DrawLine(p.start, p.end);
			}
		}
	}
}
