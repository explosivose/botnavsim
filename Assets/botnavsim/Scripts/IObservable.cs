using UnityEngine;
using System.Collections;

// objects that are observable via CamController must keep their world bounds up-to-date
public interface IObservable {
	string name {get;}
	Bounds bounds {get;}
}
