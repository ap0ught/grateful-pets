using System;
using ConsoleLib.Console;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class DrawInTheDark : IPart
{
	public string ForegroundTileColor = "";

	public string BackgroundTileColor = "";

	public string DetailColor = "";

	public string BackgroundColor = "";

	public string ForegroundColor = "";

	public override bool Render(RenderEvent E)
	{
		if (ParentObject.Physics.CurrentCell != null && ParentObject.Physics.currentCell.IsExplored() && !ParentObject.Physics.currentCell.IsVisible())
		{
			E.WantsToPaint = true;
		}
		return true;
	}

	public override void OnPaint(ScreenBuffer buffer)
	{
		if (ParentObject.Physics.currentCell == null || !ParentObject.Physics.currentCell.IsExplored() || ParentObject.Physics.currentCell.IsVisible())
		{
			return;
		}
		int x = ParentObject.Physics.currentCell.X;
		int y = ParentObject.Physics.currentCell.Y;
		if (Options.UseTiles && !string.IsNullOrEmpty(ParentObject.Render.Tile))
		{
			if (!string.IsNullOrEmpty(BackgroundTileColor))
			{
				buffer.Buffer[x, y].SetBackground(BackgroundTileColor[0]);
			}
			else if (!string.IsNullOrEmpty(BackgroundColor))
			{
				buffer.Buffer[x, y].SetBackground(BackgroundColor[0]);
			}
			if (!string.IsNullOrEmpty(ForegroundTileColor))
			{
				buffer.Buffer[x, y].SetForeground(ForegroundTileColor[0]);
			}
			else if (!string.IsNullOrEmpty(ForegroundColor))
			{
				buffer.Buffer[x, y].SetForeground(ForegroundColor[0]);
			}
			if (!string.IsNullOrEmpty(DetailColor))
			{
				buffer.Buffer[x, y].Detail = ColorUtility.ColorMap[DetailColor[0]];
			}
		}
		else
		{
			if (!string.IsNullOrEmpty(BackgroundColor))
			{
				buffer.Buffer[x, y].SetBackground(BackgroundColor[0]);
			}
			if (!string.IsNullOrEmpty(ForegroundColor))
			{
				buffer.Buffer[x, y].SetForeground(ForegroundColor[0]);
			}
		}
	}
}
