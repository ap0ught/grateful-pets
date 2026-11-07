using System.Collections.Generic;
using System.Linq;

namespace XRL.Help;

public class XRLManualPage
{
	public string Topic;

	public string Data;

	private List<string> _lines;

	public List<string> Lines => _lines ?? (_lines = GetData().Split('\n').ToList());

	public List<string> LinesStripped => Lines;

	public XRLManualPage(string Data)
	{
		this.Data = Data.Replace("\r", "");
	}

	public string GetData(bool StripBrackets = true)
	{
		if (!StripBrackets)
		{
			return Data;
		}
		return Data.Replace("[[", "").Replace("]]", "");
	}
}
