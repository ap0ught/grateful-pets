using System.Collections.Generic;

namespace XRL.UI.Framework;

public class StartingLocationData : FrameworkDataElement, IFrameworkDataHotkey
{
	public string Name;

	public string Location;

	public string Set;

	public string ExcludeFromDaily;

	public List<StartingLocationSkill> skills = new List<StartingLocationSkill>();

	public List<StartingLocationItem> items = new List<StartingLocationItem>();

	public List<StartingLocationReputation> reputations = new List<StartingLocationReputation>();

	public Dictionary<string, StartingLocationGridElement> grid = new Dictionary<string, StartingLocationGridElement>();

	public Dictionary<string, string> stringGameStates = new Dictionary<string, string>();

	public string Hotkey { get; set; }
}
