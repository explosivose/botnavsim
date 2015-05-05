using UnityEngine;
using System.Collections;

/// <summary>
/// Sets a random sprite for displaying the robot destination.
/// </summary>
public class Destination : MonoBehaviour {

	/// <summary>
	/// Sprite library (values set in Unity Inspector)
	/// </summary>
	public Sprite[] sprites;
	
	/// <summary>
	/// Reference to the sprite renderer component.
	/// </summary>
	private SpriteRenderer _sr;
	
	/// <summary>
	/// Chooses the random sprite.
	/// </summary>
	public void ChooseRandomSprite() {
		_sr.sprite = sprites[Random.Range(0, sprites.Length-1)];
	}
	
	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake() {
		_sr = GetComponent<SpriteRenderer>();
	}
	
	/// <summary>
	/// Set the sprite to face the camera.
	/// </summary>
	void Update() {
		transform.LookAt(CamController.Instance.transform);
	}
}
