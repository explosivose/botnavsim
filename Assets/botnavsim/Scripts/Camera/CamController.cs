using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

/// <summary>
/// Controls the camera orientation and render modes according to ViewMode in viewModeList 
/// user input. viewModeList is maintained by the state manager class in control
/// i.e. Simulation, LogLoader, RobotEditor, EnvironmentEditor
/// </summary>
[RequireComponent(typeof(Camera))]
public class CamController : MonoBehaviour {

	// class types
	// ~-~-~-~-~-~-
	
	public enum ViewMode {
		/// <summary>
		/// Top-down orthographic view. 
		/// </summary>
		Birdseye,
		
		/// <summary>
		/// Camera orbit around a point of interest. 
		/// </summary>
		Orbit,
		
		/// <summary>
		/// Camera attached to an object i.e. robot. 
		/// </summary>
		Mounted,
		
		/// <summary>
		/// A stationary camera position
		/// </summary>
		Static,
		
		/// <summary>
		/// Free-flying camera. 
		/// </summary>
		FreeMovement
	}
	
	public enum RenderMode {
		/// <summary>
		/// Display BotData and scene geometry.
		/// </summary>
		Hybrid,
		
		/// <summary>
		/// Display scene geometry only.
		/// </summary>
		Normal,
		
		/// <summary>
		/// Display the robot data (drawn with Draw class) only.
		/// </summary>
		BotData
	}
	
	/// <summary>
	/// A blank IObservable implementation (used when the _areas list is empty)
	/// </summary>
	private class StubObservable : IObservable {
		public StubObservable() {
			bounds = new Bounds();
		}
		public string name {
			get { return Strings.projectTitle; }
		}
		
		public Bounds bounds {
			get; private set;
		}
	}
	
	// static members
	// ~-~-~-~-~-~-~-
	
	/// <summary>
	/// Gets the MonoBehaviour instance.
	/// </summary>
	public static CamController Instance {
		get; private set;
	}
	
	/// <summary>
	/// Gets the current ViewMode.
	/// </summary>
	public static ViewMode viewMode {
		get {
			return _modes[_mode];
		}
	}
	
	/// <summary>
	/// Gets or sets the RenderMode in use. RenderMode determines which render layers are drawn. 
	/// </summary>
	public static RenderMode renderMode {
		get {
			return _renderMode;
		}
		set {
			_renderMode = value;
			switch(_renderMode) {
			case RenderMode.Normal:
			default:
				_camera.cullingMask = Instance.maskNormal;
				break;
			case RenderMode.BotData:
				_camera.cullingMask = Instance.maskBotData;
				break;
			case RenderMode.Hybrid:
				_camera.cullingMask = Instance.maskHybrid;
				break;
			}
		}
	}
	
	/// <summary>
	/// Gets the current observable area of interest.
	/// </summary>
	public static IObservable area {
		get { 
			if (_areas.Count > 0)
				return _areas[_area]; 
			else 
				return _stub;
		}
	}
	
	/// <summary>
	/// List of all <see cref="ViewMode"/> in effect.
	/// </summary>
	public static ReadOnlyCollection<ViewMode> viewModeList {
		get { return _modes.AsReadOnly(); }
	}
	
	private static List<ViewMode>		_modes;		// list of available ViewMode to use 
	private static List<IObservable>	_areas;		// list of areas to point the camera at
	private static IObservable			_stub;		// a stub object to use when _areas list is empty
	private static int _mode; 						// index for _modes
	private static int _area; 						// index for _areas
	private static Camera _camera;					// reference to the camera component attached to the MonoBehaviour instance
	private static RenderMode _renderMode;			// the current RenderMode
	
	// static methods
	// ~-~-~-~-~-~-~-
	
	// static constructor
	static CamController() {
		// modes list will always have ViewMode.FreeMovement, and any other modes registered by AddViewMode
		_modes = new List<ViewMode>();
		_modes.Add(ViewMode.FreeMovement);
		_areas = new List<IObservable>();
		_stub = new StubObservable();
	}

	/// <summary>
	/// Adds an area of interest to the list of areas for the camera to observe.
	/// </summary>
	/// <param name="area">An area of interest defined by a bounding box.</param>
	public static void AddAreaOfInterest(IObservable area) {
		if (!_areas.Contains(area)) _areas.Add(area);
	}
	
