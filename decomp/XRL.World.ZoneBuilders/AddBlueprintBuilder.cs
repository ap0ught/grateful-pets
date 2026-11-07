using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class AddBlueprintBuilder
{
	public string Object;

	public bool BuildZone(Zone Z)
	{
		int num = 500;
		GameObject gameObject = GameObjectFactory.Factory.CreateObject(Object);
		if (gameObject != null)
		{
			while (num > 0)
			{
				int x = Stat.Random(0, Z.Width - 1);
				int y = Stat.Random(0, Z.Height - 1);
				if (Z.GetCell(x, y).IsEmpty())
				{
					Z.GetCell(x, y).AddObject(gameObject);
					return true;
				}
				num--;
			}
		}
		return true;
	}
}
