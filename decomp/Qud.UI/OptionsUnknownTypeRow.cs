using XRL.UI;

namespace Qud.UI;

public class OptionsUnknownTypeRow : OptionsDataRow<string>
{
	public string Type => Option.Type;

	public OptionsUnknownTypeRow(GameOption Option)
		: base(Option)
	{
		Initialize(Options.GetOption(Option.ID));
	}
}
