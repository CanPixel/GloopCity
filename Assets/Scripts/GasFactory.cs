using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GasFactory : Building {
	private GameObject particles;

	public bool particlesEnabled = false;

	void Start () {
		particles = transform.GetChild(0).gameObject;
		particles.SetActive(particlesEnabled);
	}
}
