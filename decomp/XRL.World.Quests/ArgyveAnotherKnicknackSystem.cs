using System;

namespace XRL.World.Quests;

[Serializable]
public class ArgyveAnotherKnicknackSystem : ArgyveKnicknackSystem
{
	public override string StepID => "Find Another Knickknack";

	public override void Finish()
	{
		base.Finish();
		Zone zone = The.ZoneManager.GetZone("JoppaWorld.11.22.1.1.10");
		for (int i = 18; i <= 20; i++)
		{
			for (int j = 4; j <= 11; j++)
			{
				Cell.ObjectRack objects = zone.Map[j][i].Objects;
				for (int num = objects.Count - 1; num >= 0; num--)
				{
					GameObject gameObject = objects[num];
					if (gameObject.Physics?.Owner == "Joppa")
					{
						gameObject.Physics.Owner = null;
					}
				}
			}
		}
	}
}
