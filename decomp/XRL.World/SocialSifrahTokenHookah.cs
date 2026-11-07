using System;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class SocialSifrahTokenHookah : SifrahPrioritizableToken
{
	public static readonly string BLUEPRINT = "Hookah";

	public SocialSifrahTokenHookah()
	{
		Description = "offer a puff on a hookah";
		Tile = "Items/sw_hookah.bmp";
		RenderString = "ë";
		ColorString = "&R";
		DetailColor = 'w';
	}

	public override int GetPriority()
	{
		if (!IsPotentiallyAvailable())
		{
			return 0;
		}
		if (!IsAvailable())
		{
			return 1879048185;
		}
		return int.MaxValue;
	}

	public override int GetTiebreakerPriority()
	{
		return int.MaxValue;
	}

	public bool IsPotentiallyAvailable()
	{
		return The.Player.HasObjectInInventory(BLUEPRINT);
	}

	public bool IsAvailable()
	{
		GameObject gameObject = The.Player.FindObjectInInventory(BLUEPRINT);
		if (gameObject == null)
		{
			return false;
		}
		return gameObject.GetPart<Hookah>()?.CanPuff() ?? false;
	}

	public override bool GetDisabled(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		if (!IsAvailable())
		{
			return true;
		}
		return base.GetDisabled(Game, Slot, ContextObject);
	}

	public override bool CheckTokenUse(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		if (!IsPotentiallyAvailable())
		{
			Popup.ShowFail("You do not have a hookah.");
			return false;
		}
		if (!IsAvailable())
		{
			Popup.ShowFail("Your hookah is not filled with water.");
			return false;
		}
		return base.CheckTokenUse(Game, Slot, ContextObject);
	}

	public override void UseToken(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		GameObject gameObject = The.Player.FindObjectInInventory(BLUEPRINT);
		if (gameObject != null)
		{
			gameObject.ModIntProperty("SifrahActions", 1);
			gameObject.SetLongProperty("LastSifrahActionTurn", The.CurrentTurn);
		}
		for (int i = 2; i < 5; i++)
		{
			ParticleFX.Smoke(X, Y, 150, 180);
		}
		base.UseToken(Game, Slot, ContextObject);
	}
}
