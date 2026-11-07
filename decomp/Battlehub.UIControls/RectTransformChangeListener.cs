using UnityEngine.EventSystems;

namespace Battlehub.UIControls;

public class RectTransformChangeListener : UIBehaviour
{
	public event RectTransformChanged RectTransformChanged;

	protected override void OnRectTransformDimensionsChange()
	{
		if (RectTransformChanged != null)
		{
			RectTransformChanged();
		}
	}
}
