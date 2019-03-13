using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour {
	private GameObject outline;
	private List<Material> outlineMats = new List<Material>();

	private float alpha = 0, targetAlpha = 0;
	public Vector2Int pos;

	private GameObject building;
	private EnergyComponent energy;

	public bool locked = true;

	private GameObject smoke;

	void Start() {
		outline = transform.GetChild(0).gameObject;
		foreach(Transform trans in outline.transform) outlineMats.Add(trans.GetComponent<MeshRenderer>().material);
	}

	public void Unlock() {
		Destroy(smoke);
		locked = false;
	}

	public bool isUnlocked() {
		return !locked;
	}

	public void InitSmoke() {
		if(locked) GenSmoke();
	}

	private void GenSmoke() {
		smoke = Instantiate(CityManager.self.smokePrefab, new Vector2(0, 0), Quaternion.identity);
		smoke.transform.SetParent(gameObject.transform);
		smoke.transform.localPosition = new Vector3(0, 0, -21);
		smoke.transform.localRotation = Quaternion.Euler(0, 0, Camera.main.transform.localEulerAngles.z);
	}

	public void Build(GameObject obj, EnergyComponent e) {
		building = obj;
		energy = e;
	}

	void FixedUpdate() {
		alpha = Mathf.Lerp(alpha, targetAlpha, Time.deltaTime * 4);
		foreach(Material mat in outlineMats) mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, alpha);
	
		if(!CityManager.lastTile) DisableOutline();
	}

	void OnMouseOver() {
		if(PriceTag.hover) return;
		CityManager.lastTile = this;
		if(!outline.activeInHierarchy) targetAlpha = 1;
		outline.SetActive(true);
	}

	void OnMouseEnter() {
		if(!isAvailable() && building.GetComponent<Building>() != null) {
			Building comp = building.GetComponent<Building>();
			PriceTag.SetPriceTag(Camera.main.WorldToScreenPoint(building.transform.position) + new Vector3(80, 30, 0), comp, energy);
		}
	}

	void OnMouseExit() {
		if(PriceTag.hover) return;
		DisableOutline();
	}

	public void DisableOutline() {
		outline.SetActive(false);
		targetAlpha = 0;
		if(!PriceTag.hover) PriceTag.Hide();
	}

	public bool isAvailable() {
		return building == null;
	}
}
