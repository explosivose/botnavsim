using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This is a manager class used to overlook the running of a simulation.
/// </summary>
public class Simulation : MonoBehaviour {

	public enum State {
		/// <summary>
		/// BotNavSim is not simulating.
		/// </summary>
		inactive,
		
		/// <summary>
		/// Simulation is about to start.
		/// </summary>
		starting,
		
		/// <summary>
		/// Simulation is running.
		/// </summary>
		simulating,
		
		/// <summary>
		/// Simulation is stopped.
		/// </summary>
		stopped,
		
		/// <summary>
		/// Simulation has reached the end of the batch. 
		/// </summary>
		end
	}

	public enum StopCode {
		/// <summary>
		/// Reason for simulation stopping is not given.
		/// </summary>
		Unspecified,
		
		/// <summary>
		/// Simulation stopped because user requested next test.
		/// </summary>
		UserRequestNextTest,
		
		/// <summary>
		/// Simulation stopped because the robot reached the destination.
		/// </summary>
		RobotReachedDestination,
		
		/// <summary>
		/// Simulation stopped because the maximum test time was exceeded.
		/// </summary>
		MaxTimeExceeded,
		
		/// <summary>
		/// Simulation stopped because the robot appears to be stuck.
		/// i.e. the robot position has not changed for some time.
		/// </summary>
		RobotIsStuck
	}
	
	
	
	[System.Serializable]
	public class Settings {
		
		/// <summary>
		/// The title of this simulation.
		/// </summary>
		public string title = "Simulation";
				
		/// <summary>
		/// The number of repeat tests with these parameters.
		/// </summary>
		public int numberOfTests = 1;
		
		/* ### Core Parameters ### */
		
		/// <summary>
		/// The filename of the environment to load.
		/// </summary>
		public string environmentName = "<none>";
		/// <summary>
		/// The filename of the navigation assembly to load.
		/// </summary>
		public string navigationAssemblyName = "<none>";
		/// <summary>
		/// The filename of the robot to load.
		/// </summary>
		public string robotName = "<none>";

				
		/* ### Initial Conditions ### */
		
		/// <summary>
		/// If true, robot starts each test at a random location in simulation bounds.
		/// </summary>
		public bool randomizeOrigin = false;
		/// <summary>
		/// If true, destination starts each test at a random location in simulation bounds.
		/// </summary>
		public bool randomizeDestination = false;
		
		/* ### Termination Conditions ### */
		
		/// <summary>
		/// The maximum test time in seconds.
		/// </summary>
		public int maximumTestTime = 60;
		/// <summary>
		/// If true, test ends when robot reaches the destination.
		/// </summary>
		public bool continueOnNavObjectiveComplete = false;
		/// <summary>
		/// If true, test ends when robot average position over time doesn't change enough.
		/// </summary>
		public bool continueOnRobotIsStuck = false;
		
		
		/// <summary>
		/// Gets a value indicating whether this <see cref="Simulation.Settings"/> is valid for simulating with.
		/// </summary>
		/// <value><c>true</c> if is valid; otherwise, <c>false</c>.</value>
		public bool isValid {
			get {
				bool v = true;
				v &= environmentName != "<none>";
				v &= navigationAssemblyName != "<none>";
				v &= robotName != "<none>";
				v &= numberOfTests > 0;
				return v;
			}
		}
		
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Simulation.Settings"/> is active
		/// and determines which properties are editable in UI.
		/// </summary>
		/// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
		public bool active { get; set; }
		
		public string name {
			get {
				return robotName + "_" + navigationAssemblyName + "_" + environmentName;
			}
		}
		public string fileName {
			get {
				return datetime.ToString("yyyyMMdd_HHmmss_") +
					Simulation.settings.title + ".xml";
			}
		}
		public System.DateTime datetime {get; private set;}
		public string date {
			get {
				return datetime.ToShortDateString();
			}
		}
		public string time {
			get {
				return datetime.ToShortTimeString();
			}
		}
		
