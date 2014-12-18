using UnityEngine;
using System.Collections;

public class SpriteFade : MonoBehaviour {

	public float fadeRate;
	
	SpriteRenderer sr;

	// Use this for initialization
	void Start () {
		sr = GetComponent<SpriteRenderer>();
	}
	
	void Update() {
		if (!sr.enabled) return;
		Color color = sr.color;
		color.a -= fadeRate * Time.deltaTime;
		sr.color = color;
		if (color.a <= 0f) sr.enabled = false;
		
	}
}
