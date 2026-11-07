using System.Collections.Generic;

namespace XRL.UI;

public class FullscreenPickerEntry
{
	public string Title = "";

	public List<string> Lines = new List<string>();

	public List<string> Details = new List<string>();

	public FullscreenPickerEntry(string title, List<string> Lines)
	{
		Title = title;
		this.Lines.AddRange(Lines);
	}
}