		public Settings() {
			datetime = System.DateTime.Now;
		}
		
		/// <summary>
		/// Randomly select simulation parameters. 
		/// </summary>
		public void Randomize() {
			title = "Random Settings";
			numberOfTests = 3;
			environmentName = EnvLoader.RandomEnvironmentName();
			robotName = BotLoader.RandomRobotName();
			navigationAssemblyName = NavLoader.RandomPluginName();
			randomizeDestination = true;
			continueOnNavObjectiveComplete = true;
			continueOnRobotIsStuck = true;
		}
	}

	/// <summary>
	/// Singleton pattern.
	/// </summary>
	public static Simulation Instance;
	
	
	/* ### Static  Properties ### */
	
	/// <summary>
	/// Gets the simulation state.
	/// </summary>
	/// <value>The current simulation state.</value>
	public static State state {get; private set;}
	
	/// <summary>
	/// Exhibition mode will run continually and 
	/// randomly choose camera perspectives and simulation settings.
	/// </summary>
	public static bool exhibitionMode;
	
	/// <summary>
	/// Gets or sets the settings for the current simulation.
	/// </summary>
	/// <value>The settings for the current (active) simulation.</value>
	public static Settings settings {
		get { return _settings; }
		set {
			_settings.active = false;
			_settings = value;
			_settings.active = true;
		}
	}

	/// <summary>
	/// List of settings to iterate through in batch mode.
	/// </summary>
	public static List<Settings> batch = new List<Settings>();
	
	/// <summary>
	/// Gets the simulation number (index in batch list, 1 to batch.Count).
	/// </summary>
	/// <value>The simulation number.</value>
	public static int simulationNumber {
		get; private set;
	}
	
	/// <summary>
	/// Gets the current test number (1 to settings.numberOfTests).
	/// </summary>
	/// <value>The current test number.</value>
	public static int testNumber {
		get; private set;
	}
	
	/// <summary>
	/// Gets reference to the robot in the current simulation.
	/// </summary>
	/// <value>The robot.</value>
	public static Robot robot {
		get { return _robot; }
		private set {
			if(_robot) _robot.transform.Recycle();
			_robot = value;
			robot.destination = destination.transform;
		}
	}
	
	// Reference to the environment
	/// <summary>
	/// Gets or sets reference to the environment in the current simulation.
	/// </summary>
	/// <value>The environment.</value>
	public static GameObject environment {
		get {
			return _environment;
		}
		set {
			if (isRunning) StopSimulation();
			if (_environment) _environment.transform.Recycle();
			_environment = value;
			SetBounds();
		}
	}
	
	// Reference to the destination
	/// <summary>
	/// Gets reference to the destination.
	/// </summary>
	/// <value>The destination.</value>
	public static GameObject destination { 
		get; private set; 
	}
	
	// Simulation states
	/// <summary>
	/// Gets a value indicating whether this <see cref="Simulation"/> has not yet started.
	/// </summary>
	/// <value><c>true</c> if pre simulation; otherwise, <c>false</c>.</value>
	public static bool isInactive {
		get { return state == State.inactive; }
	}
	/// <summary>
	/// Gets a value indicating whether this <see cref="Simulation"/> is running.
	/// </summary>
	/// <value><c>true</c> if is running; otherwise, <c>false</c>.</value>
	public static bool isRunning {
		get { return state == State.simulating; }
	}
	/// <summary>
	/// Gets a value indicating whether this <see cref="Simulation"/> is stopped.
	/// </summary>
	/// <value><c>true</c> if is stopped; otherwise, <c>false</c>.</value>
	public static bool isStopped {
		get { return state == State.stopped; }
	}
	/// <summary>
	/// Gets a value indicating whether this <see cref="Simulation"/> is finished.
	/// </summary>
	/// <value><c>true</c> if is finished; otherwise, <c>false</c>.</value>
	public static bool isFinished {
		get { return state == State.end; }
	}
	
