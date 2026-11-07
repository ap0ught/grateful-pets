namespace XRL.World.Conversations.Parts;

public class RequireReputation : IConversationPart
{
	public string Faction;

	public int Value = int.MaxValue;

	public bool Fulfilled;

	public string Level
	{
		set
		{
			Value = value.ToUpperInvariant() switch
			{
				"LOVED" => 2, 
				"LIKED" => 1, 
				"INDIFFERENT" => 0, 
				"DISLIKED" => -1, 
				"HATED" => -2, 
				_ => int.MaxValue, 
			};
		}
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != PrepareTextEvent.ID && ID != EnterElementEvent.ID && ID != GetChoiceTagEvent.ID)
		{
			return ID == ColorTextEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(PrepareTextEvent E)
	{
		Fulfilled = The.Game.PlayerReputation.GetLevel(Faction ?? The.Speaker.GetPrimaryFaction()) >= Value;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnterElementEvent E)
	{
		return Fulfilled;
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		Faction faction = Factions.Get(Faction ?? The.Speaker.GetPrimaryFaction());
		char c = (Fulfilled ? 'C' : 'r');
		string text = "Hated by ";
		if (Value >= 2)
		{
			text = "Loved by ";
		}
		else if (Value >= 1)
		{
			text = "Liked by ";
		}
		else if (Value >= 0)
		{
			text = "Indifferent to ";
		}
		else if (Value >= -1)
		{
			text = "Disliked by ";
		}
		E.Tag = "{{" + c + "|[" + text + faction.GetFormattedName() + "]}}";
		return false;
	}

	public override bool HandleEvent(ColorTextEvent E)
	{
		if (!Fulfilled)
		{
			E.Color = "K";
			return false;
		}
		return base.HandleEvent(E);
	}
}
