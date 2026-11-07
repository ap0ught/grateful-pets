using XRL.World;

namespace XRL;

public abstract class IPartEntry
{
	public const int FLAG_HIDDEN = 1;

	public const int FLAG_OBFUSCATED = 2;

	public const int FLAG_INITIATORY = 4;

	public const int FLAG_EX_POOL = 8;

	public string Name;

	public string Class;

	public string Attribute;

	public string Snippet = "";

	public int Cost = -999;

	public int Flags;

	/// <summary>Entry is hidden until acquired.</summary>
	public bool Hidden
	{
		get
		{
			return Flags.HasBit(1);
		}
		set
		{
			Flags.SetBit(1, value);
		}
	}

	/// <summary>Entry is excluded from random acquisition by mutation/skill pools.</summary>
	public bool ExcludeFromPool
	{
		get
		{
			return Flags.HasBit(8);
		}
		set
		{
			Flags.SetBit(8, value);
		}
	}

	public abstract IPart Instance { get; }

	public virtual void HandleXMLNode(XmlDataHelper Reader)
	{
		Class = Reader.ParseAttribute("Class", Class, Class == null);
		Attribute = Reader.ParseAttribute("Attribute", Attribute);
		Snippet = Reader.ParseAttribute("Snippet", Snippet);
		Cost = Reader.ParseAttribute("Cost", Cost);
		Hidden = !Reader.ParseAttribute("Visible", !Hidden);
		Hidden = Reader.ParseAttribute("Hidden", Hidden);
		ExcludeFromPool = Reader.ParseAttribute("ExcludeFromPool", ExcludeFromPool);
	}
}
