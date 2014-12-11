using UnityEngine;
using System.Collections;

public class CameraPerspective : MonoBehaviour {

	public enum Perspective {
		FirstPerson,
		ThirdPerson,
		Birdseye
	}
	
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
				_camera.transform.parent = Simulation.robot.transform;
				break;
			default:
			case Perspective.Birdseye:
				_camera.camera.orthographic = true;
				_camera.transform.parent = null;
				
				break;
			}
		}
	}
	
	private Perspective _p;
	private Camera _camera;
	private Vector3 targetPosition;
	private Quaternion targetRotation;
	
	public void CyclePerspective() {
		perspective++;
		if (perspective > Perspective.Birdseye) perspective = Perspective.FirstPerson;
	}
	
	void Awake() {
		_camera = GetComponent<Camera>();
	}
	
	void Update() {
		if (!Simulation.isRunning) return;
		Transform bot = Simulation.robot.transform;
		switch (_p) {
		case Perspective.FirstPerson:
			targetPosition = bot.position + Vector3.up * 1.5f;
			if (bot.rigidbody.velocity.magnitude > 0.1f)
			targetRotation = Quaternion.LookRotation(bot.rigidbody.velocity);
			break;
		case Perspective.ThirdPerson:
			_camera.orthographic = false;
			targetPosition = bot.position;
			targetPosition -= bot.rigidbody.velocity.normalized * 5f;
			targetPosition += bot.up * 5f;
			targetRotation = Quaternion.LookRotation(bot.position - targetPosition);
			break;
		default:
		case Perspective.Birdseye:
			targetPosition = (bot.position + Simulation.destination.transform.position)/2f;
			targetPosition += Vector3.up * 100f;
			targetRotation = Quaternion.LookRotation(Vector3.down);
			float size = Simulation.robot.distanceToDestination * 0.75f;
			size = Mathf.Max(size, 10f);
			_camera.orthographicSize = size;
			break;
		}
		
		_camera.transform.position = Vector3.Slerp(
			_camera.transform.position, 
			targetPosition, 
			Time.deltaTime * 2f
			);
		
		_camera.transform.rotation = Quaternion.Slerp(
			_camera.transform.rotation, 
			targetRotation, 
			Time.deltaTime * 1f
			);
	}
}
