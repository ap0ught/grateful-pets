using System.Collections.Generic;
using Qud.API;
using XRL.Language;

namespace XRL.World.Conversations.Parts;

public abstract class IKithAndKinPart : IConversationPart
{
	protected List<string> _Eliminated;

	protected List<JournalObservation> Circumstances
	{
		get
		{
			if (!IConversationPart.TryGetState<List<JournalObservation>>("Circumstances", out var Value))
			{
				return null;
			}
			return Value;
		}
		set
		{
			IConversationPart.State["Circumstances"] = value;
		}
	}

	protected List<JournalObservation> Motives
	{
		get
		{
			if (!IConversationPart.TryGetState<List<JournalObservation>>("Motives", out var Value))
			{
				return null;
			}
			return Value;
		}
		set
		{
			IConversationPart.State["Motives"] = value;
		}
	}

	protected JournalObservation Circumstance
	{
		get
		{
			if (!IConversationPart.TryGetState<JournalObservation>("Circumstance", out var Value))
			{
				return null;
			}
			return Value;
		}
		set
		{
			IConversationPart.State["Circumstance"] = value;
		}
	}

	protected JournalObservation Motive
	{
		get
		{
			if (!IConversationPart.TryGetState<JournalObservation>("Motive", out var Value))
			{
				return null;
			}
			return Value;
		}
		set
		{
			IConversationPart.State["Motive"] = value;
		}
	}

	protected List<string> Eliminated
	{
		get
		{
			if (_Eliminated != null)
			{
				return _Eliminated;
			}
			if (The.Game.ObjectGameState.TryGetValue("KithAndKinEliminated", out var value))
			{
				return _Eliminated = (List<string>)value;
			}
			_Eliminated = new List<string>();
			The.Game.ObjectGameState["KithAndKinEliminated"] = _Eliminated;
			return _Eliminated;
		}
	}

	protected string CircumstanceInfluence
	{
		get
		{
			if (Circumstance == null || !Circumstance.TryGetAttribute("influence:", out var Value))
			{
				return "trade";
			}
			return Value;
		}
	}

	protected string MotiveInfluence
	{
		get
		{
			if (Motive == null || !Motive.TryGetAttribute("influence:", out var Value))
			{
				return "craft";
			}
			return Value;
		}
	}

	protected string Thief
	{
		get
		{
			if (Motive == null || !Motive.TryGetAttribute("motive:", out var Value))
			{
				return "keh";
			}
			return Value;
		}
	}

	protected string ThiefName => Thief switch
	{
		"kendren" => "a kendren", 
		"esk" => "Eskhind", 
		"kese" => "Kesehind", 
		_ => "Keh", 
	};

	public static string KeyOf(string Key)
	{
		return Key switch
		{
			"kendren" => "Kendren", 
			"esk" => "Esk", 
			"kese" => "Kese", 
			"keh" => "Keh", 
			"love" => "Love", 
			"nolove" => "NoLove", 
			"tumultuous" => "Tumultuous", 
			"prosperous" => "Prosperous", 
			"mixed" => "Mixed", 
			_ => Grammar.InitCap(Key), 
		};
	}
}
