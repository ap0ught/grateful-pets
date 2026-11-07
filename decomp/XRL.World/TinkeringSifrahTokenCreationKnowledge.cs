using System;
using XRL.UI;
using XRL.World.Tinkering;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class TinkeringSifrahTokenCreationKnowledge : SifrahPrioritizableToken
{
	public string Blueprint;

	public string FailureDescription;

	public TinkeringSifrahTokenCreationKnowledge()
	{
		Description = "apply knowledge of this artifact's manufacture";
		FailureDescription = "You do not know how to manufacture this kind of artifact.";
		Tile = "Items/sw_anvil.bmp";
		RenderString = "Ò";
		ColorString = "&K";
		DetailColor = 'w';
	}

	public TinkeringSifrahTokenCreationKnowledge(string Blueprint)
		: this()
	{
		this.Blueprint = Blueprint;
		if (!string.IsNullOrEmpty(Blueprint))
		{
			GameObject gameObject = GameObject.CreateSample(Blueprint);
			string pluralName = gameObject.GetPluralName();
			Description = "apply knowledge of the manufacture of " + pluralName;
			FailureDescription = "You do not know how to manufacture " + pluralName + ".";
			gameObject.Obliterate();
		}
	}

	public TinkeringSifrahTokenCreationKnowledge(GameObject Object)
		: this(Object.GetTinkeringBlueprint())
	{
	}

	public override int GetPriority()
	{
		if (!IsAvailable())
		{
			if (!IsPotentiallyAvailable())
			{
				return int.MinValue;
			}
			return 1;
		}
		return int.MaxValue;
	}

	public override int GetTiebreakerPriority()
	{
		if (!IsPotentiallyAvailable())
		{
			return int.MinValue;
		}
		return int.MaxValue;
	}

	public bool IsAvailable()
	{
		if (!string.IsNullOrEmpty(Blueprint))
		{
			foreach (TinkerData knownRecipe in TinkerData.KnownRecipes)
			{
				if (knownRecipe.Blueprint == Blueprint)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsPotentiallyAvailable()
	{
		return IsPotentiallyAvailableFor(Blueprint);
	}

	public static bool IsPotentiallyAvailableFor(string Blueprint)
	{
		if (!string.IsNullOrEmpty(Blueprint))
		{
			foreach (TinkerData tinkerRecipe in TinkerData.TinkerRecipes)
			{
				if (tinkerRecipe.Blueprint == Blueprint)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool IsPotentiallyAvailableFor(GameObject Object)
	{
		return IsPotentiallyAvailableFor(Object?.Blueprint);
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
			Popup.ShowFail(FailureDescription);
			return false;
		}
		return base.CheckTokenUse(Game, Slot, ContextObject);
	}
}
