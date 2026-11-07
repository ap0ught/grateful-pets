using System;
using XRL.UI.Framework;

namespace Qud.UI;

[Serializable]
public class HelpDataRow : FrameworkDataElement
{
	public string CategoryId;

	public string HelpText;

	public bool Collapsed;
}
