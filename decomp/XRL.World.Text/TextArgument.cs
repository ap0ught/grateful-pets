namespace XRL.World.Text;

public struct TextArgument(GameObject Object, string Explicit = null, IPronounProvider Pronouns = null)
{
	public GameObject Object = Object;

	public string Explicit = Explicit;

	public IPronounProvider Pronouns = Pronouns;

	public TextArgument(string Explicit, IPronounProvider Pronouns = null)
		: this(null, Explicit, Pronouns)
	{
	}

	public TextArgument(string Explicit, bool ExplicitPlural)
		: this(null, Explicit, ExplicitPlural)
	{
	}

	public TextArgument(GameObject Object, string Explicit, bool ExplicitPlural = false)
		: this(Object, Explicit, ExplicitPlural ? Gender.DefaultPlural : Gender.DefaultNeuter)
	{
	}
}
