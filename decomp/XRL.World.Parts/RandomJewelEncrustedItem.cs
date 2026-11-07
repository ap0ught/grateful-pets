using System;
using Qud.API;
using XRL.World.Tinkering;

namespace XRL.World.Parts;

[Serializable]
public class RandomJewelEncrustedItem : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool CanGenerateStacked()
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeObjectCreatedEvent.ID && ID != EnteredCellEvent.ID)
		{
			return ID == TakenEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeObjectCreatedEvent E)
	{
		GenerateJewelEncrustedItem(E.Context, E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TakenEvent E)
	{
		GenerateJewelEncrustedItem(E.Context);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		GenerateJewelEncrustedItem();
		return base.HandleEvent(E);
	}

	private void GenerateJewelEncrustedItem(string Context = null, IObjectCreationEvent E = null)
	{
		GameObject gameObject = null;
		bool flag = false;
		do
		{
			gameObject?.Obliterate();
			gameObject = GameObject.Create(EncountersAPI.GetARandomDescendantOf("Item"), 0, 0, null, null, null, Context);
			if (!gameObject.HasTag("NoSwapInAtGenerate") && gameObject.GetPropertyOrTag("Mods", "").Contains("CommonMods") && gameObject.HasTag("Tier") && gameObject.GetTier() < 7)
			{
				flag = true;
			}
		}
		while (!flag);
		if (!gameObject.HasPart<ModJewelEncrusted>())
		{
			ItemModding.ApplyModification(gameObject, "ModJewelEncrusted", 1);
		}
		if (E != null)
		{
			E.ReplacementObject = gameObject;
		}
		else
		{
			ParentObject.ReplaceWith(gameObject);
		}
	}
}
