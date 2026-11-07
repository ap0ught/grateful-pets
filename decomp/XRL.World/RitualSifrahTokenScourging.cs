using System;
using XRL.Rules;
using XRL.UI;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class RitualSifrahTokenScourging : SifrahPrioritizableToken
{
	public static readonly string BLUEPRINT = "Leather Whip";

	public RitualSifrahTokenScourging()
	{
		Description = "scourge myself with a leather whip";
		Tile = "Items/sw_whip_1.bmp";
		RenderString = "\u00a8";
		ColorString = "&w";
		DetailColor = 'K';
	}

	public override int GetPriority()
	{
		if (!IsAvailable())
		{
			return 0;
		}
		return The.Player.Stat("Hitpoints");
	}

	public override int GetTiebreakerPriority()
	{
		return 0;
	}

	public bool IsAvailable()
	{
		return The.Player.HasObjectInInventory(BLUEPRINT);
	}

	public override bool CheckTokenUse(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		if (!IsAvailable())
		{
			Popup.ShowFail("You do not have a leather whip.");
			return false;
		}
		return base.CheckTokenUse(Game, Slot, ContextObject);
	}

	public override int GetPowerup(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		if (Slot.CurrentMove == Slot.Token)
		{
			return 1;
		}
		return base.GetPowerup(Game, Slot, ContextObject);
	}

	public override void UseToken(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		GameObject gameObject = The.Player.FindObjectInInventory(BLUEPRINT);
		if (gameObject != null)
		{
			gameObject.ModIntProperty("SifrahActions", 1);
			gameObject.SetLongProperty("LastSifrahActionTurn", The.CurrentTurn);
		}
		SoundManager.PlaySound("Sounds/Damage/sfx_damage_stone");
		The.Player.TakeDamage(Stat.Random(1, 4), "from scourging yourself.", null, "You scourged yourself to death.", null, The.Player);
		base.UseToken(Game, Slot, ContextObject);
	}
}
