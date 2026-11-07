using UnityEngine;
using UnityEngine.UI;

namespace ModelShark;

public class DynamicImage : MonoBehaviour
{
	public string placeholderName;

	[HideInInspector]
	public Image image;

	public string Name { get; set; }

	public Image PlaceholderImage => GetComponent<Image>();
}
