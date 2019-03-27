using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddMoneyOnSecond : MonoBehaviour {
	private float time;
	private float constTime;
	public int moneyAmount = 1;
	public bool bump = true;
	private Vector3 baseScale;

	public bool ignoreYScaling = false, workOnStart = false;

	public float bumperScale = 1;
	public float bumpOffset = 0;

	void Start() {
		baseScale = transform.localScale;
		if(moneyAmount > 0) StartCoroutine(Increment());
	}

	void Update () {
		if(!CityManager.gamePlaying && !workOnStart) return;
		constTime += Time.deltaTime;
		if(bump) {
			float i = Mathf.Sin(constTime * 6 + bumpOffset) / 40 * bumperScale;
			float j = Mathf.Cos(constTime * 6 + bumpOffset) / 50 * bumperScale;
			transform.localScale = new Vector3(baseScale.x + i, (ignoreYScaling)?transform.localScale.y : baseScale.y + j, baseScale.z);
		}
	}

	IEnumerator Increment() {
		while(enabled) {
			CityManager.AddMoney(moneyAmount);
			yield return new WaitForSeconds(1);
		}
	}
}
