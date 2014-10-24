using UnityEngine;
using System.Collections;

public interface INavigation {
	Vector3 MoveDirection(Vector3 currentPosition);
	void DepthData(Vector3 start, Vector3 end, bool obstructed);
}