using UnityEngine;
using System.Collections;

public class Simulation : MonoBehaviour {

	public static Simulation Instance;
	
	public static GameObject robot;
	public static GameObject destination;
	public static CameraPerspective cam;
	
	public static Robot botscript;
	
	void Awake() {
		if (Instance) {
			Destroy(this.gameObject);
		}
		else {
			Instance = this;
		}
	}
	
	void Start() {
		robot = GameObject.Find("Bot");
		destination = GameObject.Find("Destination");
		cam = Camera.main.GetComponent<CameraPerspective>();
		
		if (robot) 
			botscript = robot.GetComponent<Robot>();
		else 
			Debug.LogError("Bot not found.");
			
		cam.perspective = CameraPerspective.Perspective.Birdseye;
	}
	
	void Update() {
		if (Input.GetKeyUp(KeyCode.Space)) {
			cam.CyclePerspective();
		}
	}
	
	void OnGUI() {
		GUILayout.Label("Viewing from: " + cam.perspective.ToString());
		GUILayout.Label(botscript.description);
	}
}