	/// <summary>
	/// The simulation bounds described as a cube. This is the search
	/// space indicated to INavigation.
	/// </summary>
	public static Bounds bounds;
	
	/// <summary>
	/// If true, simulation will be logged to a file via Log class.
	/// </summary>
	public static bool loggingEnabled = true;
	
	// is the simulation paused?
	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="Simulation"/> is paused.
	/// </summary>
	/// <value><c>true</c> if paused; otherwise, <c>false</c>.</value>
	public static bool paused {
		get { return _paused; }
		set {
			_paused = value;
			if (_paused) Time.timeScale = 0f;
			else Time.timeScale = timeScale;
		}
	}

	/// <summary>
	/// Time (in seconds) since robot started searching for destination.
	/// </summary>
	/// <value>The simulation time.</value>
	public static float time {
		get {
			if (isRunning) _stopTime = Time.time;
			return _stopTime - _startTime;
		}
	}
	
	/// <summary>
	/// Gets or sets the time scale.
	/// </summary>
	/// <value>The time scale.</value>
	public static float timeScale {
		get { return _timeScale; }
		set {
			_timeScale = value;
			Time.timeScale = value;
		}
	}

	// Time variables used to calculate Simulation.time
	private static float _startTime;
	private static float _stopTime;
	
	private static Settings _settings;
	private static Robot _robot;
	private static GameObject _environment;
	private static bool _paused;
	private static float _timeScale = 1f;
	
	/* ### Static Methods ### */
	
	/// <summary>
	/// Begin simulating.
	/// </summary>
	public static void Begin() {
		simulationNumber = 0;
		NextSimulation();
	}
	
	/// <summary>
	/// Run the next simulation in batch.
	/// </summary>
	public static void NextSimulation() {
		// stop current simulation
		if (state == State.simulating) {
			Halt(StopCode.Unspecified);
		}
		// next in batch
		simulationNumber++;
		if (simulationNumber >= batch.Count) {
			// end of batch
			Halt(StopCode.Unspecified);
			state = State.end;
			return;
		}
		// load simulation settings
		settings = batch[simulationNumber-1];
		Log.Settings();
		// load environment
		EnvLoader.SearchForEnvironments();
		environment = EnvLoader.LoadEnvironment(settings.environmentName);
		destination.transform.position = RandomInBounds();
		// load robot
		Camera.main.transform.parent = null;
		BotLoader.SearchForRobots();
		robot = BotLoader.LoadRobot(settings.robotName);
		robot.navigation = NavLoader.LoadPlugin(settings.navigationAssemblyName);
		// start the simulation
		testNumber = 0;
		NextTest();
	}
	
	/// <summary>
	/// Stops the current test and starts the next test in current simulation.
	/// </summary>
	/// <param name="code">Code.</param>
	public static void NextTest() {
		if (testNumber+1 >= settings.numberOfTests) {
			
		}
		// start test routine
		if (state != State.starting) Instance.StartCoroutine(StartTestRoutine());
	}
	
	/// <summary>
	/// Halt simulation and write log to file. 
	/// </summary>
	/// <param name="code">Reason for halt.</param>
	public static void Halt(StopCode code) {
		// stop logging
		Log.Stop(code);
		// freeze the robot
		if (robot) {
			robot.rigidbody.velocity = Vector3.zero;
			robot.rigidbody.angularVelocity = Vector3.zero;
			robot.moveEnabled = false;
		}
		// set simulation state
		state = State.stopped;
	}
	
	
	/// <summary>
	/// 
	/// </summary>
	private static void StopTest() {
		
	}
	
	/// <summary>
	/// Stops the simulation.
	/// </summary>
	private static void StopSimulation() {
		if (robot) {
			robot.rigidbody.velocity = Vector3.zero;
			robot.rigidbody.angularVelocity = Vector3.zero;
			robot.moveEnabled = false;
		}
		state = State.stopped;
	}
	

