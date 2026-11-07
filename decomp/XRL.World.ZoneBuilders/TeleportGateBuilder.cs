using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class TeleportGateBuilder
{
	public string Blueprint;

	public GlobalLocation InboundTarget;

	public GlobalLocation ReturnTarget;

	public string TwinID;

	public bool BuildZone(Zone Z)
	{
		if (!Blueprint.IsNullOrEmpty())
		{
			Cell cell = null;
			if (InboundTarget == null || InboundTarget.ZoneID != Z.ZoneID || InboundTarget.CellX == -1 || InboundTarget.CellY == -1)
			{
				cell = Z.GetEmptyCells().GetRandomElement();
				GameObject gameObject = GameObject.FindByID(TwinID);
				if (gameObject != null && gameObject.TryGetPart<TeleportGate>(out var Part))
				{
					if (Part.Target == null)
					{
						Part.Target = new GlobalLocation(cell);
					}
					else
					{
						Part.Target.CellX = cell.X;
						Part.Target.CellY = cell.Y;
					}
				}
			}
			else
			{
				cell = Z.GetCell(InboundTarget.CellX, InboundTarget.CellY);
			}
			Cell cell2 = ((cell != null) ? cell.GetEmptyConnectedAdjacentCells(1).GetRandomElement() : null) ?? ((cell != null) ? cell.GetEmptyConnectedAdjacentCells(2).GetRandomElement() : null);
			if (cell2 == null)
			{
				cell2 = Z.GetEmptyCells().GetRandomElement();
			}
			if (cell2 != null)
			{
				GameObject gameObject2 = GameObject.Create(Blueprint);
				if (gameObject2 != null)
				{
					if (ReturnTarget != null)
					{
						TeleportGate part = gameObject2.GetPart<TeleportGate>();
						if (part != null)
						{
							part.Target = ReturnTarget;
						}
					}
					cell2.AddObject(gameObject2);
				}
			}
		}
		return true;
	}
}
