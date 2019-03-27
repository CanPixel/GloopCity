using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GasFactory : Building {
	private GameObject particles;

	public bool particlesEnabled = false;

	private bool started = false;

	void Start () {
		particles = transform.GetChild(0).gameObject;
		if(CityManager.gamePlaying) particles.SetActive(particlesEnabled);
		else particles.SetActive(false);
	}

	void FixedUpdate() {
		if(CityManager.gamePlaying && !started) {
			started = true;
			particles.SetActive(particlesEnabled);
		}
	}
}
