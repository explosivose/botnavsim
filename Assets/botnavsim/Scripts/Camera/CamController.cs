using UnityEngine;
using System.Collections;

public class CamController : Singleton<CamController> {

	public float mouseSensitivity = 10f;
	
	public LayerMask maskNormal;
	public LayerMask maskBotData;
	public LayerMask maskHybrid;
	
	public enum Perspective {
		Birdseye,
		ThirdPerson,
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
				break;
			case Perspective.ThirdPerson:
				_camera.camera.orthographic = false;
				_camera.camera.fieldOfView = 60f;
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
	
	// Increment perspective circularly
	public void CyclePerspective() {
		perspective++;
		if (perspective > Perspective.FirstPerson) perspective = Perspective.Birdseye;
	}
	
	// Increment rendermode circularly
	public void CycleRenderMode() {
		renderMode++;
		if (renderMode > RenderMode.BotData) renderMode = RenderMode.Hybrid;
	}
	
	private Perspective _p;
	private RenderMode _r;
	private Camera _camera;
	private Robot _robot;
	private float _3rdPersonDist = 10f;
	private Vector3 _3rdPersonDir = Vector3.one;
	private Vector3 _skyPoint;
	private Quaternion _skyRotation;
	private bool _skyViewNeedsUpdating;
	
	void Awake() {
		_camera = GetComponent<Camera>();
	}
	
	void Update() {
		_robot = Simulation.robot;
		
		if (Simulation.isRunning) {
			PerspectiveUpdate();
		}
		else if (Simulation.exhibitionMode) {
			SkyPerspective();
		}
		
		RenderModeUpdate();
	}
	
	void RenderModeUpdate() {
		if (Simulation.isReady && renderMode != RenderMode.Normal) 
			Simulation.robot.navigation.DrawDebugInfo();
	}
	
	void PerspectiveUpdate() {
		_skyViewNeedsUpdating = true;
		switch(perspective) {
		case Perspective.Birdseye:
			BirdseyePerspective();
			break;
		case Perspective.ThirdPerson:
			ThirdPersonPerspective();
			break;
		case Perspective.FirstPerson: 
			FirstPersonPerspective();
			break;
		}
	}

	void BirdseyePerspective() {
		
		// size orthographic camera to fit robot and destination in shot
		float size = Simulation.robot.distanceToDestination * 0.75f;
		size = Mathf.Max(size, 10f);
		_camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, size, 4f);
		
		// calculate position above and between robot and target
		Vector3 targetPosition = (_robot.position + Simulation.destination.transform.position)/2f;
		targetPosition += Vector3.up * 100f;
		// smooth move to position
		_camera.transform.position = Vector3.Slerp(
			_camera.transform.position, 
			targetPosition, 
			Time.deltaTime * 2f
			);
		
		// look down
		Quaternion targetRotation = Quaternion.LookRotation(Vector3.down);
		_camera.transform.rotation = Quaternion.Slerp(
			_camera.transform.rotation, 
			targetRotation, 
			Time.deltaTime * 1f
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
		if (Physics.SphereCast(ray, 0.5f, out hit, _3rdPersonDist, maskNormal)) {
			targetPosition = hit.point + hit.normal;
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
	
	void FirstPersonPerspective() {
		
	}
	
	void SkyPerspective() {
		if (_skyViewNeedsUpdating) {
			_skyPoint = Simulation.RandomInBounds();
			_skyPoint.y = Simulation.bounds.max.y;
			_skyPoint.z = Simulation.bounds.min.z;
			_skyRotation = Quaternion.LookRotation(Simulation.bounds.center - _skyPoint);
			
			perspective = Perspective.ThirdPerson;
			_skyViewNeedsUpdating = false;
		}
		// smooth move to position
		_camera.transform.position = Vector3.Slerp(
			_camera.transform.position, 
			_skyPoint, 
			Time.deltaTime * 4f
			);
		// look at robot
		_camera.transform.rotation = Quaternion.Slerp(
			_camera.transform.rotation, 
			_skyRotation, 
			Time.deltaTime * 4f
			);
	}
}
