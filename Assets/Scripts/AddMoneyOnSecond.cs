using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddMoneyOnSecond : MonoBehaviour {
	private float time;
	private float constTime;
	public int moneyAmount = 1;
	public bool bump = true;
	private Vector3 baseScale;

	void Start() {
		baseScale = transform.localScale;
		StartCoroutine(Increment());
	}

	void Update () {
		constTime += Time.deltaTime;
		//time += Time.deltaTime;
		//if(time > 1) {
	//		time = 0;
//			CityManager.AddMoney(moneyAmount);
//		}
		if(bump) {
			float i = Mathf.Sin(constTime * 6) / 40;
			float j = Mathf.Cos(constTime * 6) / 50;
			transform.localScale = new Vector3(baseScale.x + i, baseScale.y + j, baseScale.z);
		}
	}

	IEnumerator Increment() {
		while(enabled) {
			CityManager.AddMoney(moneyAmount);
			yield return new WaitForSeconds(1);
		}
	}
}
