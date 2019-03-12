using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CityManager : MonoBehaviour {
	public Text HappinessTXT, MoneyTXT, HealthTXT, EcoTXT, TimeTXT;

	public static CityManager self;

	public GameObject factoryPrefab, windmillPrefab, solarpanelPrefab, nuclearPrefab, tilePrefab;
	public int terrainWidth, terrainHeight;

	public GameObject windmillComponent, solarComponent, nuclearComponent;
	private Button windmillButton, solarButton, nuclearButton;
	private EnergyComponent windmill, solar, nuclear;

	public Color grassCol, stoneCol;

	private int playWidth;

	public static Tile lastTile;

	[Space(5)]
	public int factoryCount = 10;
	public int grassWidth = 3;
	[HideInInspector]
	public float scale = 1;

	public class TileType {
		public string name;
		public Color col;

		public TileType(string name, Color col) {
			this.name = name;
			this.col = col;
		}
	}
	public TileType Stone, Grass, City;

	[Space(15)]
	[Header("Game Values")]
	public int Happiness;
	public int Health;
	public int Economy;
	public int Money;
	public float time;

	[SerializeField]
	public enum EnergySource {
		WINDMILL, NUCLEARPLANT, SOLARPANEL, GASFACTORY
	}
	[System.Serializable]
	public class Energy {
		[HideInInspector]
		public string name = "Building";
		public Vector2Int pos;
		public EnergySource src;
		public int price;

		public Energy(Vector2Int pos, EnergySource src, int price = 100) {
			this.pos = pos;
			this.src = src;
			this.price = price;
		}
	}
	[Space(10)]
	[Header("Starting Buildings")]
	public List<Energy> buildingsOnStart = new List<Energy>();

	public GameObject[,] tiles;

	public static GameObject draggingOBJ = null;

	void Start () {
		self = this;
		Stone = new TileType("Stone", stoneCol);
		Grass = new TileType("Grass", grassCol);
		City = new TileType("City", new Color(0, 0, 0, 0));
		PreloadUI();
		GenerateTerrain();
		foreach(Energy e in buildingsOnStart) GenerateBuilding(e);
	}

	private void PreloadUI() {
		windmillButton = windmillComponent.GetComponent<Button>();
		solarButton = solarComponent.GetComponent<Button>();
		nuclearButton = nuclearComponent.GetComponent<Button>();
		windmill = windmillButton.GetComponent<EnergyComponent>();
		solar = solarButton.GetComponent<EnergyComponent>();
		nuclear = nuclearButton.GetComponent<EnergyComponent>();
	}
	
	void FixedUpdate () {
		//Time
		time += Time.deltaTime;
		if(time > 180) EndGame(); //Time limit?

		UITick();

		if(Input.GetButtonDown("Jump")) NextStep();
	}

	public static bool CheckForSpot(Vector2Int pos) {
		return self.tiles[pos.x, pos.y].GetComponent<Tile>().isAvailable();
	}

	public static void PlaceOBJ(Energy e, int price, int happiness, int economy, int health) {
		self.GenerateBuilding(e);
		self.Money -= price;
		self.Happiness += happiness;
		self.Economy += economy;
		self.Health += health;
		Debug.Log(e.src.ToString() + " placed at "+e.pos+"!");
	}

	private void UITick() {
		HappinessTXT.text = Happiness.ToString();
		MoneyTXT.text = "€ " + Money.ToString();
		EcoTXT.text = Economy.ToString();
		HealthTXT.text = Health.ToString();

		int minutes = (int)time / 60;
		int seconds = (int)time - minutes * 60;
		TimeTXT.text = minutes.ToString() + ":" + ((seconds.ToString().Length < 2)? ("0" + seconds.ToString()) : seconds.ToString());

		windmillButton.interactable = (Money >= windmill.price);
		solarButton.interactable = (Money >= solar.price);
		nuclearButton.interactable = (Money >= nuclear.price);
	}

	public void NextStep() {
		Debug.Log("Next Step!");
	}

	public void EndGame() {
		Debug.Log("Game ended!");
	}

	private void GenerateTerrain() {
		tiles = new GameObject[terrainWidth, terrainHeight];
		for(int x = 0; x < terrainWidth; x++)
			for(int y = 0; y < terrainHeight; y++) {
				TileType type = City;
				playWidth = terrainHeight - (grassWidth + 1);
				if(y <= grassWidth) type = Grass;
				else if(y > grassWidth && y < playWidth) type = Stone;
				tiles[x, y] = GenerateTile(x, y, type);
			}
		factoryCount -= 2;
	}

	private GameObject GenerateTile(int x, int y, TileType type) {
		GameObject go = Instantiate(tilePrefab, new Vector2(0, 0), Quaternion.identity);
		go.transform.SetParent(transform);
		go.name = "Tile (" + x + ", " + y + ")";
		scale = go.transform.localScale.x;
		go.transform.localPosition = new Vector2(x * scale, y * scale) - new Vector2(terrainWidth / 2, terrainHeight / 2);
		go.GetComponent<MeshRenderer>().material.color = type.col;
		go.GetComponent<MeshRenderer>().material.SetColor("Main Color", type.col);
		go.GetComponent<Tile>().pos = new Vector2Int(x, y);
		return go;
	}

	protected void GenerateBuilding(Energy e) {
		GameObject prefab;
		switch(e.src) {
			default:
			case EnergySource.WINDMILL:
				prefab = windmillPrefab;
				break;
			case EnergySource.SOLARPANEL:
				prefab = solarpanelPrefab;
				break;
			case EnergySource.NUCLEARPLANT:
				prefab = nuclearPrefab;
				break;
			case EnergySource.GASFACTORY:
				prefab = factoryPrefab;
				break;
		}
		GameObject go = Instantiate(prefab, new Vector2(0, 0), Quaternion.identity);
		go.transform.SetParent(tiles[e.pos.x, e.pos.y].transform);
		tiles[e.pos.x, e.pos.y].GetComponent<Tile>().Build(go);
		go.transform.localPosition = new Vector3(0, 0, -20);
		go.transform.localRotation = Quaternion.Euler(0, 0, Camera.main.transform.localEulerAngles.z);
		if(go.GetComponent<Building>() != null) go.GetComponent<Building>().price = e.price;
		if(e.src == EnergySource.GASFACTORY) go.GetComponent<GasFactory>().particlesEnabled = true;
	}

	private void GenerateFactory(GameObject parent, float x, float y, int price = 100) {
		GameObject go = Instantiate(factoryPrefab, new Vector2(0, 0), Quaternion.identity);
		go.transform.SetParent(parent.transform);
		go.transform.localPosition = new Vector3(x, y, y) - new Vector3(terrainWidth / 2, terrainHeight / 2, -x);
		go.transform.localRotation = Quaternion.Euler(0, 0, Camera.main.transform.localEulerAngles.z);
		go.transform.localScale = new Vector3(1 - y / 10f, 1 - y / 10f, 1 - y / 10f);
		go.GetComponent<Building>().price = price;
	}

	public static void AddMoney(int amount) {
		self.Money += amount;
	}
}


