using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseHover : MonoBehaviour {
	private static Image image;

	void Start() {
		image = GetComponent<Image>();
		image.enabled = false;
	}

	void FixedUpdate () {
		transform.position = Input.mousePosition;	
	}

	public static void SetMouse(GameObject obj) {
		if(obj == null) {
			image.enabled = false;
			return;
		}
		image.enabled = true;
		image.sprite = obj.GetComponent<SpriteRenderer>().sprite;
	}
}
