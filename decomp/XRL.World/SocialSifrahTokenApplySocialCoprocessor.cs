using System;
using System.Collections.Generic;
using XRL.World.Parts;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class SocialSifrahTokenApplySocialCoprocessor : SifrahPrioritizableToken
{
	public SocialSifrahTokenApplySocialCoprocessor()
	{
		Description = "apply social coprocessor";
		Tile = "Items/sw_social_coprocessor.bmp";
		RenderString = "\u0001";
		ColorString = "&r";
		DetailColor = 'C';
	}

	public override int GetPriority()
	{
		if (!IsPotentiallyAvailable())
		{
			return 0;
		}
		if (!IsAvailable())
		{
			return 536870911;
		}
		return 1610612733;
	}

	public override int GetTiebreakerPriority()
	{
		if (!The.Player.IsTrueKin())
		{
			return 0;
		}
		return 1610612733;
	}

	public override bool CheckTokenUse(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		if (!UsabilityCheckedThisTurn && !IsAvailable())
		{
			DisabledThisTurn = true;
			return false;
		}
		return true;
	}

	public static bool IsPotentiallyAvailable()
	{
		return HasAnySocialCoprocessor(The.Player);
	}

	public static bool IsAvailable()
	{
		return HasUsableSocialCoprocessor(The.Player);
	}

	public override void UseToken(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		GameObject usableSocialCoprocessor = GetUsableSocialCoprocessor(The.Player);
		if (usableSocialCoprocessor != null)
		{
			usableSocialCoprocessor.ModIntProperty("SifrahActions", 1);
			usableSocialCoprocessor.SetLongProperty("LastSifrahActionTurn", The.CurrentTurn);
		}
		base.UseToken(Game, Slot, ContextObject);
	}

	public static GameObject GetAnySocialCoprocessor(GameObject Actor)
	{
		List<GameObject> list = Actor?.GetInstalledCybernetics();
		if (list == null || list.Count <= 0)
		{
			return null;
		}
		foreach (GameObject item in list)
		{
			if (item.HasPart<CyberneticsSocialCoprocessor>())
			{
				return item;
			}
		}
		return null;
	}

	public static bool HasAnySocialCoprocessor(GameObject Actor)
	{
		return GetAnySocialCoprocessor(Actor) != null;
	}

	public static GameObject GetUsableSocialCoprocessor(GameObject Actor)
	{
		List<GameObject> list = Actor?.GetInstalledCybernetics();
		if (list == null || list.Count <= 0)
		{
			return null;
		}
		foreach (GameObject item in list)
		{
			if (item.TryGetPart<CyberneticsSocialCoprocessor>(out var Part) && Part.IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				return item;
			}
		}
		return null;
	}

	public static bool HasUsableSocialCoprocessor(GameObject Actor)
	{
		return GetUsableSocialCoprocessor(Actor) != null;
	}
}
