using System;
using ConsoleLib.Console;
using XRL.Core;

namespace XRL.World.Parts;

[Serializable]
public class AlarmCircle : IPart
{
	public int Radius = 3;

	public override void Attach()
	{
		ParentObject.Flags |= 128;
	}

	public override bool Render(RenderEvent E)
	{
		if (ParentObject.CurrentCell != null)
		{
			E.WantsToPaint = true;
		}
		return true;
	}

	public override void OnPaint(ScreenBuffer buffer)
	{
		if (Globals.RenderMode != RenderModeType.Text || XRLCore.GetCurrentFrameAtFPS(60) % 120 >= 60)
		{
			return;
		}
		foreach (Cell localAdjacentCell in ParentObject.CurrentCell.GetLocalAdjacentCells(Radius))
		{
			if (localAdjacentCell.Location.Distance(ParentObject.CurrentCell.Location) == Radius)
			{
				buffer.Buffer[localAdjacentCell.X, localAdjacentCell.Y].SetBackground('R');
				buffer.Buffer[localAdjacentCell.X, localAdjacentCell.Y].Detail = The.Color.Red;
			}
		}
	}
}
