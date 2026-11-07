using System;
using XRL.Language;

namespace XRL.World.Parts;

[Serializable]
public class ZoneLandmarkGroup : IZoneLandmark
{
	public string DisplayName;

	public string Blueprint;

	public int Count;

	public int Padding;

	public bool Diagonal;

	[NonSerialized]
	private string DisplayNameCache;

	[NonSerialized]
	private long CacheTick = -2147483648L;

	[NonSerialized]
	private int X1;

	[NonSerialized]
	private int Y1;

	[NonSerialized]
	private int X2;

	[NonSerialized]
	private int Y2;

	[NonSerialized]
	private int Found;

	[NonSerialized]
	private Func<int, int, bool> ScanGet;

	[NonSerialized]
	private Action<int, int> ScanSet;

	public void CacheBounds()
	{
		long timeTicks = The.Game.TimeTicks;
		if (timeTicks - 100 <= CacheTick)
		{
			return;
		}
		CacheTick = timeTicks;
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			return;
		}
		X1 = (Y1 = int.MaxValue);
		X2 = (Y2 = int.MinValue);
		Found = 0;
		if (ScanGet == null)
		{
			ScanGet = (int x, int y) => ParentObject.CurrentZone.GetCell(x, y).HasObject(Blueprint);
		}
		if (ScanSet == null)
		{
			ScanSet = (int x, int y) =>
			{
				if (x < X1)
				{
					X1 = x;
				}
				if (y < Y1)
				{
					Y1 = y;
				}
				if (x > X2)
				{
					X2 = x;
				}
				if (y > Y2)
				{
					Y2 = y;
				}
				Found++;
			};
		}
		Tools.ScanFill(cell.X, cell.Y, cell.ParentZone.Width, cell.ParentZone.Height, ScanGet, ScanSet, Diagonal);
		if (Padding != 0)
		{
			X1 = Math.Max(X1 - Padding, 0);
			Y1 = Math.Max(Y1 - Padding, 0);
			X2 = Math.Min(X2 + Padding, cell.ParentZone.Width - 1);
			Y2 = Math.Min(Y2 + Padding, cell.ParentZone.Height - 1);
		}
	}

	public override string GetDisplayName()
	{
		if (!DisplayName.IsNullOrEmpty())
		{
			return DisplayName;
		}
		if (DisplayNameCache == null)
		{
			DisplayNameCache = GameObjectFactory.Factory.GetBlueprint(Blueprint).CachedDisplayNameStripped;
			if (Found > 1 || Count > 1)
			{
				DisplayNameCache = Grammar.Pluralize(DisplayNameCache);
			}
		}
		return DisplayNameCache;
	}

	public override bool IsApplicable(int X, int Y)
	{
		CacheBounds();
		if (Found < Count)
		{
			return false;
		}
		if (X < X1)
		{
			return false;
		}
		if (X > X2)
		{
			return false;
		}
		if (Y < Y1)
		{
			return false;
		}
		if (Y > Y2)
		{
			return false;
		}
		return true;
	}
}
