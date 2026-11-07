using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class BethesdaColdZone
{
	public bool BuildZone(Zone Z)
	{
		Z.BaseTemperature = 25 - (Z.Z - 10) * 9;
		for (int i = 0; i < Z.Height; i++)
		{
			for (int j = 0; j < Z.Width; j++)
			{
				Cell cell = Z.Map[j][i];
				BlueTile.PaintCell(cell);
				int k = 0;
				for (int count = cell.Objects.Count; k < count; k++)
				{
					GameObject gameObject = cell.Objects[k];
					if (gameObject.Physics != null)
					{
						if (gameObject.HasProperty("StartFrozen"))
						{
							gameObject.Physics._Temperature = gameObject.Physics.BrittleTemperature - 30;
						}
						else if (gameObject.Stat("ColdResistance") < 100 || gameObject.IsScenery || gameObject.IsWall())
						{
							gameObject.Physics._Temperature = Z.BaseTemperature;
						}
					}
				}
			}
		}
		return true;
	}
}
