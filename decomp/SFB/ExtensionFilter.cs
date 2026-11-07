namespace SFB;

public struct ExtensionFilter(string filterName, params string[] filterExtensions)
{
	public string Name = filterName;

	public string[] Extensions = filterExtensions;
}
