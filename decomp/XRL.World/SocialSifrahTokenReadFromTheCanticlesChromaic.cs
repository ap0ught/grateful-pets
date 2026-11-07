using System;
using XRL.UI;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class SocialSifrahTokenReadFromTheCanticlesChromaic : SifrahPrioritizableToken
{
	public static readonly string BLUEPRINT = "Canticles3";

	public SocialSifrahTokenReadFromTheCanticlesChromaic()
	{
		Description = "read from the Canticles Chromaic";
		Tile = "Items/sw_book2.bmp";
		RenderString = "ë";
		ColorString = "&c";
		DetailColor = 'W';
	}

	public override int GetPriority()
	{
		if (!IsAvailable())
		{
			return 0;
		}
		return int.MaxValue;
	}

	public override int GetTiebreakerPriority()
	{
		return int.MaxValue;
	}

	public bool IsAvailable()
	{
		return The.Player.HasObjectInInventory(BLUEPRINT);
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
		if (!IsAvailable())
		{
			Popup.ShowFail("You do not have a copy of the Canticles Chromaic.");
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
		string text = BookUI.Books["Preacher1"].Pages.GetRandomElement().FullText.Replace("\n", " ").Replace("  ", " ").Trim();
		The.Player.ParticleText("{{W|'" + text + "'}}");
		base.UseToken(Game, Slot, ContextObject);
	}
}
