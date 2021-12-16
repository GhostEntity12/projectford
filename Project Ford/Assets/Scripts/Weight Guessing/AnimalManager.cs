using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnimalManager : MonoBehaviour
{
	[SerializeField] private List<Animal> _possibleAnimals = new List<Animal>();

	[SerializeField] private List<Image> _animalImages = new List<Image>();

	[SerializeField] private TMP_InputField _inputText = null;

	[SerializeField] private Image _indicator = null;

	[SerializeField] private BoxCollider2D _spawnZone = null;

	private List<Animal> _selectedAnimals = new List<Animal>();

	private int _weightSum = 0;

	void Awake()
	{
		// Seed Unity rng with the clock time.
		UnityEngine.Random.InitState(((int)System.DateTime.Now.Ticks));

		Bounds spawnBounds = _spawnZone.bounds;
		Transform spawnZoneTransform = _spawnZone.transform;

		// Select 3 random animals.
		for(int i = 0; i < 3; ++i)
		{
			int randomSelect = UnityEngine.Random.Range(0, 3);
			Animal randomAnimal = _possibleAnimals[randomSelect];

			_selectedAnimals.Add(randomAnimal);
			_animalImages[i].sprite = _selectedAnimals[i].GetSprite();

			Vector2 spawnPointInBounds = RandomPointInBounds(spawnBounds, 1.0f);
			Vector3 vec3SpawnPoint = new Vector3(spawnPointInBounds.x, spawnPointInBounds.y, 0.0f);
			// Spawn the physics object representing the animal in a random point in the spawn zone.
			GameObject spawnedPhysicsObject = GameObject.Instantiate
			(
				randomAnimal.GetPhysicsObject(),
				vec3SpawnPoint,
				Quaternion.Euler(0.0f, 0.0f, UnityEngine.Random.Range(0.0f, 360.0f)),
				spawnZoneTransform
			);
			Debug.Log(spawnedPhysicsObject.name + " pos: " + spawnedPhysicsObject.transform.position);
		}

		// Get the sum of the sides of the different animals.
		foreach(Animal animal in _selectedAnimals)
			_weightSum += animal.GetWeight();
	}

	public void Submit()
	{
		int playerGuess = 0;

		// Parse the input as an integer, if it couldn't be parsed: display an error.
		if (!Int32.TryParse(_inputText.text, out playerGuess))
		{
			Debug.LogError("Input '" + _inputText.text + "' could not be passed as integer!");
			_indicator.color = Color.red;
			return;
		}

		// Input is an integer, check if it was right.
		if (playerGuess == _weightSum)
			_indicator.color = Color.green;
		else
			_indicator.color = Color.red;
	}

	public Vector3 RandomPointInBounds(Bounds bounds, float scale)
	{
		Vector3 randomPoint = new Vector3
		(
			UnityEngine.Random.Range(bounds.min.x * scale, bounds.max.x * scale),
			UnityEngine.Random.Range(bounds.min.y * scale, bounds.max.z * scale),
			0.0f
		);

		return randomPoint;
	}
}