	/// <summary>
	/// Stop all simulations.
	/// </summary>
	public static void End() {
		if (robot) {
			robot.rigidbody.velocity = Vector3.zero;
			robot.moveEnabled = false;
		}
		state = State.end;
		Log.Stop(0);
		if (exhibitionMode) {
			if (batch.Count > 10) {
				batch.RemoveAt(0);
			}
			settings = new Settings();
			settings.Randomize();
			batch.Add(settings);
			Begin();
		}
	}
	
	// Return a random position inside the simulation bounds
	/// <summary>
	/// Return a random position inside the simulation bounds, but 
	/// not inside any physical objects.
	/// </summary>
	/// <returns>Random position inside simulation bounds.</returns>
	public static Vector3 RandomInBounds() {
		Vector3 v = bounds.min;
		v.x += Random.Range(0f, bounds.max.x);
		v.y += bounds.max.y;
		v.z += Random.Range(0f, bounds.max.z);
		RaycastHit hit;
		if (Physics.Raycast(v, Vector3.down, out hit, 100f)) {
			v = hit.point + hit.normal;
		}
		return v;
	}
	
	// Routine for starting a new test
	private static IEnumerator StartTestRoutine() {
		state = State.starting;
		Halt(0);
		CamController.Instance.OnTestEnd();
		yield return new WaitForSeconds(1f);
		// randomly place the robot
		if (settings.randomizeOrigin) {
			robot.Reset();
			robot.transform.position = RandomInBounds();
			robot.transform.rotation = Quaternion.identity;
		}
			
		// randomly place the destination
		if (settings.randomizeDestination)
			destination.transform.position = RandomInBounds();
			
		yield return new WaitForSeconds(1f);
		
		CamController.Instance.OnTestStart();
		testNumber++;
		
		yield return new WaitForSeconds(1f);
		
		_startTime = Time.time;
		state = State.simulating;
		if (loggingEnabled) Log.Start();
		robot.moveEnabled = true;
		robot.NavigateToDestination();
	}
	
	// Set the simulation bounds to encapsulate all renderers in scene
	private static void SetBounds() {
		bounds = new Bounds(Vector3.zero, Vector3.zero);
		foreach(Renderer r in environment.GetComponentsInChildren<Renderer>())
			bounds.Encapsulate(r.bounds);
	}

	/** Instance Methods **/
	
	// Called on gameobject created
	void Awake() {
		// singleton pattern (can only be one Instance of Simulation)
		if (Instance) {
			Destroy(this.gameObject);
		}
		else {
			Instance = this;
		}
		_settings = new Settings();
	}
	
	// Called on the first frame
	void Start() {
		destination = GameObject.Find("Destination");
	}
	
	// Called every frame
	void Update() {
		if (isRunning) {
			// check for conditions to end the test
			if (robot.atDestination && settings.continueOnNavObjectiveComplete) {
				Debug.Log("Simulation: nav objective complete!");
				Halt(StopCode.RobotReachedDestination);
			}
			else if (robot.isStuck && settings.continueOnRobotIsStuck) {
				Debug.LogWarning("Simulation: Robot appears to be stuck! Skipping test.");
				Halt(StopCode.RobotIsStuck);
			}
			else if (settings.maximumTestTime > 0 && time > settings.maximumTestTime) {
				Debug.LogWarning("Simulation: Max test time exceeded! Skipping test.");
				Halt(StopCode.MaxTimeExceeded);
			}

		}
	}

	// Called every frame for drawing Gizmos
	void OnDrawGizmos() {
		Gizmos.DrawWireCube(bounds.center, bounds.size);
	}
	
	// called before application shuts down
	void OnApplicationQuit() {
		Log.Stop(StopCode.Unspecified);
	}
}
