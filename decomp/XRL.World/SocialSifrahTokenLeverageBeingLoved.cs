using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class SocialSifrahTokenLeverageBeingLoved : SifrahToken
{
	private string UseFaction;

	public SocialSifrahTokenLeverageBeingLoved()
	{
		Description = "leverage being loved";
		Tile = "Items/ms_heart.png";
		RenderString = "\u0003";
		ColorString = "&R";
		DetailColor = 'Y';
	}

	public SocialSifrahTokenLeverageBeingLoved(GameObject Representative)
		: this()
	{
		UseFaction = Representative.GetPrimaryFaction();
	}

	public SocialSifrahTokenLeverageBeingLoved(string Faction)
		: this()
	{
		UseFaction = Faction;
	}

	public override string GetDescription(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		if (!UseFaction.IsNullOrEmpty())
		{
			return Description + " by " + Faction.GetFormattedName(UseFaction);
		}
		return Description;
	}
}
