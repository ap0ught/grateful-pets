using XRL.UI;

namespace Qud.UI;

public class OptionsComboBoxRow : OptionsDataRow<string>
{
	public string[] Options => Option.Values;

	public string[] DisplayOptions => Option.DisplayValues;

	public OptionsComboBoxRow(GameOption Option)
		: base(Option)
	{
		Initialize(XRL.UI.Options.GetOption(Option.ID));
	}
}
