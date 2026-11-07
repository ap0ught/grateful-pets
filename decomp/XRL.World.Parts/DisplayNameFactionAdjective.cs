using System;

namespace XRL.World.Parts;

[Serializable]
public class DisplayNameFactionAdjective : IPart
{
	public string Faction;

	public string FactionAdjective;

	public string NonFactionAdjective;

	public bool EvenIfProperName;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetDisplayNameEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (EvenIfProperName || !E.Object.HasProperName)
		{
			if (E.Object.IsFactionMember(Faction))
			{
				if (!FactionAdjective.IsNullOrEmpty())
				{
					E.AddAdjective(FactionAdjective, 110);
				}
			}
			else if (!NonFactionAdjective.IsNullOrEmpty())
			{
				E.AddAdjective(NonFactionAdjective, 110);
			}
		}
		return base.HandleEvent(E);
	}
}