	/// <summary>
	/// Removes an area of interest from the list of areas for the camera to observe.
	/// </summary>
	/// <param name="area">An area of interest defined by a bounding box.</param>
	public static void RemoveAreaOfInterest(IObservable area) {
		if (_areas.Count > 1) _areas.Remove(area);
		if (_area >= _areas.Count) _area = 0;
	}
	
	/// <summary>
	/// Clears the area list.
	/// </summary>
	public static void ClearAreaList() {
		_areas.Clear();
		_area = 0;
	}
	
	/// <summary>
	/// Sets the area of interest by index.
	/// </summary>
	/// <param name="index">Index for area list.</param>
	public static void SetAreaOfInterest(int index) {
		// ignore out of range indexes
		if (index < 0 || index >= _areas.Count) {
			Debug.LogWarning("SetAreaOfInterest index param out of range. Ignoring.");
			return;
		}
		_area = index;
	}
	
	/// <summary>
	/// Sets the area of interest by object (does nothing if the obj is not in the areas list.
	/// </summary>
	/// <param name="obj">Object.</param>
	public static void SetAreaOfInterest(IObservable obj) {
		int index = _areas.IndexOf(obj);
		if (index >= 0) _area = index;
	}
	
	/// <summary>
	/// Set area to next area of interest (circular index of areas list)
	/// </summary>
	public static void CycleAreaOfInterest() {
		_area = (++_area) % _areas.Count;
	}
	
	/// <summary>
	/// Set renderMode to next RenderMode (circular index of render modes).
	/// </summary>
	public static void CycleRenderMode() {
		int length = System.Enum.GetValues(typeof(RenderMode)).Length;
		renderMode = (RenderMode)((int)(++renderMode) % length);
	}
	
	/// <summary>
	/// Set renderMode to random RenderMode. 
	/// </summary>
	public static void RandomRenderMode() {
		System.Array values = System.Enum.GetValues(typeof(RenderMode));
		renderMode = (RenderMode)values.GetValue(Random.Range(0,values.Length-1));
	}
	
	
	/// <summary>
	/// Adds a <see cref="ViewMode"/> to <see cref="viewModeList"/>.
	/// </summary>
	/// <param name="mode">The view mode enum to add.</param>
	public static void AddViewMode(ViewMode mode) {
		if (!_modes.Contains(mode)) _modes.Add(mode);
	}
	
	/// <summary>
	/// Removes a <see cref="ViewMode"/> from <see cref="viewModeList"/>. 
	/// </summary>
	/// <param name="mode">The view mode enum to remove.</param>
	public static void RemoveViewMode(ViewMode mode) {
		if (mode != ViewMode.FreeMovement) _modes.Remove(mode);
		if (_mode > _modes.Count) {
			SetViewMode(0);
		}
	}
	
	/// <summary>
	/// Clears the view mode list.
	/// </summary>
	public static void ClearViewModeList() {
		_modes.Clear();
		_modes.Add(ViewMode.Static);
		SetViewMode(0);
	}
	
	/// <summary>
	/// Sets the view mode.
	/// </summary>
	/// <param name="mode">ViewMode selection. If selection is not in viewModeList then it will fail.</param>
	public static void SetViewMode(ViewMode mode) {
		int index = _modes.IndexOf(mode);
		SetViewMode(index);
	}
	
	/// <summary>
	/// Sets the view mode.
	/// </summary>
	/// <param name="index">Index for selecting from viewModeList.</param>
	public static void SetViewMode(int index) {
		// ignore out of range indexes
		if (index < 0 || index >= _modes.Count) {
			Debug.LogWarning("SetViewMode index param out of range. Ignoring.");
			return;
		}
		_mode = index;
		_camera.transform.parent = null;
		// set camera properties for new ViewMode
		switch (_modes[_mode]) {
		case ViewMode.Mounted:
			_camera.orthographic = false;
			_camera.fieldOfView = 90f;
			_camera.transform.parent = Simulation.robot.cameraMount;
			_camera.transform.localPosition = Vector3.zero;
			_camera.transform.localRotation = Quaternion.identity;
			break;
		case ViewMode.Orbit:
		case ViewMode.FreeMovement:
			_camera.orthographic = false;
			_camera.fieldOfView = 60f;
			break;
		case ViewMode.Static:
			_camera.orthographic = false;
			_camera.fieldOfView = 60f;
			Bounds b = Simulation.Instance.bounds;
			Vector3 position = b.max;
			position.y *= 2f;
			position.x = Random.value < 0.5f ? b.min.x : b.max.x;
			position.z = Random.value < 0.5f ? b.min.z : b.max.z;
			Instance.transform.position = position;
			Instance.transform.rotation = Quaternion.LookRotation(b.center - Instance.transform.position);
			break;
		default:
		case ViewMode.Birdseye:
			_camera.camera.orthographic = true;
			_camera.transform.parent = null;
			break;
		}
	}
	
