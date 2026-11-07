using System;
using ConsoleLib.Console;

namespace XRL.World.Parts;

[Serializable]
public class PaintTest : IPart
{
	public override bool Render(RenderEvent E)
	{
		if (ParentObject.Physics.CurrentCell != null)
		{
			E.WantsToPaint = true;
		}
		return true;
	}

	public override void OnPaint(ScreenBuffer buffer)
	{
		int num = (int)(IComponent<GameObject>.frameTimerMS % 1000 / 200);
		foreach (Cell localAdjacentCell in ParentObject.Physics.CurrentCell.GetLocalAdjacentCells(num))
		{
			if (localAdjacentCell.Location.Distance(ParentObject.Physics.CurrentCell.Location) == num)
			{
				buffer.Buffer[localAdjacentCell.X, localAdjacentCell.Y].SetBackground('G');
				buffer.Buffer[localAdjacentCell.X, localAdjacentCell.Y].Detail = The.Color.Green;
			}
		}
	}
}
