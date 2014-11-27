using UnityEngine;
using System.Collections;

public interface INavigation {
	// search initialisation
	void SetSearchSpace(Bounds searchSpace);
	void SetDestination(Vector3 destination);
	Vector3 GetDestination();
	void SetBotPosition(Vector3 position);
	Vector3 GetBotPosition();
	// bot communication
	Vector3 MoveDirection(Vector3 currentPosition);
	void DepthData(Vector3 start, Vector3 end, bool obstructed);
	bool SearchComplete();
	// debug methods
	void DrawGizmos();
	
	
	
	
	// void Obstruction(vector3 location);
	// void Unobstructed(vector3 location);
	// void CheatDetectObstructions();
	// void ForgetObstructions();
	
}