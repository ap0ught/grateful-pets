using UnityEngine.EventSystems;

namespace AiUnity.Common.LogUI.Scripts;

public interface IButtonStateChange : IEventSystemHandler
{
	void ButtonStateChange(ButtonState buttonState);
}
