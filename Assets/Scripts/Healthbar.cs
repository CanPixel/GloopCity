using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healthbar : MonoBehaviour {
	private GameObject bar;
	private float health;
	private SpriteRenderer pixels;

	void Start () {
		bar = transform.GetChild(0).gameObject;		
		pixels = bar.GetComponent<SpriteRenderer>();
	}
	
	void FixedUpdate () {
		health = CityManager.self.Health;
		float percentage = (CityManager.self.Health + CityManager.self.HealthLimit) * (100f / (CityManager.self.HealthLimit*2));

		float r = Mathf.Lerp(Color.red.r, Color.green.r, percentage / 100f);
		float g = Mathf.Lerp(Color.red.g, Color.green.g, percentage / 100f);
		float b = Mathf.Lerp(Color.red.b, Color.green.b, percentage / 100f);
		pixels.color = new Color(r, g, b);
		bar.transform.localScale = new Vector2(bar.transform.localScale.x, Mathf.Clamp(percentage / 100f, 0, 1f));
	}
}
