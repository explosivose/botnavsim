using UnityEngine;
using System.Collections;

public class Destination : MonoBehaviour {

	public Sprite[] sprites;
	
	private SpriteRenderer _sr;
	
	public void ChooseRandomSprite() {
		_sr.sprite = sprites[Random.Range(0, sprites.Length-1)];
	}
	
	void Awake() {
		_sr = GetComponent<SpriteRenderer>();
	}
	
	void Update() {
		transform.LookAt(CamController.Instance.transform);
	}
}
