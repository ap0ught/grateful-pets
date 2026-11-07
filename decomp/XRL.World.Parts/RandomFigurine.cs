using System;
using System.Collections.Generic;
using Qud.API;
using UnityEngine;

namespace XRL.World.Parts;

[Serializable]
public class RandomFigurine : IPart
{
	public string Material = "copper";

	public string Creature;

	public bool IncludeCreatureDescription = true;

	public bool IncludeCreatureColors;

	public static readonly int DEFAULT_REP_VALUE = 60;

	public static readonly int COPPER_REP_VALUE = 60;

	public static readonly int SILVER_REP_VALUE = 100;

	public static readonly int GOLD_REP_VALUE = 200;

	public static readonly int AGATE_REP_VALUE = 100;

	public static readonly int TOPAZ_REP_VALUE = 150;

	public static readonly int JASPER_REP_VALUE = 200;

	public static readonly int AMETHYST_REP_VALUE = 250;

	public static readonly int SAPPHIRE_REP_VALUE = 300;

	public static readonly int EMERALD_REP_VALUE = 350;

	public static readonly int PERIDOT_REP_VALUE = 350;

	public RandomFigurine()
	{
	}

	public RandomFigurine(string Creature)
		: this()
	{
		this.Creature = Creature;
	}

	public override bool SameAs(IPart p)
	{
		RandomFigurine randomFigurine = p as RandomFigurine;
		if (randomFigurine.Material != Material)
		{
			return false;
		}
		if (randomFigurine.Creature != Creature)
		{
			return false;
		}
		if (randomFigurine.IncludeCreatureDescription != IncludeCreatureDescription)
		{
			return false;
		}
		if (randomFigurine.IncludeCreatureColors != IncludeCreatureColors)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool CanGenerateStacked()
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
		GameObject gameObject = ((Creature == null) ? EncountersAPI.GetASampleCreature((GameObjectBlueprint ob) => !ob.HasTag("NoFigurine")) : GameObjectFactory.Factory.CreateSampleObject(Creature));
		Creature = gameObject.Blueprint;
		ParentObject.Render.DisplayName = ParentObject.Render.DisplayName.Replace("*creature*", gameObject.DisplayNameOnlyStripped);
		Description part = gameObject.GetPart<Description>();
		Description part2 = ParentObject.GetPart<Description>();
		if (gameObject.HasPart<Lovely>() && !ParentObject.HasPart<Lovely>())
		{
			ParentObject.AddPart(new Lovely());
		}
		if (gameObject.Brain == null)
		{
			Debug.LogWarning("had brainless creature " + gameObject.Blueprint);
		}
		else
		{
			string text = "";
			foreach (var (text3, _) in gameObject.Brain.Allegiance)
			{
				text = text + text3 + ",";
			}
			text = text.TrimEnd(',');
			AddsRep.AddModifier(ParentObject, text, GetFigurineRepValue(Material));
		}
		part2._Short = part2._Short.Replace("*material*", Material).Replace("*creature.a*", gameObject.a).Replace("*creature.an*", gameObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true))
			.Replace("*creature*", gameObject.DisplayNameOnlyStripped) + (IncludeCreatureDescription ? GameText.VariableReplace(part._Short, gameObject) : "");
		if (IncludeCreatureColors)
		{
			string foregroundColor = gameObject.GetForegroundColor();
			string detailColor = gameObject.GetDetailColor();
			if (foregroundColor == detailColor)
			{
				ParentObject.DisplayName = "{{" + foregroundColor + "|" + ParentObject.DisplayNameOnlyDirect + "}}";
			}
			else
			{
				ParentObject.DisplayName = "{{" + foregroundColor + "-" + foregroundColor + "-" + foregroundColor + "-" + detailColor + " sequence|" + ParentObject.DisplayNameOnlyDirect + "}}";
			}
			ParentObject.SetForegroundColor(foregroundColor);
			ParentObject.SetDetailColor(detailColor);
		}
		return base.HandleEvent(E);
	}

	public static int GetFigurineRepValue(string material)
	{
		return material switch
		{
			"copper" => COPPER_REP_VALUE, 
			"silver" => SILVER_REP_VALUE, 
			"gold" => GOLD_REP_VALUE, 
			"agate" => AGATE_REP_VALUE, 
			"topaz" => TOPAZ_REP_VALUE, 
			"jasper" => JASPER_REP_VALUE, 
			"amethyst" => AMETHYST_REP_VALUE, 
			"sapphire" => SAPPHIRE_REP_VALUE, 
			"emerald" => EMERALD_REP_VALUE, 
			"peridot" => PERIDOT_REP_VALUE, 
			_ => DEFAULT_REP_VALUE, 
		};
	}
}
