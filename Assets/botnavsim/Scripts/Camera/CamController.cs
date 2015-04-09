using UnityEngine;
using System.Collections;

public class CamController : Singleton<CamController> {

	public float mouseSensitivity = 10f;
	
	public LayerMask maskCameraCollision;
	public LayerMask maskNormal;
	public LayerMask maskBotData;
	public LayerMask maskHybrid;
	
	public enum Perspective {
		Birdseye,
		ThirdPerson,
		Landscape,
		FirstPerson
	}
	
	public enum RenderMode {
		Hybrid,
		Normal,
		BotData
	}
	
	// Perspective state change behaviour
	public Perspective perspective {
		get {
			return _p;
		}
		set {
			_p = value;
			switch (_p) {
			case Perspective.FirstPerson:
				_camera.orthographic = false;
				_camera.fieldOfView = 120f;
				_camera.transform.parent = Simulation.robot.transform;
				_camera.transform.localPosition = Vector3.zero;
				_camera.transform.localRotation = Quaternion.identity;
				break;
			case Perspective.ThirdPerson:
				_camera.camera.orthographic = false;
				_camera.camera.fieldOfView = 60f;
				break;
			case Perspective.Landscape:
				_camera.camera.orthographic = false;
				_camera.camera.fieldOfView = 60f;
				Bounds b = Simulation.bounds;
				Vector3 position = b.max;
				position.y *= 2f;
				position.x = Random.value < 0.5f ? b.min.x : b.max.x;
				position.z = Random.value < 0.5f ? b.min.z : b.max.z;
				transform.position = position;
				transform.rotation = Quaternion.LookRotation(b.center - transform.position);
				break;
			default:
			case Perspective.Birdseye:
				_camera.camera.orthographic = true;
				_camera.transform.parent = null;
				break;
			}
		}
	}
	
	// Render mode state change behaviour
	public RenderMode renderMode {
		get {
			return _r;
		}
		set {
			_r = value;
			switch(_r) {
			case RenderMode.Normal:
			default:
				_camera.cullingMask = maskNormal;
				break;
			case RenderMode.BotData:
				_camera.cullingMask = maskBotData;
				break;
			case RenderMode.Hybrid:
				_camera.cullingMask = maskHybrid;
				break;
			}
		}
	}
	
	/// <summary>
	/// Raises the test start event. (handles camera exhibitionMode behaviour)
	/// </summary>
	public void OnTestStart() {
		if (Simulation.exhibitionMode) {
			RandomPerspective();
			RandomRenderMode();
		}
	}
	
	/// <summary>
	/// Raises the test end event. (handles camera exhibitionMode behaviour)
	/// </summary>
	public void OnTestEnd() {
		if (Simulation.exhibitionMode) {
			perspective = Perspective.Landscape;
		}
	}
	
	// Increment perspective circularly
	public void CyclePerspective() {
		int length = System.Enum.GetValues(typeof(Perspective)).Length;
		if ((int)perspective + 1 >= length) perspective = 0;
		else perspective++;
	}
	
	public void RandomPerspective() {
		System.Array values = System.Enum.GetValues(typeof(Perspective));
		perspective = (Perspective)values.GetValue(Random.Range(0,values.Length-1));
	}
	
	// Increment rendermode circularly
	public void CycleRenderMode() {
		int length = System.Enum.GetValues(typeof(RenderMode)).Length;
		if ((int)renderMode + 1 >= length) renderMode = 0;
		else renderMode++;
	}
	
	public void RandomRenderMode() {
		System.Array values = System.Enum.GetValues(typeof(RenderMode));
		renderMode = (RenderMode)values.GetValue(Random.Range(0,values.Length-1));
	}
	
	private Perspective _p;
	private RenderMode _r;
	private Camera _camera;
	private Robot _robot;
	private float _birdseyeDist = 0f;
	private float _3rdPersonDist = 10f;
	private Vector3 _3rdPersonDir = Vector3.one;

	
	void Awake() {
		_camera = GetComponent<Camera>();
	}
	
