using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XRL;
using XRL.UI;

namespace Qud.UI;

[UIView("MouseBlocker", false, false, false, null, null, false, 0, false, UICanvas = "MouseBlocker", UICanvasHost = 1)]
public class MouseBlocker : SingletonWindowBase<MouseBlocker>, IPointerClickHandler, IEventSystemHandler
{
	private Image blocker;

	private int clickCount;

	private float clickReset;

	public async void UpdateOptions()
	{
		await The.UiContext;
		if (Options.MouseInput)
		{
			Hide();
			gameObject.SetActive(value: false);
			_canvas.enabled = false;
			_raycaster.enabled = false;
			return;
		}
		raycaster.enabled = true;
		takesInput = true;
		Show();
		canvas.enabled = true;
		if (blocker != null)
		{
			blocker.enabled = true;
		}
	}

	public override bool AllowPassthroughInput()
	{
		return true;
	}

	public void Update()
	{
		if (!canvas.enabled || !raycaster.enabled)
		{
			canvas.enabled = true;
			raycaster.enabled = true;
		}
		if (clickCount > 0 && clickReset < Time.realtimeSinceStartup)
		{
			clickCount = 0;
		}
		if (transform.GetSiblingIndex() != transform.parent.childCount - 1)
		{
			transform.SetAsLastSibling();
		}
	}

	public async void OnPointerClick(PointerEventData eventData)
	{
		clickReset = Time.realtimeSinceStartup + 60f;
		clickCount++;
		if (clickCount >= 5)
		{
			if (blocker == null)
			{
				blocker = GetComponent<Image>();
			}
			blocker.enabled = false;
			DialogResult dialogResult = await Popup.ShowYesNoAsync("Mouse input is disabled but you clicked on the screen several times. Would you like to enable mouse input?");
			blocker.enabled = true;
			if (dialogResult == DialogResult.Yes)
			{
				Options.SetOption("OptionMouseInput", Value: true);
			}
		}
	}
}
