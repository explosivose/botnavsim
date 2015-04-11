using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

/// <summary>
/// Controls the camera orientation and render modes according to registered 
/// ViewMode and user input. ViewMode are registered in modes by BotNavSim 
/// state manager classes i.e. Simulation, LogLoader, RobotEditor, EnvironmentEditor
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
		Hybrid,
		Normal,
		BotData
	}
	
	// static members
	// ~-~-~-~-~-~-~-
	
	/// <summary>
	/// Gets the instance.
	/// </summary>
	/// <value>The instance.</value>
	public static CamController Instance {
		get; private set;
	}
	
	/// <summary>
	/// Gets the current ViewMode.
	/// </summary>
	/// <value>The view mode.</value>
	public static ViewMode viewMode {
		get {
			return _modes[_mode];
		}
	}
	
	/// <summary>
	/// Gets or sets the RenderMode in use. RenderMode determines which render layers are drawn. 
	/// </summary>
	/// <value>The render mode.</value>
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
	/// Gets the current area of interest.
	/// </summary>
	/// <value>The area.</value>
	public static IObservable area {
		get { return _areas[_area]; }
	}
	
	/// <summary>
	/// Gets a readonly view mode list.
	/// </summary>
	/// <value>The view mode list.</value>
	public static ReadOnlyCollection<ViewMode> viewModeList {
		get { return _modes.AsReadOnly(); }
	}
	
	private static List<ViewMode>		_modes;		// list of available ViewMode to use 
	private static List<IObservable>	_areas;		// list of areas to point the camera at
	private static int _mode; 						// index for _modes
	private static int _area; 						// index for _areas
	private static Camera _camera;
	private static RenderMode _renderMode;
	
	// static methods
	// ~-~-~-~-~-~-~-
	
	// static constructor
	static CamController() {
		// modes list will always have ViewMode.FreeMovement, and any other modes registered by AddViewMode
		_modes = new List<ViewMode>();
		_modes.Add(ViewMode.FreeMovement);
		_areas = new List<IObservable>();
	}

	/// <summary>
	/// Adds an area of interest for the camera to look at.
	/// </summary>
	/// <param name="b">An area of interest defined by a bounding box.</param>
	public static void AddAreaOfInterest(IObservable area) {
		if (!_areas.Contains(area)) _areas.Add(area);
	}
	
	/// <summary>
	/// Removes an area of interest for the camera to look at.
	/// </summary>
	/// <param name="b">An area of interest defined by a bounding box.</param>
	public static void RemoveAreaOfInterest(IObservable area) {
		if (_areas.Count > 1) _areas.Remove(area);
		if (_area >= _areas.Count) _area = 0;
	}
	
	/// <summary>
	/// Clears the area list.
	/// </summary>
	public static void ClearAreaList() {
		_areas.Clear();
		_areas.Add(Simulation.Instance);
		_area = 0;
	}
	
	/// <summary>
	/// Go to next area of interest
	/// </summary>
	public static void CyclePointOfInterest() {
		_area = (++_area) % _areas.Count;
	}
	
	/// <summary>
	/// Use next RenderMode
	/// </summary>
	public static void CycleRenderMode() {
		int length = System.Enum.GetValues(typeof(RenderMode)).Length;
		renderMode = (RenderMode)((int)(++renderMode) % length);
	}
	
	/// <summary>
	/// Select random RenderMode
	/// </summary>
	public static void RandomRenderMode() {
		System.Array values = System.Enum.GetValues(typeof(RenderMode));
		renderMode = (RenderMode)values.GetValue(Random.Range(0,values.Length-1));
	}
	
	
	/// <summary>
	/// Adds a ViewMode to the list of camera modes to use.
	/// </summary>
	/// <param name="mode">Mode.</param>
	public static void AddViewMode(ViewMode mode) {
		if (!_modes.Contains(mode)) _modes.Add(mode);
	}
	
	/// <summary>
	/// Removes a ViewMode from the list of camera modes to use. 
	/// </summary>
	/// <param name="mode">Mode.</param>
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
	/// Use next ViewMode in modes
	/// </summary>
	public static void CycleViewMode() {
		SetViewMode((++_mode) % _modes.Count);
	}
	
	/// <summary>
	/// Use random ViewMode in modes
	/// </summary>
	public static void RandomViewMode() {
		SetViewMode(Random.Range(0, _modes.Count-1));
	}
	

	
	// instance members (defined in Unity Inspector)
	// ~-~-~-~-~-~-~-~-
	
	public float mouseSensitivity;			// multiplies speed of mouse axis input 
	public float freeMoveSpeed;
	public float freeMoveSpeedShiftMult;
	
	public LayerMask maskCameraCollision;	// layermasks for rendering modes
	public LayerMask maskNormal;
	public LayerMask maskBotData;
	public LayerMask maskHybrid;
	
	private float _birdseyeDist = 0f;
	private float _3rdPersonDist = 10f;
	private Vector3 _3rdPersonDir = Vector3.one;
	private Vector3 _1stPersonDir = Vector3.one;
	
	// instance methods
	// ~-~-~-~-~-~-~-~-
	
	/// <summary>
	/// Raises the test start event. (handles camera exhibitionMode behaviour)
	/// </summary>
	public void OnTestStart() {
		if (Simulation.exhibitionMode) {
			RandomViewMode();
			RandomRenderMode();
			CyclePointOfInterest();
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
		_areas.Add(Simulation.Instance);
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
			if (Input.GetKeyDown(KeyCode.Space)) CyclePointOfInterest();
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
		size = Mathf.Max(size, 10f);
		_camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, size, Time.deltaTime * 4f);
		Vector3 targetPosition = area.bounds.center + Vector3.up * 100f;
		
		// smooth move to position
		_camera.transform.position = Vector3.Slerp(
			_camera.transform.position, 
			targetPosition, 
			Time.deltaTime * 4f
			);
		
		// look down
		Quaternion targetRotation = Quaternion.LookRotation(Vector3.down);
		_camera.transform.rotation = Quaternion.Slerp(
			_camera.transform.rotation, 
			targetRotation, 
			Time.deltaTime * 8f
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
		float y = Input.GetAxisRaw("Vertical") * freeMoveSpeed * b * Time.deltaTime;
		float x = Input.GetAxisRaw("Horizontal") * freeMoveSpeed * b * Time.deltaTime;
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
			Time.deltaTime * 4f
			);
		
		// look at robot
		Quaternion targetRotation = Quaternion.LookRotation(area.bounds.center - targetPosition);
		_camera.transform.rotation = Quaternion.Slerp(
			_camera.transform.rotation, 
			targetRotation, 
			Time.deltaTime * 8f
			);
	}
	
}
