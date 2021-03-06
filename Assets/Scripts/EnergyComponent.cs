﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EnergyComponent : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
	public CityManager.EnergySource energySource;

	public GameObject targetOBJ;

	public int price, Happiness,  Economy, Health;

	private Button button;

	void Start() {
		button = GetComponent<Button>();
	}

	public void DragComponent() {
		if(!CityManager.gamePlaying) return;
		CityManager.draggingOBJ = targetOBJ;
		MouseHover.SetMouse(targetOBJ);
	}

	private void Place() {
		if(!CityManager.lastTile) return;
		CityManager.Energy e = new CityManager.Energy(CityManager.lastTile.pos, energySource);
		e.owned = true;
		if(CityManager.CheckForSpot(e.pos)) CityManager.PlaceOBJ(e, price, Happiness, Economy, Health);
		else Debug.Log("Couldn't place!");
	}

	public void OnBeginDrag(PointerEventData pointer) {
		if(!CityManager.gamePlaying) return;
		if(button.interactable) DragComponent();
	}

	public void OnDrag(PointerEventData data) {}

	public void OnEndDrag(PointerEventData data) {
		if(!CityManager.gamePlaying) return;
		if(!CityManager.draggingOBJ) return;
		Place();
		MouseHover.SetMouse(null);
		CityManager.draggingOBJ = null;
	}
}
