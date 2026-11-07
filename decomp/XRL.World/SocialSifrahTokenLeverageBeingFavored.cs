using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class SocialSifrahTokenLeverageBeingFavored : SifrahToken
{
	private string UseFaction;

	public SocialSifrahTokenLeverageBeingFavored()
	{
		Description = "leverage being favored";
		Tile = "Items/ms_happy_face.png";
		RenderString = "\u0001";
		ColorString = "&M";
		DetailColor = 'Y';
	}

	public SocialSifrahTokenLeverageBeingFavored(GameObject Representative)
		: this()
	{
		UseFaction = Representative.GetPrimaryFaction();
	}

	public SocialSifrahTokenLeverageBeingFavored(string Faction)
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
