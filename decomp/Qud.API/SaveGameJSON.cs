using System;
using System.Collections.Generic;

namespace Qud.API;

[Serializable]
public class SaveGameJSON
{
	public int InfoVersion = 1;

	public int SaveVersion;

	public string GameVersion;

	public string ID;

	public string Name;

	public int Level;

	public string GenoSubType;

	public string GameMode;

	public string CharIcon;

	public char FColor;

	public char DColor;

	public string Location;

	public string InGameTime;

	public long Turn;

	public string SaveTime;

	public List<string> ModsEnabled;
}