	void Update() {
		
		
		// set camera size on screen
		float x = UI_Toolbar.I.width/Screen.width;
		camera.rect = new Rect(0, 0, 1f-x, Screen.height);
		
		if ( camera.pixelRect.Contains(Input.mousePosition) ) {
			if (Input.GetKeyDown(KeyCode.C)) CyclePerspective();
			if (Input.GetKeyDown(KeyCode.R)) CycleRenderMode();
			// scrollwheel zoom
			_birdseyeDist -= Input.GetAxis("Mouse ScrollWheel") * 4f;
			// adjust third person distance with scroll wheel input
			_3rdPersonDist -= Input.GetAxis("Mouse ScrollWheel") * 4f;
			_3rdPersonDist = Mathf.Min(_3rdPersonDist, 20f);
			_3rdPersonDist = Mathf.Max(_3rdPersonDist, 1f);
		}

		
		if (BotNavSim.isSimulating) {
			_robot = Simulation.robot;
			PerspectiveUpdate();
			RenderModeUpdate();
		}
		
		if (BotNavSim.isViewingData) {
			BirdseyePerspective();
		}
		
	}
	
	void RenderModeUpdate() {
		if (renderMode != RenderMode.Normal) 
			Simulation.robot.navigation.DrawDebugInfo();
	}
	
	void PerspectiveUpdate() {

		switch(perspective) {
		case Perspective.Birdseye:
			BirdseyePerspective();
			break;
		case Perspective.ThirdPerson:
			ThirdPersonPerspective();
			break;
		}
	}

	void BirdseyePerspective() {
		
		
		
		Vector3 targetPosition = Vector3.up *100f;
		
		if (BotNavSim.isSimulating) {
			// size orthographic camera to fit robot and destination in shot
			float size = Simulation.robot.distanceToDestination * 0.75f;
			size += _birdseyeDist;
			size = Mathf.Max(size, 10f);
			_camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, size, 4f);
			
			// calculate position above and between robot and target
			targetPosition = (_robot.position + Simulation.destination.transform.position)/2f;
			targetPosition += Vector3.up * 100f;
		}
		
		if (BotNavSim.isViewingData) {
			// size orthographic camera to fit environment in shot
			float size = 50f;
			size += _birdseyeDist;
			_camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, size, 4f);
			
			// calculate position above and center of environment
			targetPosition = LogLoader.bounds.center;
			targetPosition += Vector3.up * 100f;
		}
		
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
	
	void ThirdPersonPerspective() {
		

		
		// move camera position based on input
		if (Input.GetMouseButton(1)) {
			float x = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
			float y = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
			Quaternion rotation = Quaternion.AngleAxis(-y, Vector3.up);
			_3rdPersonDir = rotation * _3rdPersonDir;
			rotation = Quaternion.AngleAxis(x, _camera.transform.right);
			_3rdPersonDir = rotation * _3rdPersonDir;
		}
		
		Vector3 targetPosition = _robot.position;
		// raycast from robot in a direction to avoid placing camera inside other objects
		Ray ray = new Ray(_robot.position, _3rdPersonDir);
		RaycastHit hit;
		// if raycast hit, put camera on hit object
		if (Physics.SphereCast(ray, 0.5f, out hit, _3rdPersonDist, maskCameraCollision)) {
			targetPosition = hit.point + hit.normal;
			Debug.DrawLine(ray.origin, hit.point, Color.red);
		}
		// else place camera at _3rdPersonDist away from robot
		else {
			targetPosition = _robot.position + _3rdPersonDir * _3rdPersonDist;
		}
		
		// smooth move to position
		_camera.transform.position = Vector3.Slerp(
			_camera.transform.position, 
			targetPosition, 
			Time.deltaTime * 4f
			);
		
		// look at robot
		Quaternion targetRotation = Quaternion.LookRotation(_robot.position - targetPosition);
		_camera.transform.rotation = Quaternion.Slerp(
			_camera.transform.rotation, 
			targetRotation, 
			Time.deltaTime * 8f
			);
	}
	
}
