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
	/// Just an image that changes colour to show if the input was right or wrong (just for prototyping).
	/// </summary>
	/// <returns></returns>
	[SerializeField] private Indicator _indicator = null;

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
	/// The starting amount of animals the manager spawns at the start of the round.
	/// </summary>
	[SerializeField] private int _startingAnimalSpawnAmount = 3;

	/// <summary>
	/// The maximum amount of animals the manager can be able to spawn.
	/// </summary>
	[SerializeField] private int _maxAnimalSpawnAmount = 5;

	[SerializeField] private Toggle _endlessModeToggle = null;

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
	/// The current amount of animals the manager spawns at the start of a round.
	/// </summary>
	private int _currentSpawnAmount = 3;

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
	}

	void Start()
	{
		_comboManager = ComboManager.GetInstance();
	}

	/// <summary>
	/// Reset the game.
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

		_weightSum = 0;

		// Select random animals from current variety.
		for (int i = 0; i < _currentSpawnAmount; ++i)
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
	public bool Submit(int playerGuess)
	{
		// Input is an integer, check if it is the same as the weight calculated when the animals were selected.
		if (playerGuess == _weightSum)
		{
			_indicator.SetIndicator(true);
			_comboManager.IncrementComboCounter();

			ResetWeightGuessGame();
			return true;
		}
		else
		{
			_indicator.SetIndicator(false);
			_comboManager.ResetComboCounter(false);
		}

		return false;
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
	/// Increase the number of animals the animal manager can spawn by 1.
	/// </summary>
	public void IncreaseAnimalVariety()
	{
		// Only get new animal if we don't already have all the animals avaliable to spawn.
		if (_currentAnimalVariety.Count < _allPossibleAnimals.Count)
		{
			_currentAnimalVariety.Add(_allPossibleAnimals[_currentAnimalVariety.Count]);

			// Add the last newly added animal to the weight list.
			GameObject animal = Instantiate(_weightPrefab, _weightList);
			animal.GetComponent<AnimalWeightInfo>().SetValues(_currentAnimalVariety[_currentAnimalVariety.Count - 1]);
		}
	}

	public void GetDifficultyFromManager()
	{
		DifficultyManager dmInstance = DifficultyManager.GetInstance();
		DifficultyManager.DifficultyEnum selectedDifficulty = dmInstance.GetDifficulty();
		DifficultyManager.DifficultySettings settings = new DifficultyManager.DifficultySettings(0, 0);
		int variety = 0;

		// Go through each of the difficulty settings.
		switch (selectedDifficulty)
		{
			// Easy.
			case DifficultyManager.DifficultyEnum.Easy:
				settings = dmInstance.GetEasyDifficultySettings();
				variety = settings._difAnimals;

				for (int i = 0; i < variety; ++i)
					_currentAnimalVariety.Add(_allPossibleAnimals[i]);
			break;

			// Medium.
			case DifficultyManager.DifficultyEnum.Medium:
				settings = dmInstance.GetMediumDifficultySettings();
				variety = settings._difAnimals;

				for (int i = 0; i < variety; ++i)
					_currentAnimalVariety.Add(_allPossibleAnimals[i]);
			break;

			// Hard
			case DifficultyManager.DifficultyEnum.Hard:
				settings = dmInstance.GetHardDifficultySettings();
				variety = settings._difAnimals;

				for (int i = 0; i < variety; ++i)
					_currentAnimalVariety.Add(_allPossibleAnimals[i]);
			break;

			default:
				Debug.LogError("Error in determining selected difficulty, difficulty was " + selectedDifficulty, this);
			break;
		}

		_comboManager.SetFinishQuestionAmount(settings._finishQuestionStreakAmount);
		_comboManager.SetEndlessMode(_endlessModeToggle.isOn);

		// Spawn the weight text.
		_currentAnimalVariety.ForEach(animal => Instantiate(_weightPrefab, _weightList).GetComponent<AnimalWeightInfo>().SetValues(animal));
	}

	/// <summary>
	/// Increase the amount of animals the manager spawns at the start of the round up to a maximum amount.
	/// </summary>
	public void IncreaseAnimalSpawnAmount()
	{
		// Only increase the current spawn amount if it's below the maximum.
		if (_currentSpawnAmount < _maxAnimalSpawnAmount)
			_currentSpawnAmount++;
	}

	static public AnimalManager GetInstance()
	{
		return _instance;
	}
}