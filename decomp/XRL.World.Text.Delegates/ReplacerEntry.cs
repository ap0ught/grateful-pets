namespace XRL.World.Text.Delegates;

public struct ReplacerEntry(Replacer Delegate, string Default = null, int Flags = 0, bool Capitalize = false)
{
	public Replacer Delegate = Delegate;

	public string Default = Default;

	public int Flags = Flags;

	public bool Capitalize = Capitalize;
}
