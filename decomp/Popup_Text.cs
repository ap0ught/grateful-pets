using QupKit;
using UnityEngine;
using UnityEngine.UI;
using XRL.UI;

[UIView("Popup:Text", false, false, false, "Menu", "Popup:Text", false, 0, false)]
public class Popup_Text : BaseView
{
	public new static string Text;

	public Vector2 originalDialogSize = Vector2.zero;

	public Vector2 originalTextSize = Vector2.zero;

	public override void Enter()
	{
		base.Enter();
		if (originalDialogSize == Vector2.zero)
		{
			originalDialogSize = GetChildComponent<RectTransform>("Controls").sizeDelta;
			originalTextSize = GetChildComponent<RectTransform>("Controls/Message").sizeDelta;
		}
		GetChildComponent<RectTransform>("Controls/Message").sizeDelta = originalTextSize;
		GetChildComponent<UnityEngine.UI.Text>("Controls/Message").text = Text;
		Canvas.ForceUpdateCanvases();
		GetChildComponent<RectTransform>("Controls/Message").sizeDelta = new Vector2(GetChildComponent<UnityEngine.UI.Text>("Controls/Message").preferredWidth, GetChildComponent<UnityEngine.UI.Text>("Controls/Message").preferredHeight);
		Vector2 vector = GetChildComponent<RectTransform>("Controls/Message").sizeDelta;
		if (vector.x < 850f)
		{
			vector = new Vector2(850f, vector.y);
		}
		if (vector.y < originalTextSize.y)
		{
			vector = new Vector2(vector.x, originalTextSize.y);
		}
		GetChildComponent<RectTransform>("Controls").sizeDelta = originalDialogSize + (vector - originalTextSize);
	}
}
