using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CityManager : MonoBehaviour {
	public Text HappinessTXT, MoneyTXT, HealthTXT, EcoTXT, TimeTXT;

	public static bool gamePlaying = false;

	public static CityManager self;

	public GameObject canvas, UI_BG, darkOverlay, trashScreen;
	private RawImage darkness;

	public GameObject factoryPrefab, windmillPrefab, solarpanelPrefab, nuclearPrefab, tilePrefab, smokePrefab, toolTipPrefab;
	public int terrainWidth, terrainHeight;

	public GameObject windmillComponent, solarComponent, nuclearComponent;
	private Button windmillButton, solarButton, nuclearButton;
	private EnergyComponent windmill, solar, nuclear;
	public EnergyComponent coalFactory;

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
	public float timeLimit;
	[Space(5)]
	public int HappinessLimit;
	public int HealthLimit, EconomyLimit;

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
		public bool owned = false;
		public bool locked = true;

		public Energy(Vector2Int pos, EnergySource src, int price = 100) {
			this.pos = pos;
			this.src = src;
			this.price = price;
		}
	}
	[Space(10)]
	[Header("Starting Buildings")]
	public List<Energy> buildingsOnStart = new List<Energy>();

	public enum Property {
		HEALTH, ECONOMY, HAPPINESS
	}

	public GameObject[,] tiles;
	public static GameObject draggingOBJ = null;
	public List<Building> buildings = new List<Building>(), allBuildings = new List<Building>();

	private bool end = false;

	void Start () {
		self = this;
		Stone = new TileType("Stone", stoneCol);
		Grass = new TileType("Grass", grassCol);
		City = new TileType("City", new Color(0, 0, 0, 0));
		darkness = darkOverlay.GetComponent<RawImage>();
		PreloadUI();
		GenerateTerrain();
		foreach(Energy e in buildingsOnStart) GenerateBuilding(e);
	}

	public void ToolTips() {
		SpawnTooltip("Repeatedly press Space to make money!", Color.black, 3, new Vector2(0, 0));
		SpawnTooltip("Press R to reset", Color.gray, 2, new Vector2(0, -50));

		StartCoroutine("Tutorial");
	}

	IEnumerator Tutorial() {
		yield return new WaitForSeconds(4);
		SpawnTooltip("Revive the city by removing polluting sources!", new Color(0.2f, 0.2f, 0.4f), 2, new Vector2(0, 50), 0.6f);
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
		darkness.color = new Color(0, 0, 0, Mathf.Lerp(darkness.color.a, (gamePlaying)? 0 : 0.8f, Time.deltaTime * 2));

		if(gamePlaying) {
			time += Time.deltaTime;
			if(time > timeLimit || GameWon()) EndGame(); //Time limit?

			UITick();

			if(Input.GetButtonDown("Jump")) NextStep();
		} else nuclearButton.interactable = windmillButton.interactable = solarButton.interactable = false;

		if(Input.GetKeyUp(KeyCode.R)) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public static bool CheckForSpot(Vector2Int pos) {
		return self.tiles[pos.x, pos.y].GetComponent<Tile>().isAvailable() & self.tiles[pos.x, pos.y].GetComponent<Tile>().isUnlocked();
	}

	public bool GameWon() {
		return Health >= HealthLimit;
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
		Health = Mathf.Clamp(Health, -HealthLimit, HealthLimit);

		HappinessTXT.text = Happiness.ToString();
		MoneyTXT.text = "$ " + Money.ToString();
		EcoTXT.text = Economy.ToString();
		HealthTXT.text = Health.ToString();

		HappinessTXT.color = (Happiness > 0)? PriceTag.self.green : PriceTag.self.red;
		EcoTXT.color = (Economy > 0)? PriceTag.self.green : PriceTag.self.red;
		HealthTXT.color = (Health > 0)? PriceTag.self.green : PriceTag.self.red;

		int minutes = (int)time / 60;
		int seconds = (int)time - minutes * 60;
		TimeTXT.text = minutes.ToString() + ":" + ((seconds.ToString().Length < 2)? ("0" + seconds.ToString()) : seconds.ToString());

		windmillButton.interactable = (Money >= windmill.price);
		solarButton.interactable = (Money >= solar.price);
		nuclearButton.interactable = (Money >= nuclear.price);
	}

	public void NextStep() {
		int count = 0;
		foreach(Building b in buildings) {
			if(!b) continue;
			count++;
			int amount = b.gameObject.GetComponent<AddMoneyOnSecond>().moneyAmount;
			AddMoney(amount);
			//UI Feedback
			Vector2 scrPoint = Camera.main.WorldToScreenPoint(b.transform.position);
			SpawnTooltip("+ $" + amount.ToString(), new Color(0, 1, 0), 0.5f, new Vector2(scrPoint.x, scrPoint.y + 100), 0.5f, true);
		}
		if(count <= 0) SpawnTooltip("No source of income!", Color.red, 1, new Vector2(0, 0));
	}

	public void SpawnTooltip(string text, Color col, float life, Vector2 pos, float scale = 0.5f, bool worldSpace = false) {
		GameObject tip = Instantiate(toolTipPrefab, new Vector2(0, 0), Quaternion.identity);
		tip.transform.SetParent(canvas.transform);
		tip.transform.localPosition = pos;
		if(worldSpace) {
			Vector3 fin = pos;
			if(pos.x > 380) fin = new Vector2(fin.x - 50, fin.y);
			tip.transform.localPosition = new Vector2(0, 0);
			tip.transform.position = fin;
		}
		tip.transform.localScale = new Vector2(scale, scale);
		tip.GetComponent<Tooltip>().text = text;
		tip.GetComponent<Tooltip>().lifeSpan = life;
		tip.GetComponent<Tooltip>().color = col;
	}

	private Vector2 toolTipPos = new Vector2(-400, 370);

	public void Burn() {
		trashScreen.SetActive(false);
		Health -= 100;
		StartGame();

		SpawnTooltip("Because of changes in another game,", new Color(1f, 0.4f, 0.4f), 180, new Vector2(toolTipPos.x, toolTipPos.y + 25), 0.3f);
		SpawnTooltip("you start with lower health.", Color.red, 180, toolTipPos, 0.3f);
	}
	public void Recycle() {
		trashScreen.SetActive(false);
		Economy -= 50;
		foreach(Building b in allBuildings) {
			if(!b) continue;
			b.price += 100;
		}
		StartGame();
		SpawnTooltip("Because of changes in another game,", new Color(1f, 0.4f, 0.4f), 180, new Vector2(toolTipPos.x, toolTipPos.y + 25), 0.3f);
		SpawnTooltip("Everything is more expensive.", Color.red, 180, toolTipPos, 0.3f);
	}
	public void Bury() {
		trashScreen.SetActive(false);
		int amountOfLockedTiles = 5;
		for(int i = 0; i < amountOfLockedTiles; i++) {
			int x = Random.Range(0, tiles.GetLength(0));
			int y = Random.Range(0, tiles.GetLength(1));
			if(!tiles[x, y]) {
				i--; 
				continue;
			}
			Destroy(tiles[x, y].gameObject);
		}
		StartGame();
		SpawnTooltip("Because of changes in another game,", new Color(1f, 0.4f, 0.4f), 180, new Vector2(toolTipPos.x, toolTipPos.y + 25), 0.3f);
		SpawnTooltip("You have less usable land.", Color.red, 180, toolTipPos, 0.3f);
	}

	public void StartGame() {
		gamePlaying = true;
		nuclearButton.interactable = windmillButton.interactable = solarButton.interactable = true;
		ToolTips();
	}

	public void EndGame() {
		if(!end) Debug.Log("Game ended!");
		end = true;
		gamePlaying = false;
		SpawnTooltip("Game ended!", Color.white, 360, new Vector2(0, 0));
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
		if(y < 1 || y > 8 || x > 10) return null;
		GameObject go = Instantiate(tilePrefab, new Vector2(0, 0), Quaternion.identity);
		go.transform.SetParent(transform);
		go.name = "Tile (" + x + ", " + y + ")";
		scale = go.transform.localScale.x;
		go.transform.localPosition = new Vector2(x * scale, y * scale) - new Vector2(terrainWidth / 2, terrainHeight / 2);
		go.GetComponent<MeshRenderer>().material.color = type.col;
		go.GetComponent<MeshRenderer>().material.SetColor("Main Color", type.col);
		go.GetComponent<Tile>().pos = new Vector2Int(x, y);
		go.GetComponent<Tile>().InitSmoke();
		float road = 8;
		float grass = 4;
		if(y == road || y < grass) {
			if(y < grass || y == road) go.GetComponent<Tile>().Unlock();
			if(y == road) Destroy(go.GetComponent<Tile>());
		}
		return go;
	}

	protected void GenerateBuilding(Energy e) {
		GameObject prefab;
		EnergyComponent energyComponent;
		switch(e.src) {
			default:
			case EnergySource.WINDMILL:
				energyComponent = windmill;
				prefab = windmillPrefab;
				break;
			case EnergySource.SOLARPANEL:
				energyComponent = solar;
				prefab = solarpanelPrefab;
				break;
			case EnergySource.NUCLEARPLANT:
				energyComponent = nuclear;
				prefab = nuclearPrefab;
				break;
			case EnergySource.GASFACTORY:
				energyComponent = coalFactory;
				prefab = factoryPrefab;
				break;
		}
		GameObject go = Instantiate(prefab, new Vector2(0, 0), Quaternion.identity);
		go.transform.SetParent(tiles[e.pos.x, e.pos.y].transform);
		tiles[e.pos.x, e.pos.y].GetComponent<Tile>().Build(go, energyComponent);
		go.transform.localPosition = new Vector3(0, 0, -20 + e.pos.y * 2);
		go.transform.localRotation = Quaternion.Euler(0, 0, Camera.main.transform.localEulerAngles.z);
		if(go.GetComponent<Building>() != null) {
			go.GetComponent<Building>().price = e.price;
			go.GetComponent<Building>().owned = e.owned;
			go.GetComponent<Building>().locked = e.locked;
			if(e.owned) buildings.Add(go.GetComponent<Building>());
			allBuildings.Add(go.GetComponent<Building>());
		}
		if(!e.locked) tiles[e.pos.x, e.pos.y].GetComponent<Tile>().Unlock();
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

	public static void ClearSmoke(Tile tile) {
		self.Clean(tile);
	}

	private void Clean(Tile tile) {
		if(!tile) return;
		Vector2Int basePos = tile.pos;
		int radius = 3;
		for(int x = -(radius - 1); x < radius; x++)
			for(int y = -(radius - 1); y < radius; y++) {
				try {
					if(tiles[basePos.x + x, basePos.y + y] == null) continue;
					Tile tilesCasasPescador = tiles[basePos.x + x, basePos.y + y].GetComponent<Tile>();
					if(tilesCasasPescador == null) continue;
					tilesCasasPescador.Unlock();
					if(tilesCasasPescador.isAvailable()) tilesCasasPescador.Unlock();
				} catch(System.IndexOutOfRangeException){continue;}
			}
	}

	public static Vector3 GetOffset() {
		return new Vector2(self.terrainWidth / 2, self.terrainHeight / 2);
	}

	public static void ClaimBuilding(Building build) {
		self.buildings.Add(build);
		AddMoney(-build.price);
	}

	public static void refreshList() {
		self.recalculate();
	}

	private void recalculate() {
		buildings.Clear();
		GameObject[] bds = GameObject.FindGameObjectsWithTag("Building");
		foreach(GameObject go in bds)
			if(go.GetComponent<Building>() != null) { 
				Building bd = go.GetComponent<Building>();
				if(bd.owned) buildings.Add(bd);			
			}
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