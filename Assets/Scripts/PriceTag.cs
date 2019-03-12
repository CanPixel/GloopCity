using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PriceTag : MonoBehaviour {
	public static PriceTag self;

	private Text text;
	private int num;
	private Text child;
	private Vector3 baseLoc;

	void Start() {
		self = this;
		text = GetComponent<Text>();
		child = transform.GetChild(0).GetComponent<Text>();
		Hide();
	}

	void FixedUpdate () {
		text.text = child.text = "€" + num;

		transform.position = new Vector3(baseLoc.x, baseLoc.y + Mathf.Sin(Time.time * 3), baseLoc.z);
	}

	public static void SetPriceTag(Vector3 pos, int price) {
		self.transform.position = self.baseLoc = pos;
		self.num = price;
		self.gameObject.SetActive(true);
	}

	public static void Hide() {
		self.gameObject.SetActive(false);
	}
}
