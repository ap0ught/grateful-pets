using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class TransfigureOnStatus : IPart
{
	public string BrokenTile;

	public string BrokenRenderString;

	public string BrokenColorString;

	public string BrokenTileColor;

	public string BrokenDetailColor;

	public string RustedTile;

	public string RustedRenderString;

	public string RustedColorString;

	public string RustedTileColor;

	public string RustedDetailColor;

	public string LastTile;

	public string LastRenderString;

	public string LastColorString;

	public string LastTileColor;

	public string LastDetailColor;

	public int State;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EffectAppliedEvent.ID)
		{
			return ID == EffectRemovedEvent.ID;
		}
		return true;
	}

	public void SaveRender()
	{
		if (State == 0)
		{
			Render render = ParentObject.Render;
			LastTile = render.Tile;
			LastRenderString = render.RenderString;
			LastColorString = render.ColorString;
			LastTileColor = render.TileColor;
			LastDetailColor = render.DetailColor;
		}
	}

	public void CheckState()
	{
		Render render = ParentObject.Render;
		if (ParentObject.HasEffect(typeof(Broken)))
		{
			if (State != 2)
			{
				SaveRender();
				render.Tile = BrokenTile.GetRandomSubstring(',').Coalesce(render.Tile);
				render.RenderString = BrokenRenderString.GetRandomSubstring(',').Coalesce(render.RenderString);
				render.ColorString = BrokenColorString.GetRandomSubstring(',').Coalesce(render.ColorString);
				render.TileColor = BrokenTileColor.GetRandomSubstring(',').Coalesce(render.TileColor);
				render.DetailColor = BrokenDetailColor.GetRandomSubstring(',').Coalesce(render.DetailColor);
				State = 2;
			}
		}
		else if (ParentObject.HasEffect(typeof(Rusted)))
		{
			if (State != 1)
			{
				SaveRender();
				render.Tile = RustedTile.GetRandomSubstring(',').Coalesce(render.Tile);
				render.RenderString = RustedRenderString.GetRandomSubstring(',').Coalesce(render.RenderString);
				render.ColorString = RustedColorString.GetRandomSubstring(',').Coalesce(render.ColorString);
				render.TileColor = RustedTileColor.GetRandomSubstring(',').Coalesce(render.TileColor);
				render.DetailColor = RustedDetailColor.GetRandomSubstring(',').Coalesce(render.DetailColor);
				State = 1;
			}
		}
		else
		{
			render.Tile = LastTile;
			render.RenderString = LastRenderString;
			render.ColorString = LastColorString;
			render.TileColor = LastTileColor;
			render.DetailColor = LastDetailColor;
			State = 0;
		}
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		Effect effect = E.Effect;
		if (effect is Broken || effect is Rusted)
		{
			CheckState();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		Effect effect = E.Effect;
		if (effect is Broken || effect is Rusted)
		{
			CheckState();
		}
		return base.HandleEvent(E);
	}
}