	/// <summary>
	/// Set viewMode to the next ViewMode in viewModeList (circular index of viewModeList).
	/// </summary>
	public static void CycleViewMode() {
		SetViewMode((++_mode) % _modes.Count);
	}
	
	/// <summary>
	/// Set viewMode to random ViewMode in viewModeList.
	/// </summary>
	public static void RandomViewMode() {
		SetViewMode(Random.Range(0, _modes.Count-1));
	}
	

	
	// instance members (defined in Unity Editor)
	// ~-~-~-~-~-~-~-~-
	
	/// <summary>
	/// Multiplies speed of mouse axis input 
	/// </summary>
	public float mouseSensitivity;		
	
	/// <summary>
	/// Camera translation speed when in free movement mode
	/// </summary>
	public float freeMoveSpeed;		
	
	/// <summary>
	/// Camera translation speed multiplier when the SHIFT key is used
	/// </summary>
	public float freeMoveSpeedShiftMult;
	
	
	/// <summary>
	/// Camera collision mask to avoid placing camera inside objects.
	/// </summary>
	public LayerMask maskCameraCollision;
	
	/// <summary>
	/// Camera culling mask for RenderMode.Normal
	/// </summary>
	public LayerMask maskNormal;
	
	/// <summary>
	/// Camera culling mask for RenderMode.BotData
	/// </summary>
	public LayerMask maskBotData;
	
	/// <summary>
	/// Camera culling mask for RenderMode.Hybrid
	/// </summary>
	public LayerMask maskHybrid;
	
	
	private float _birdseyeDist = 0f;				// vertical distance offset for camera in birdseye mode
	private float _3rdPersonDist = 5f;				// distance offset for camera in orbit mode
	private Vector3 _3rdPersonDir = Vector3.one;	// direction offset for camera in orbit mode
	private Vector3 _1stPersonDir = Vector3.one;	// camera direction when in free movement mode
	
	// instance methods
	// ~-~-~-~-~-~-~-~-
	
	/// <summary>
	/// Raises the test start event. (handles camera exhibitionMode behaviour)
	/// </summary>
	public void OnTestStart() {
		if (Simulation.exhibitionMode) {
			RandomViewMode();
			RandomRenderMode();
			CycleAreaOfInterest();
		}
	}
	
	/// <summary>
	/// Raises the test end event. (handles camera exhibitionMode behaviour)
	/// </summary>
	public void OnTestEnd() {
		if (Simulation.exhibitionMode) {
			SetViewMode(viewModeList.IndexOf(ViewMode.Birdseye));
		}
	}

	// called when the instance enters the scene
	void Awake() {
		if (Instance == null) {	
			Instance = this;
		}
		else {
			Destroy(this);
		}
		_camera = GetComponent<Camera>();
	}
	
	// called on the first frame for this instance 
	void Start() {
		SetViewMode(0);
	}
	
	// called every frame for this instance
	void Update() {
		
		// set camera size on screen
		float w = UI_Toolbar.I.width/Screen.width;
		camera.rect = new Rect(0, 0, 1f-w, 1f);
		
		// ignore input unless mouse position is inside camera screen area
		if ( camera.pixelRect.Contains(Input.mousePosition) ) {
			if (Input.GetKeyDown(KeyCode.C)) CycleViewMode();
			if (Input.GetKeyDown(KeyCode.R)) CycleRenderMode();
			if (Input.GetKeyDown(KeyCode.Space)) CycleAreaOfInterest();
			// scrollwheel zoom
			_birdseyeDist -= Input.GetAxis("Mouse ScrollWheel") * 4f;
			// adjust third person distance with scroll wheel input
			_3rdPersonDist -= Input.GetAxis("Mouse ScrollWheel") * 4f;
			_3rdPersonDist = Mathf.Min(_3rdPersonDist, 20f);
			_3rdPersonDist = Mathf.Max(_3rdPersonDist, 1f);
			
			// move orbit direction vector while rightclick drag
			if (Input.GetMouseButton(1)) {
				float x = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
				float y = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
				Quaternion rotation = Quaternion.AngleAxis(-y, Vector3.up);
				_3rdPersonDir = rotation * _3rdPersonDir;
				rotation = Quaternion.AngleAxis(x, _camera.transform.right);
				_3rdPersonDir = rotation * _3rdPersonDir;
				
				rotation = Quaternion.AngleAxis(y, Vector3.up);
				_1stPersonDir = rotation * _1stPersonDir;
				rotation = Quaternion.AngleAxis(-x, _camera.transform.right);
				_1stPersonDir = rotation * _1stPersonDir;
			}
		}

		// choose update behaviour 
		switch(viewMode) {
		case ViewMode.Birdseye:
			BirdseyeUpdate();
			break;
		case ViewMode.FreeMovement:
			FreeMovementUpdate();
			break;
		case ViewMode.Orbit:
			OrbitUpdate();  
			break;
		case ViewMode.Mounted:
		case ViewMode.Static:
		default:
			break;
		}
		
		RenderModeUpdate();
		
	}
	
