using UnityEngine;
using System.Collections;

public class CameraType : MonoBehaviour {

	public LayerMask maskNormal;
	public LayerMask maskBotData;
	public LayerMask maskHybrid;
	
	public enum Type {
		Normal,
		BotData,
		Hybrid
	}
	
	public Type type {
		get {
			return _type;
		}
		set {
			_type = value;
			switch(_type) {
			case Type.Normal:
			default:
				_camera.cullingMask = maskNormal;
				break;
			case Type.BotData:
				_camera.cullingMask = maskBotData;
				break;
			case Type.Hybrid:
				_camera.cullingMask = maskHybrid;
				break;
			}
		}
	}
	
	private Type _type;
	private Camera _camera;
	
	public void CycleType() {
		type++;
		if (type > Type.Hybrid) type = Type.Normal;
	}
	
	void Awake() {
		_camera = GetComponent<Camera>();
	}
	
	void Update() {
		if (type == Type.BotData || type == Type.Hybrid) {
			Simulation.botscript.navigation.DrawDebugInfo();
		}
	}
	
}
