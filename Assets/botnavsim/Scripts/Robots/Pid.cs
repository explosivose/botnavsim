using UnityEngine;
using System.Collections;

[System.Serializable]
public class Pid {
	public float kp, ki, kd;
	public float error {get; private set;}
	public float previous_error {get; private set;}
	public float integral {get; private set;}
	public float derivative {get; private set;}
	
	public float output(float target, float actual) {
		error = target - actual;
		integral += error * Time.fixedDeltaTime;
		derivative = (error - previous_error) / Time.fixedDeltaTime;
		previous_error = error;
		return (kp*error) + (ki*integral) + (kd*derivative);
	}
	
	public void CopySettings(Pid pid) {
		kp = pid.kp;
		ki = pid.ki;
		kd = pid.kd;
	}
}
