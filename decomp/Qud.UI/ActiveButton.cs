using UnityEngine;
using UnityEngine.UI;

namespace Qud.UI;

[ExecuteAlways]
public class ActiveButton : MonoBehaviour
{
	public Image TargetImage;

	public Sprite ActiveImage;

	public Sprite DisabledImage;

	public Color InactiveColor;

	public Color ActiveColor;

	public bool IsActive;

	public bool? _wasActive;

	public void Update()
	{
		if (_wasActive == IsActive)
		{
			return;
		}
		_wasActive = IsActive;
		Button component = gameObject.GetComponent<Button>();
		if (component != null)
		{
			ColorBlock colors = component.colors;
			colors.normalColor = (IsActive ? ActiveColor : InactiveColor);
			colors.selectedColor = (IsActive ? ActiveColor : InactiveColor);
			component.colors = colors;
			if (TargetImage != null && ActiveImage != null && DisabledImage != null)
			{
				TargetImage.sprite = (IsActive ? ActiveImage : DisabledImage);
			}
		}
	}
}
