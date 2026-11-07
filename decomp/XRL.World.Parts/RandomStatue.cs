using System;
using Qud.API;
using XRL.Language;

namespace XRL.World.Parts;

[Serializable]
public class RandomStatue : IPart
{
	public string Material = "Stone";

	public string BaseBlueprint;

	public string StatueType;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		GameObject creature = ((BaseBlueprint == null) ? EncountersAPI.GetACreature() : GameObjectFactory.Factory.CreateObject(BaseBlueprint));
		SetCreature(creature);
		return base.HandleEvent(E);
	}

	public void SetCreature(GameObject creatureObject)
	{
		BaseBlueprint = creatureObject.Blueprint;
		if (!string.IsNullOrEmpty(StatueType))
		{
			ParentObject.Render.DisplayName = Grammar.InitLower(Material) + " " + StatueType + " of " + creatureObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: false, Short: true, BaseOnly: true);
		}
		else
		{
			ParentObject.Render.DisplayName = Grammar.InitLower(Material) + " statue of " + creatureObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: false, Short: true, BaseOnly: true);
		}
		ParentObject.Render.RenderString = creatureObject.Render.RenderString;
		ParentObject.Render.Tile = creatureObject.Render.Tile;
		Description part = creatureObject.GetPart<Description>();
		Description part2 = ParentObject.GetPart<Description>();
		if (!string.IsNullOrEmpty(StatueType))
		{
			part2._Short = "This " + StatueType + " worked from " + Material + " intricately depicts " + creatureObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: false, Short: true, BaseOnly: true) + ":\n\n" + GameText.VariableReplace(part._Short, creatureObject);
		}
		else
		{
			part2._Short = "This statue worked from " + Material + " intricately depicts " + creatureObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: false, Short: true, BaseOnly: true) + ":\n\n" + GameText.VariableReplace(part._Short, creatureObject);
		}
		if (creatureObject.HasPart<Lovely>())
		{
			ParentObject.RequirePart<Lovely>();
		}
		else
		{
			ParentObject.RemovePart<Lovely>();
		}
	}
}
