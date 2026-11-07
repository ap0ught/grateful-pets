using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AiUnity.Common.LogUI.Scripts;

public class ButtonStateManager : MonoBehaviour
{
	public bool pressed;

	public Sprite pressedSprite;

	private Image buttonImage;

	public Button Button { get; set; }

	public void ToggleButtonState()
	{
		ToggleButtonState(!pressed);
	}

	public void ToggleButtonState(bool pressed)
	{
		this.pressed = pressed;
		buttonImage.overrideSprite = (this.pressed ? pressedSprite : null);
		ButtonState buttonState = (this.pressed ? ButtonState.Pressed : ((ButtonState)0));
		ExecuteEvents.ExecuteHierarchy(gameObject, null, (IButtonStateChange x, BaseEventData y) =>
		{
			x.ButtonStateChange(buttonState);
		});
	}

	private void Start()
	{
		Button = GetComponent<Button>();
		Button.onClick.AddListener(ToggleButtonState);
		buttonImage = GetComponent<Image>();
		ToggleButtonState(pressed: true);
	}
}
