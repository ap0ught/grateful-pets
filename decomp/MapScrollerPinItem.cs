using UnityEngine;
using XRL.UI;

public class MapScrollerPinItem : MonoBehaviour
{
	public UITextSkin titleText;

	public UITextSkin detailsText;

	public void SetData(MapScrollerController.MapPinData data)
	{
		titleText.SetText(data.title);
		detailsText.SetText(data.details);
	}
}