	/// <summary>
	/// Call INavigation.DrawDebugInfo as appropriate
	/// (Camera culling mask is handled in renderMode.set)
	/// </summary>
	void RenderModeUpdate() {
		if (Simulation.isRunning)
			if (renderMode != RenderMode.Normal) 
				Simulation.robot.navigation.DrawDebugInfo();
	}

	/// <summary>
	/// Place the camera above the area of interest
	/// </summary>
	void BirdseyeUpdate() {
		
		float size = Mathf.Max(area.bounds.size.x/2f, area.bounds.size.z/2f);
		size += _birdseyeDist;
		size = Mathf.Max(size, 5f);
		_camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, size, Time.unscaledDeltaTime * 4f);
		Vector3 targetPosition = area.bounds.center + Vector3.up * 10f;
		
		// smooth move to position
		_camera.transform.position = Vector3.Slerp(
			_camera.transform.position, 
			targetPosition, 
			Time.unscaledDeltaTime * 4f
			);
		
		// look down
		Quaternion targetRotation = Quaternion.LookRotation(Vector3.down);
		_camera.transform.rotation = Quaternion.Slerp(
			_camera.transform.rotation, 
			targetRotation, 
			Time.unscaledDeltaTime * 8f
			);
	}
	
	/// <summary>
	/// Camera position and rotation controlled by axis keys and mouse axis
	/// </summary>
	void FreeMovementUpdate() {
		// rotate accoring to right click drag calculated in Update()
		transform.rotation = Quaternion.LookRotation(_1stPersonDir);
		// boost movement speed if LeftShift is held down
		float b = Input.GetKey(KeyCode.LeftShift) ? freeMoveSpeedShiftMult : 1f;
		// grab axis input
		float y = Input.GetAxisRaw("Z") * freeMoveSpeed * b * Time.unscaledDeltaTime;
		float x = Input.GetAxisRaw("X") * freeMoveSpeed * b * Time.unscaledDeltaTime;
		// modify position according to orientation (move forward/back, strafe left/right)
		Vector3 pos = transform.position;
		pos += transform.forward * y;
		pos += transform.right * x;
		transform.position = pos;
	}
	
	/// <summary>
	/// Camera orbit around the area of interest center
	/// </summary>
	void OrbitUpdate() {
		
		Vector3 targetPosition = area.bounds.center;
		// raycast from robot in a direction to avoid placing camera inside other objects
		Ray ray = new Ray(area.bounds.center, _3rdPersonDir);
		RaycastHit hit;
		// if raycast hit, put camera on hit object
		//if (Physics.SphereCast(ray, 0.5f, out hit, _3rdPersonDist, maskCameraCollision)) {
		//	targetPosition = hit.point + hit.normal;
		//	Debug.DrawLine(ray.origin, hit.point, Color.red);
		//}
		// else place camera at _3rdPersonDist away from robot
		//else {
			targetPosition = area.bounds.center + _3rdPersonDir * _3rdPersonDist;
		//}
		
		// smooth move to position
		_camera.transform.position = Vector3.Slerp(
			_camera.transform.position, 
			targetPosition, 
			Time.unscaledDeltaTime * 4f
			);
		
		// look at robot
		Quaternion targetRotation = Quaternion.LookRotation(area.bounds.center - targetPosition);
		_camera.transform.rotation = Quaternion.Slerp(
			_camera.transform.rotation, 
			targetRotation, 
			Time.unscaledDeltaTime * 8f
			);
	}
	
}
