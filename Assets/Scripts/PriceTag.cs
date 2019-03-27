using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PriceTag : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
	public static PriceTag self;

	public Text price, priceShadow, title, titleShadow, happiness, happinessShadow, health, healthShadow, eco, ecoShadow;
	private int num;
	private Vector3 baseLoc;

	public Image priceTag;
	private Button priceButton;
	private Building current;

	public Color green, red, gray;
	public static bool hover = true;
	private bool owned = false;

	void Awake() {
		self = this;
		priceButton = priceTag.GetComponent<Button>();
		Hide();
	}

	void FixedUpdate () {
		transform.position = new Vector3(baseLoc.x, baseLoc.y + Mathf.Sin(Time.time * 3), baseLoc.z);
	
		if(!owned) {
			price.text = priceShadow.text = "€" + num;
			if(CityManager.self.Money < num) {
				priceTag.color = red; 
				priceButton.interactable = false;
			}
			else {
				priceTag.color = green; 
				priceButton.interactable = true;
			}
		} else {
			priceTag.color = gray;
			priceButton.interactable = true;
			price.text = priceShadow.text = "Kill";
		}
	}

	public static void SetPriceTag(Vector3 pos, Building building, EnergyComponent e) {
		if(!CityManager.gamePlaying) return;
		Vector3 UIpos = pos;
		if(Input.mousePosition.x > 790) UIpos.x -= 100;
		if(Input.mousePosition.y < 100) UIpos.y += 50;
		self.transform.position = self.baseLoc = UIpos;
		self.num = building.price;
		self.title.text = building.buildingName;
		self.titleShadow.text = building.buildingName;
		self.gameObject.SetActive(true);
		self.current = building;
		self.owned = building.owned;
		SetProperty(CityManager.Property.HEALTH, e.Health);
		SetProperty(CityManager.Property.HAPPINESS, e.Happiness);
		SetProperty(CityManager.Property.ECONOMY, e.Economy);
	}

	public static void SetProperty(CityManager.Property prop, int value) {
		bool plus = false;
		if(value > 0) plus = true;
		string val = ((plus)? "+" : "") + value;
		Color valCol = (plus)? self.green : self.red;
		float darker = 0.1f;
		switch(prop) {
			default:
			case CityManager.Property.HEALTH:
				self.health.text = self.healthShadow.text = val;
				self.health.color = valCol;
				self.healthShadow.color = new Color(valCol.r - darker, valCol.g - darker, valCol.b - darker);
				break;
			case CityManager.Property.ECONOMY:
				self.eco.text = self.ecoShadow.text = val;
				self.eco.color = valCol;
				self.ecoShadow.color = new Color(valCol.r - darker, valCol.g - darker, valCol.b - darker);
				break;
			case CityManager.Property.HAPPINESS:
				self.happiness.text = self.happinessShadow.text = val;
				self.happiness.color = valCol;
				self.happinessShadow.color = new Color(valCol.r - darker, valCol.g - darker, valCol.b - darker);
				break;
		}
	}

	public void Buy() {
		if(!owned) {
			current.owned = owned = true;
			current.gameObject.GetComponent<AddMoneyOnSecond>().enabled = true;
			CityManager.ClaimBuilding(current);
		} else ClearBuilding();
	}

	public void ClearBuilding() {
		bool gas = current.isGassy;
		Destroy(current.gameObject);
		Hide();
		hover = false;
		if(gas) CityManager.ClearSmoke(CityManager.lastTile);
		CityManager.lastTile = null;
		CityManager.refreshList();
	}

	public static void Hide() {
		self.gameObject.SetActive(false);
	}

	public void OnPointerEnter(PointerEventData data) {
		hover = true;
	}

	public void OnPointerExit(PointerEventData data) {
		hover = false;
		CityManager.lastTile = null;
	}
}
