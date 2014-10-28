using UnityEngine;
using System.Collections;

public interface INavigation {
	void SetDestination(Vector3 destination);
	Vector3 GetDestination();
	Vector3 MoveDirection(Vector3 currentPosition);
	void DepthData(Vector3 start, Vector3 end, bool obstructed);
}