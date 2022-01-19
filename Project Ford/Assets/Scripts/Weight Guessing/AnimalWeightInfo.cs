using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnimalWeightInfo : MonoBehaviour
{
	[SerializeField] private Image _image = null;
	[SerializeField] private TextMeshProUGUI _animal = null;
	[SerializeField] private TextMeshProUGUI _weight = null;

	public void SetValues(Animal animal)
	{
		_image.sprite = animal.Sprite;
		_animal.text = animal.name;
		_weight.text = $"{animal.Weight} kg";
	}
}
