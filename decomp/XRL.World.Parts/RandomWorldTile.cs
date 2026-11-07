using System;

namespace XRL.World.Parts;

[Serializable]
public class RandomWorldTile : IPart
{
	public string Tiles;

	public string DoubleTiles;

	public int DoubleChance = 35;

	[NonSerialized]
	private Random _Rnd;

	public Random DoubleSeed => _Rnd ?? (_Rnd = new Random((ParentObject.CurrentCell.X / 2) ^ ParentObject.CurrentCell.Y ^ The.Game.GetWorldSeed()));

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ZoneBuiltEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		RandomizeTile();
		return base.HandleEvent(E);
	}

	public void RandomizeTile()
	{
		if ((!IsDoubleTile() || !SetDoubleTile()) && !Tiles.IsNullOrEmpty())
		{
			ParentObject.Render.Tile = PickRandomTile.RandomVariant(Tiles.GetRandomSubstring(','));
		}
	}

	public bool IsDoubleTile()
	{
		if (DoubleTiles.IsNullOrEmpty())
		{
			return false;
		}
		if (DoubleChance <= 0)
		{
			return false;
		}
		if (ParentObject.CurrentCell == null)
		{
			return false;
		}
		return DoubleSeed.NextDouble() <= (double)DoubleChance / 100.0;
	}

	public bool IsDoubleSibling(GameObject Object)
	{
		if (Object.TryGetPart<RandomWorldTile>(out var Part) && Part.DoubleChance == DoubleChance)
		{
			return Part.DoubleTiles == DoubleTiles;
		}
		return false;
	}

	public bool SetDoubleTile()
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			return false;
		}
		bool flag = cell.X % 2 == 0;
		GameObject gameObject = cell.GetCellFromDirection(flag ? "E" : "W")?.GetFirstObject(IsDoubleSibling);
		if (gameObject == null)
		{
			return false;
		}
		Render render = ParentObject.Render;
		string randomSubstring = DoubleTiles.GetRandomSubstring(',', Trim: false, DoubleSeed);
		randomSubstring = PickRandomTile.RandomVariant(randomSubstring, DoubleSeed);
		randomSubstring = randomSubstring.Replace("[DIR]", (flag != render.HFlip) ? "left" : "right");
		render.Tile = randomSubstring;
		if (!flag)
		{
			gameObject.Render.ColorString = render.ColorString;
			gameObject.Render.DetailColor = render.DetailColor;
		}
		return true;
	}
}
