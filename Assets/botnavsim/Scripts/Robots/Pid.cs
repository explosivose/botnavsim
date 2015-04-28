using UnityEngine;
using System.Collections;

[System.Serializable]
/// <summary>
/// Pid encapsulates PID control for floating point variables.
/// </summary>
public class Pid {
	public float kp, ki, kd;
	public float error {get; private set;}
	public float previous_error {get; private set;}
	public float integral {get; private set;}
	public float derivative {get; private set;}
	
	/// <summary>
	/// Pid control from actual to target. Call in Update (not FixedUpdate), uses Time.deltaTime
	/// </summary>
	/// <param name="target">Target.</param>
	/// <param name="actual">Actual.</param>
	public float output(float target, float actual) {
		error = target - actual;
		integral += error * Time.fixedDeltaTime;
		derivative = (error - previous_error) / Time.fixedDeltaTime;
		previous_error = error;
		return (kp*error) + (ki*integral) + (kd*derivative);
	}
	
	public void Reset() {
		error = 0f;
		previous_error = 0f;
		integral = 0f;
		derivative = 0f;
	}
	
	/// <summary>
	/// Copies the settings from param to this instance
	/// </summary>
	/// <param name="pid">Pid.</param>
	public void CopySettings(Pid pid) {
		kp = pid.kp;
		ki = pid.ki;
		kd = pid.kd;
	}
}

[System.Serializable]
/// <summary>
/// PidVector3 encapsulates PID control for 3D vectors.
/// </summary>
public class PidVector3 {

	public float kp, ki, kd;
	public Vector3 error {get; private set;}
	public Vector3 previous_error {get; private set;}
	public Vector3 integral {get; private set;}
	public Vector3 derivative {get; private set;}
	
	/// <summary>
	/// Pid control from actual to target. Call in Update (not FixedUpdate), uses Time.deltaTime
	/// </summary>
	/// <param name="target">Target.</param>
	/// <param name="actual">Actual.</param>
	public Vector3 output(Vector3 target, Vector3 actual) {
		error = target - actual;
		integral += error * Time.deltaTime;
		derivative = (error - previous_error) / Time.deltaTime;
		previous_error = error;
		return (kp*error) + (ki*integral) + (kd*derivative);
	}
	
	/// <summary>
	/// Copies the settings from param to this instance
	/// </summary>
	/// <param name="pid">Pid.</param>
	public void CopySettings(Pid pid) {
		kp = pid.kp;
		ki = pid.ki;
		kd = pid.kd;
	}
}