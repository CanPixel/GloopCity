using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceScreen : MonoBehaviour {
	[System.Serializable]
	public class Choice {
		public string question;
		public string A, B, C;

		public Influence resultA, resultB, resultC;
	}
	[System.Serializable]
	public class Influence {
		public int eco, happy, health, money; 
	}

	public float interval = 30;
	[Space(10)]
	public Text title;
	public Text choiceA, choiceB, choiceC;
	
	public Text aEconomy, aHappiness, aHealth, bEconomy, bHappiness, bHealth, cEconomy, cHappiness, cHealth;
	public Image aEcoIMG, aHappIMG, aHealthIMG, bEcoIMG, bHappIMG, bHealthIMG, cEcoIMG, cHappIMG, cHealthIMG;

	private Text titleShadow, choiceAShadow, choiceBShadow, choiceCShadow;

	public Choice[] questions;

	private List<Choice> availableChoices = new List<Choice>();
	private float time = 0;
	private GameObject screen;
	private Choice currentChoice;

	void Start () {
		foreach(Choice c in questions) availableChoices.Add(c);
		screen = transform.GetChild(0).gameObject;
		titleShadow = title.transform.GetChild(0).GetComponent<Text>();
		choiceAShadow = choiceA.transform.GetChild(0).GetComponent<Text>();
		choiceBShadow = choiceB.transform.GetChild(0).GetComponent<Text>();
		choiceCShadow = choiceC.transform.GetChild(0).GetComponent<Text>();
		screen.SetActive(false);
	}
	
	void FixedUpdate () {
		if(CityManager.gamePlaying) {
			time += Time.deltaTime;
			if(time > interval) {
				time = 0;
				QueueQuestion();
			}
		}
		titleShadow.text = title.text;
		choiceAShadow.text = choiceA.text;
		choiceBShadow.text = choiceB.text;
		choiceCShadow.text = choiceC.text;
	}

	public void QueueQuestion() {
		EnableScreen(true);
		currentChoice = availableChoices[Random.Range(0, availableChoices.Count)];
		title.text = currentChoice.question;
		choiceA.text = currentChoice.A;
		choiceB.text = currentChoice.B;
		choiceC.text = currentChoice.C;
		
		aEcoIMG.gameObject.SetActive(currentChoice.resultA.eco == 0);
		aHappIMG.gameObject.SetActive(currentChoice.resultA.happy == 0);
		aHealthIMG.gameObject.SetActive(currentChoice.resultA.health == 0);

		bEcoIMG.gameObject.SetActive(currentChoice.resultB.eco == 0);
		bHappIMG.gameObject.SetActive(currentChoice.resultB.happy == 0);
		bHealthIMG.gameObject.SetActive(currentChoice.resultB.health == 0);

		cEcoIMG.gameObject.SetActive(currentChoice.resultC.eco == 0);
		cHappIMG.gameObject.SetActive(currentChoice.resultC.happy == 0);
		cHealthIMG.gameObject.SetActive(currentChoice.resultC.health == 0);
	}

	public void EnableScreen(bool i) {
		screen.SetActive(i);
		CityManager.gamePlaying = !i;
	}

	public void ApplyA() {
		EnableScreen(false);
		CityManager.self.Economy += currentChoice.resultA.eco;
		CityManager.self.Health += currentChoice.resultA.health;
		CityManager.self.Happiness += currentChoice.resultA.happy;
		CityManager.self.Money += currentChoice.resultA.money;
	}

	public void ApplyB() {
		EnableScreen(false);
		CityManager.self.Economy += currentChoice.resultB.eco;
		CityManager.self.Health += currentChoice.resultB.health;
		CityManager.self.Happiness += currentChoice.resultB.happy;
		CityManager.self.Money += currentChoice.resultB.money;
	}

	public void ApplyC() {
		EnableScreen(false);
		CityManager.self.Economy += currentChoice.resultC.eco;
		CityManager.self.Health += currentChoice.resultC.health;
		CityManager.self.Happiness += currentChoice.resultC.happy;
		CityManager.self.Money += currentChoice.resultC.money;
	}
}
