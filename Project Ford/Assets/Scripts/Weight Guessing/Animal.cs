using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Animal", menuName = "Animals")]
public class Animal : ScriptableObject
{
	[SerializeField] private int _weight = 0;

	[SerializeField] private Sprite _animalSprite = null;
	
	[SerializeField] private GameObject _animalPhysicsObject = null;

	public int GetWeight()
	{
		return _weight;
	}

	public Sprite GetSprite()
	{
		return _animalSprite;
	}

	public GameObject GetPhysicsObject()
	{
		return _animalPhysicsObject;
	}
}