/*
City Game
Increase the city’s health by replacing the coal factories that produce fossil energy with windmills, solar panels and/or nuclear power plants that produce sustainable energy.
 
Goal of the game:
Get the health stat of the city to +300, while also making sure that the happiness and economy stats stay above +100.
 
Controls:
Touchscreen
Gloop
Start of the game:
At the start of the game you can only see a small part of the map/grid (down right) with one windmill and two factories on it’s left (one small one (one grid space) and one medium one (3 grid spaces)). 
Your revenue comes from that one windmill only. 
You can generate more money by pressing down your gloop as fast as you can (kind of like a cookie clicker).
 
Buying coal factories:
You can buy visible coal factories by clicking on them and playing the indicated amount of money. They all differ in price (between €100-€5000) and size (between 1-5 grid spaces). You can choose to either keep these or destroy them. If you keep the factory you will earn its revenue, but this isn’t beneficial for the city’s health. Destroying the factory will create more space on the grid and clear up a new part of the map.
 
Buying and placing sustainable energy (windmills/solar panels/nuclear power plants):
In the menu at the top of the screen, you can see the 3 types of sustainable energy you can place on empty spaces on the grid. They each take up different amounts of space and have different prices, stats, effects and revenue.
 
Windmills:
            Price: €100
            Health: +50
            Happiness: -20
            Economy: -10
 
Solar Panels:
            Price: €500
            Health: +20
            Happiness: +30
            Economy: +20
 
Nuclear Power Plants:
            Price: €3000
            Health: +10
            Happiness: -30
            Economy: +50
 */