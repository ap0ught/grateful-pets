using UnityEngine;
using UnityEngine.UI;
using XRL.UI;
using XRL.UI.Framework;

namespace XRL.CharacterBuilds.UI;

[RequireComponent(typeof(FrameworkContext))]
[RequireComponent(typeof(FrameworkClickToAccept))]
[RequireComponent(typeof(FrameworkHoverable))]
public class EmbarkBuilderModuleBackButton : MonoBehaviour
{
	public enum State
	{
		Default,
		Active,
		Disabled
	}

	public Image ButtonImage;

	public UITextSkin TextSkin;

	public MenuOption menuOption = EmbarkBuilderOverlayWindow.BackMenuOption;

	public Color disabledColor;

	public Color activeColor;

	public Color defaultColor;

	private NavigationContext _navigationContext;

	private State? lastState;

	public NavigationContext navigationContext
	{
		get
		{
			return _navigationContext ?? (_navigationContext = GetComponent<FrameworkContext>().context);
		}
		set
		{
			NavigationContext navigationContext = (GetComponent<FrameworkContext>().context = value);
			_navigationContext = navigationContext;
		}
	}

	public Color currentColor
	{
		get
		{
			if (lastState == State.Disabled)
			{
				return disabledColor;
			}
			if (lastState == State.Active)
			{
				return activeColor;
			}
			return defaultColor;
		}
	}

	public void Awake()
	{
		if (navigationContext == null)
		{
			navigationContext = new NavigationContext();
		}
	}

	public State GetState()
	{
		if (navigationContext.disabled)
		{
			return State.Disabled;
		}
		if (navigationContext.IsActive())
		{
			return State.Active;
		}
		return State.Default;
	}

	public void ForceUpdate()
	{
		lastState = null;
		Update();
	}

	public void Update()
	{
		if (lastState != GetState())
		{
			lastState = GetState();
			ButtonImage.color = currentColor;
			TextSkin.text = menuOption.getMenuText();
			TextSkin.color = currentColor;
			TextSkin.StripFormatting = GetState() == State.Disabled;
			TextSkin.Apply();
		}
	}
}
