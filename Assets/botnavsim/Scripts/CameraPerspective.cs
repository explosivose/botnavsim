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
				_camera.camera.orthographic = false;
				_camera.camera.fieldOfView = 120f;
				_camera.parent = Simulation.robot.transform;
				break;
			case Perspective.ThirdPerson:
				_camera.camera.orthographic = false;
				_camera.camera.fieldOfView = 60f;
				_camera.parent = Simulation.robot.transform;
				break;
			default:
			case Perspective.Birdseye:
				_camera.camera.orthographic = true;
				_camera.parent = null;
				
				break;
			}
		}
	}
	
	private Perspective _p;
	private Transform _camera;
	
	public void CyclePerspective() {
		perspective++;
		if (perspective > Perspective.Birdseye) perspective = Perspective.FirstPerson;
	}
	
	void Start() {
		_camera = Camera.main.transform;
	}
	
	void Update() {
		Vector3 targetPosition;
		Quaternion targetRotation;
		Transform bot = Simulation.robot.transform;
		switch (_p) {
		case Perspective.FirstPerson:
			targetPosition = bot.position + Vector3.up * 1.5f;
			targetRotation = Quaternion.LookRotation(bot.rigidbody.velocity);
			break;
		case Perspective.ThirdPerson:
			_camera.camera.orthographic = false;
			targetPosition = bot.position;
			targetPosition -= bot.rigidbody.velocity.normalized * 5f;
			targetPosition += bot.up * 5f;
			targetRotation = Quaternion.LookRotation(bot.position - targetPosition);
			break;
		default:
		case Perspective.Birdseye:
			targetPosition = (bot.position + Simulation.destination.transform.position)/2f;
			targetPosition += Vector3.up * 10f;
			targetRotation = Quaternion.LookRotation(Vector3.down);
			_camera.camera.orthographicSize = Simulation.botscript.distanceToDestination;
			break;
		}
		
		_camera.position = Vector3.Lerp(_camera.position, targetPosition, Time.deltaTime);
		_camera.rotation = Quaternion.Lerp(_camera.rotation, targetRotation, Time.deltaTime);
	}
}
