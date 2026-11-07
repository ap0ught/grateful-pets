using System.Linq;
using Genkit;

namespace XRL.World.Conversations;

[HasGameBasedStaticCache]
public class Choice : IConversationElement
{
	/// <summary>Will not store its hash among visited choices.</summary>
	public bool Transient;

	public static FixedHashSet Hashes = new FixedHashSet();

	private ulong _Hash;

	private string _Target;

	public override int Propagation => 1;

	public ulong Hash
	{
		get
		{
			if (_Hash != 0)
			{
				return _Hash;
			}
			IConversationElement parent = Parent;
			while (parent.Parent != null)
			{
				parent = parent.Parent;
			}
			_Hash = Genkit.Hash.FNV1A64(parent.ID ?? "[Unknown]");
			_Hash = Genkit.Hash.FNV1A64(ID, _Hash);
			_Hash = Genkit.Hash.FNV1A64(Texts?.FirstOrDefault()?.Text ?? Text ?? "[Empty]", _Hash);
			return _Hash;
		}
	}

	public bool Visited => Hashes.Contains(Hash);

	public string Target
	{
		get
		{
			return _Target;
		}
		set
		{
			_Target = value;
			if (value == "End")
			{
				Priority -= 999999;
			}
		}
	}

	[GameBasedCacheInit]
	public static void ResetHashes()
	{
		Hashes.Clear();
	}

	public override void Entered()
	{
		base.Entered();
		if (!Transient)
		{
			Hashes.Add(Hash);
		}
	}

	public override string GetTextColor()
	{
		string Color = "G";
		if (Visited)
		{
			Color = "g";
		}
		ColorTextEvent.Send(this, ref Color);
		return Color;
	}

	public override string GetDisplayText(bool WithColor = false)
	{
		string text = base.GetDisplayText(WithColor);
		string tag = GetTag();
		if (!tag.IsNullOrEmpty())
		{
			text = text + " " + tag;
		}
		return text;
	}

	public string GetTag()
	{
		string text = GetChoiceTagEvent.For(this);
		if (!text.IsNullOrEmpty())
		{
			return text;
		}
		if (Target == "End")
		{
			return "{{K|[End]}}";
		}
		return null;
	}
}
