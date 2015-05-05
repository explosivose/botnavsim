using UnityEngine;
using System.Collections;

/// <summary>
/// Environment editor state machine class. 
/// </summary>
public class EnvironmentEditor : MonoBehaviour {

	public enum State {
		inactive,
		editing
	}
	
	/// <summary>
	/// Singleton pattern for MonoBehaviour. 
	/// </summary>
	public static EnvironmentEditor Instance;
	
	/// <summary>
	/// Gets the RobotEditor.State.
	/// </summary>
	public static State state {
		get; private set;
	}
	
	/// <summary>
	/// Exit this state machine.
	/// </summary>
	public static void Exit() {
		state = State.inactive;
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
}
