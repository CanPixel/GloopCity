using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour {
	private MeshRenderer render;

	private GameObject outline;
	private List<Material> outlineMats = new List<Material>();

	private float alpha = 0, targetAlpha = 0;
	public Vector2Int pos;

	private GameObject building;

	void Start() {
		render = GetComponent<MeshRenderer>();
		outline = transform.GetChild(0).gameObject;

		foreach(Transform trans in outline.transform) outlineMats.Add(trans.GetComponent<MeshRenderer>().material);
	}

	public void Build(GameObject obj) {
		building = obj;
	}

	void FixedUpdate() {
		alpha = Mathf.Lerp(alpha, targetAlpha, Time.deltaTime * 4);
		foreach(Material mat in outlineMats) mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, alpha);
	}

	void OnMouseOver() {
		CityManager.lastTile = this;
		if(!outline.activeInHierarchy) targetAlpha = 1;
		outline.SetActive(true);
	}

	void OnMouseEnter() {
		if(!isAvailable() && building.GetComponent<Building>() != null) PriceTag.SetPriceTag(Camera.main.WorldToScreenPoint(building.transform.position) + new Vector3(80, 30, 0), building.GetComponent<Building>().price);
	}

	void OnMouseExit() {
		outline.SetActive(false);
		targetAlpha = 0;
		if(!isAvailable()) PriceTag.Hide();
	}

	public bool isAvailable() {
		return building == null;
	}
}
