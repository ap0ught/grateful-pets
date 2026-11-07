using System;
using XRL.Rules;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class HiddenRender : IPart
{
	public string DisplayName;

	public string RenderString;

	public string ColorString;

	public string Tile;

	public int RenderLayer;

	public new bool Visible = true;

	public string LiquidNamePreposition;

	public bool AddStairHighlight;

	public bool Found;

	public int Difficulty;

	public HiddenRender()
	{
		RenderString = "?";
		ColorString = "&y";
		Found = false;
	}

	public override bool SameAs(IPart p)
	{
		HiddenRender hiddenRender = p as HiddenRender;
		if (hiddenRender.DisplayName != DisplayName)
		{
			return false;
		}
		if (hiddenRender.RenderString != RenderString)
		{
			return false;
		}
		if (hiddenRender.ColorString != ColorString)
		{
			return false;
		}
		if (hiddenRender.Tile != Tile)
		{
			return false;
		}
		if (hiddenRender.RenderLayer != RenderLayer)
		{
			return false;
		}
		if (hiddenRender.Visible != Visible)
		{
			return false;
		}
		if (hiddenRender.LiquidNamePreposition != LiquidNamePreposition)
		{
			return false;
		}
		if (hiddenRender.Found != Found)
		{
			return false;
		}
		if (hiddenRender.Difficulty != Difficulty)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void Initialize()
	{
		base.Initialize();
		if (!Found)
		{
			if (ParentObject.Render.CustomRender && !ParentObject.HasIntProperty("CustomRenderSources"))
			{
				ParentObject.ModIntProperty("CustomRenderSources", 1);
			}
			ParentObject.Render.CustomRender = true;
			ParentObject.ModIntProperty("CustomRenderSources", 1);
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CheckTileChangeEvent>.ID && (ID != GetAdjacentNavigationWeightEvent.ID || Found))
		{
			if (ID == GetNavigationWeightEvent.ID)
			{
				return !Found;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		if (!Found)
		{
			E.Weight = E.PriorWeight;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetAdjacentNavigationWeightEvent E)
	{
		if (!Found)
		{
			E.Weight = E.PriorWeight;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CheckTileChangeEvent E)
	{
		if (Found)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CustomRender");
		Registrar.Register("Searched");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CustomRender")
		{
			if (!Found && E.GetParameter("RenderEvent") is RenderEvent renderEvent && (renderEvent.Lit == LightLevel.Radar || renderEvent.Lit == LightLevel.LitRadar))
			{
				Reveal();
			}
		}
		else if (E.ID == "Searched" && !Found)
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Searcher");
			int intParameter = E.GetIntParameter("Bonus");
			if (Stat.RollResult(gameObjectParameter.Stat("Intelligence")) + intParameter >= Difficulty)
			{
				Reveal();
			}
		}
		return base.FireEvent(E);
	}

	public new void Reveal()
	{
		if (Found)
		{
			return;
		}
		Found = true;
		ParentObject.ModIntProperty("CustomRenderSources", -1);
		if (ParentObject.GetIntProperty("CustomRenderSources") <= 0)
		{
			ParentObject.Render.CustomRender = false;
		}
		ParentObject.Render.DisplayName = DisplayName;
		ParentObject.Render.RenderString = RenderString;
		ParentObject.Render.ColorString = ColorString;
		ParentObject.Render.Tile = Tile;
		ParentObject.Render.RenderLayer = RenderLayer;
		ParentObject.Render.Visible = Visible;
		if (AddStairHighlight)
		{
			ParentObject.RequirePart<StairHighlight>();
		}
		if (!string.IsNullOrEmpty(LiquidNamePreposition))
		{
			LiquidVolume liquidVolume = ParentObject.LiquidVolume;
			if (liquidVolume != null)
			{
				liquidVolume.NamePreposition = LiquidNamePreposition;
			}
		}
		IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("are", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true) + " revealed " + The.Player.DescribeDirectionToward(ParentObject) + "!");
		if (AutoAct.IsInterruptable() && (!ParentObject.HasTag("Creature") || The.Player.IsRelevantHostile(ParentObject)))
		{
			AutoAct.Interrupt(null, null, ParentObject, IsThreat: true);
		}
		ParentObject.RemovePart<AnimatedMaterialWater>();
	}
}
