using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour {
	public string text;
	public Color color;
	public float lifeSpan = 3;
	private float time = 0;

	private Text shadow;
	private Text core;

	void Start () {
		core = GetComponent<Text>();
		shadow = transform.GetChild(0).GetComponent<Text>();
	}
	
	void FixedUpdate () {
		time += Time.deltaTime;
		core.text = text;
		shadow.text = core.text;
		shadow.color = core.color * 0.8f;

		if(time > lifeSpan) {
			float a = Mathf.Lerp(core.color.a, 0, Time.deltaTime * 4);
			core.color = new Color(core.color.r, core.color.g, core.color.b, a);
			if(time > lifeSpan + 2f) Destroy(gameObject);
		} else core.color = color;
	}
}
