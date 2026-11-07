using System;
using Qud.API;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class Disguised : Effect, ITierInitialized
{
	public string BlueprintName;

	public string Tile;

	public string RenderString;

	public string ColorString;

	public string TileColor;

	public string DetailColor;

	public string Appearance;

	public Disguised()
	{
		Duration = 9999;
		DisplayName = "{{K|disguised}}";
	}

	public Disguised(string BlueprintName)
		: this()
	{
		this.BlueprintName = BlueprintName;
		GameObjectBlueprint blueprintIfExists = GameObjectFactory.Factory.GetBlueprintIfExists(BlueprintName);
		if (blueprintIfExists == null)
		{
			throw new Exception("no such blueprint " + BlueprintName);
		}
		string partParameter = blueprintIfExists.GetPartParameter<string>("RandomTile", "Tiles");
		if (!partParameter.IsNullOrEmpty())
		{
			Tile = partParameter.CachedCommaExpansion().GetRandomElement();
		}
		else
		{
			Tile = blueprintIfExists.GetPartParameter<string>("Render", "Tile");
		}
		RenderString = XRL.World.Parts.Render.ProcessRenderString(blueprintIfExists.GetPartParameter<string>("Render", "RenderString"));
		ColorString = blueprintIfExists.GetPartParameter<string>("Render", "ColorString");
		TileColor = blueprintIfExists.GetPartParameter("Render", "TileColor", ColorString);
		DetailColor = blueprintIfExists.GetPartParameter<string>("Render", "DetailColor");
	}

	public Disguised(string BlueprintName, string Appearance)
		: this(BlueprintName)
	{
		this.Appearance = Appearance;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(2400, 7200);
		BlueprintName = EncountersAPI.GetACreatureBlueprintModel(ModDisguise.IsBlueprintUsableForDisguise).Name;
		GameObjectBlueprint blueprintIfExists = GameObjectFactory.Factory.GetBlueprintIfExists(BlueprintName);
		if (blueprintIfExists == null)
		{
			throw new Exception("no such blueprint " + BlueprintName);
		}
		string partParameter = blueprintIfExists.GetPartParameter<string>("RandomTile", "Tiles");
		if (!partParameter.IsNullOrEmpty())
		{
			Tile = partParameter.CachedCommaExpansion().GetRandomElement();
		}
		else
		{
			Tile = blueprintIfExists.GetPartParameter<string>("Render", "Tile");
		}
		RenderString = XRL.World.Parts.Render.ProcessRenderString(blueprintIfExists.GetPartParameter<string>("Render", "RenderString"));
		ColorString = blueprintIfExists.GetPartParameter<string>("Render", "ColorString");
		TileColor = blueprintIfExists.GetPartParameter("Render", "TileColor", ColorString);
		DetailColor = blueprintIfExists.GetPartParameter<string>("Render", "DetailColor");
		string partParameter2 = blueprintIfExists.GetPartParameter("Render", "DisplayName", "creature of some kind");
		string text = blueprintIfExists.GetxTag("Grammar", "iArticle");
		text = GetPropertyOrTag("OverrideIArticle", text);
		if (text == null)
		{
			Appearance = Grammar.A(partParameter2);
		}
		else
		{
			Appearance = text + " " + partParameter2;
		}
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 131072;
	}

	public override string GetDetails()
	{
		return "Has the appearance of " + (Appearance ?? "another creature") + ".";
	}

	public override bool Apply(GameObject Object)
	{
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		bool useTiles = Options.UseTiles;
		if (useTiles && !Tile.IsNullOrEmpty())
		{
			E.Tile = Tile;
		}
		if (!RenderString.IsNullOrEmpty())
		{
			E.RenderString = RenderString;
		}
		string text = (((!TileColor.IsNullOrEmpty() && !Tile.IsNullOrEmpty()) & useTiles) ? TileColor : ColorString);
		if (!text.IsNullOrEmpty())
		{
			E.ColorString = text;
		}
		if (!DetailColor.IsNullOrEmpty())
		{
			E.DetailColor = DetailColor;
		}
		return base.Render(E);
	}

	public override bool OverlayRender(RenderEvent E)
	{
		if (Options.UseTiles && !Tile.IsNullOrEmpty())
		{
			E.Tile = Tile;
		}
		if (!RenderString.IsNullOrEmpty())
		{
			E.RenderString = RenderString;
		}
		return base.OverlayRender(E);
	}
}
