namespace XRL.UI.Framework;

/// <summary>
/// Used to describe menu options in the shared overlay.
/// </summary>
public class MenuOption : FrameworkDataElement
{
	public string KeyDescription;

	public string InputCommand;

	public bool disabled;

	public string getKeyDescription()
	{
		if (!string.IsNullOrEmpty(InputCommand))
		{
			KeyDescription = ControlManager.getCommandInputDescription(InputCommand);
		}
		return KeyDescription;
	}

	public string getMenuText()
	{
		if (!string.IsNullOrEmpty(InputCommand))
		{
			KeyDescription = ControlManager.getCommandInputDescription(InputCommand);
		}
		if (!string.IsNullOrEmpty(KeyDescription))
		{
			return "[{{W|" + KeyDescription + "}}] " + Description;
		}
		return Description;
	}
}
