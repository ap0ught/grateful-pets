using System;
using XRL.World.Effects;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class SocialSifrahTokenThePowerOfLove : SifrahToken
{
	public SocialSifrahTokenThePowerOfLove()
	{
		Description = "rhapsodize about your true love";
		Tile = "Items/ms_heart.png";
		RenderString = "\u0003";
		ColorString = "&W";
		DetailColor = 'R';
	}

	public static bool IsAvailable()
	{
		return The.Player.HasEffect<Lovesick>();
	}

	public override int GetPowerup(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		if (Slot.CurrentMove == Slot.Token)
		{
			return 1;
		}
		return base.GetPowerup(Game, Slot, ContextObject);
	}
}
