using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnimalManager : MonoBehaviour
{
	/// <summary>
	/// Possible animals the manager can select.
	/// </summary>
	[SerializeField] private List<Animal> _allPossibleAnimals = new List<Animal>();

	/// <summary>
	/// The images that will show the selected animals.
	/// </summary>
	[SerializeField] private List<Image> _animalImages = new List<Image>();

	/// <summary>
	/// TMPro input field.
	/// </summary>
	[SerializeField] private TMP_InputField _inputText = null;

	/// <summary>
	/// Just an image that changes colour to show if the input was right or wrong (just for prototyping).
	/// </summary>
	/// <returns></returns>
	[SerializeField] private Image _indicator = null;

	/// <summary>
	/// The zone for spawning the physics objects.
	/// </summary>
	/// <returns></returns>
	[SerializeField] private BoxCollider2D _spawnZone = null;

	/// <summary>
	/// The Transform which holds the displayed weights of the animals
	/// </summary>
	[SerializeField] private Transform _weightList = null;

	/// <summary>
	/// The prefab for displaying animal weight information
	/// </summary>
	[SerializeField] private GameObject _weightPrefab = null;

	/// <summary>
	/// How many animals to choose for the variety of animals that can appear to begin with.
	/// </summary>
	[SerializeField] private int _startingAnimalVarietyCount = 3;

	/// <summary>
	/// The animals the manager can choose to spawn in the game.
	/// </summary>
	private List<Animal> _currentAnimalVariety = new List<Animal>();

	/// <summary>
	/// The bounds of the spawn zone.
	/// <para>Gets properly set in Awake(), just needs something for now.</para>
	/// </summary>
	private Bounds _spawnBounds = new Bounds();

	/// <summary>
	/// The transform of the spawn zone.
	/// </summary>
	private Transform _spawnZoneTransform = null;

	/// <summary>
	/// The animals that were selected for the game.
	/// </summary>
	private List<Animal> _selectedAnimals = new List<Animal>();

	/// <summary>
	/// The physics objects of the selected animals in the scene.
	/// </summary>
	private List<GameObject> _selectedAnimalsObjs = new List<GameObject>();

	/// <summary>
	/// The sum of the selected animal's weights.
	/// </summary>
	private int _weightSum = 0;

	/// <summary>
	/// The combo manager.
	/// </summary>
	private ComboManager _comboManager = null;
	
	/// <summary>
	/// The instance of the animal manager.
	/// </summary>
	static private AnimalManager _instance = null;

	/// <summary>
	/// On startup.
	/// </summary>
	void Awake()
	{
		_instance = this;

		// Get necessary variables.
		_spawnBounds = _spawnZone.bounds;
		_spawnZoneTransform = _spawnZone.transform;

		// Seed Unity RNG with the clock time.
		UnityEngine.Random.InitState((int)DateTime.Now.Ticks);

		// Go through all the possible animals to choose and get a number of animals from all the possible animals to choose.
		for(int i = 0; i < _startingAnimalVarietyCount; ++i)
		{
			IncreaseAnimalVariety();
		}

		// Select animals to begin with.
		SelectAnimals();

		// Spawn the weight text
		_allPossibleAnimals.ForEach(animal => Instantiate(_weightPrefab, _weightList).GetComponent<AnimalWeightInfo>().SetValues(animal));
	}

	void Start()
	{
		_comboManager = ComboManager.GetInstance();
	}

	/// <summary>
	/// Selects and sets animals for the weight guessing game.
	/// <para>OVERRIDES THE CURRENT ROUND!</para>
	/// </summary>
	public void SelectAnimals()
	{
		_weightSum = 0;

		// Select 3 random animals.
		for (int i = 0; i < 3; ++i)
		{
			Animal randomAnimal = _currentAnimalVariety[UnityEngine.Random.Range(0, _currentAnimalVariety.Count)];

			_selectedAnimals.Add(randomAnimal);
			_animalImages[i].sprite = _selectedAnimals[i].Sprite;

			Vector2 spawnPointInBounds = RandomPointInBounds(_spawnBounds, 1.0f);
			Vector3 vec3SpawnPoint = new Vector3(spawnPointInBounds.x, spawnPointInBounds.y, 0.0f);

			// Spawn the physics object representing the animal in a random point in the spawn zone.
			GameObject spawnedPhysicsObject = Instantiate
			(
				randomAnimal.PhysicsObject,
				vec3SpawnPoint,
				Quaternion.Euler(0.0f, 0.0f, UnityEngine.Random.Range(0.0f, 360.0f)),
				_spawnZoneTransform
			);

			// Save the objects to a list for reference later.
			_selectedAnimalsObjs.Add(spawnedPhysicsObject);
		}

		// Get the sum of the weights of the different animals.
		foreach (Animal animal in _selectedAnimals)
			_weightSum += animal.Weight;
	}

	/// <summary>
	/// Submit the player's input for the weight.
	/// <para>Also resets the game if the input was correct.</para>
	/// </summary>
	public void Submit()
	{
		int playerGuess = 0;

		// Parse the input as an integer, if it couldn't be parsed: display an error.
		if (!Int32.TryParse(_inputText.text, out playerGuess))
		{
			Debug.LogError($"Input '{_inputText.text}' could not be passed as integer!");
			_indicator.color = Color.red;
			return;
		}

		// Input is an integer, check if it is the same as the weight calculated when the animals were selected.
		if (playerGuess == _weightSum)
		{
			_indicator.color = Color.green;
			ResetWeightGuessGame();

			_comboManager.IncrementComboCounter();
		}
		else
		{
			_indicator.color = Color.red;
			_comboManager.ResetComboCounter();
		}
	}

	/// <summary>
	/// Gets a random point in a collider.
	/// </summary>
	/// <param name="bounds">The bounds of the collider</param>
	/// <param name="scale">The scale of the colliders</param>
	/// <returns>A random position in the bounds of the collider</returns>
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

	/// <summary>
	/// Reset the Guess the Weight game.
	/// </summary>
	public void ResetWeightGuessGame()
	{
		// Delete the physics objects.
		foreach (GameObject physicsObject in _selectedAnimalsObjs)
		{
			Destroy(physicsObject);
		}

		// Clear the lists of animals.
		_selectedAnimalsObjs.Clear();
		_selectedAnimals.Clear();

		// Select new animals.
		SelectAnimals();
	}

	/// <summary>
	/// Increase the number of animals the animal manager can spawn by 1.
	/// </summary>
	public void IncreaseAnimalVariety()
	{
		Animal newAnimal = _allPossibleAnimals[UnityEngine.Random.Range(0, _allPossibleAnimals.Count)];

		// If the current animal variety contains the randomly chosen animal - choose another animal (no double ups!)
		while (_currentAnimalVariety.Contains(newAnimal))
		{
			newAnimal = _allPossibleAnimals[UnityEngine.Random.Range(0, _allPossibleAnimals.Count)];
		}

		_currentAnimalVariety.Add(newAnimal);
	}

	static public AnimalManager GetInstance()
	{
		return _instance;
	}
}