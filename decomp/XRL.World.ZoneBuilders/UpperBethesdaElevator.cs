using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class UpperBethesdaElevator
{
	public bool BuildZone(Zone Z)
	{
		Box b = new Box(27, 9, 36, 15);
		int num = 30;
		int num2 = 12;
		Z.ClearBox(b);
		Z.GetCell(num, num2).AddObject("OpenShaft");
		GameObject gameObject = GameObjectFactory.Factory.CreateObject("ElevatorSwitch");
		ElevatorSwitch part = gameObject.GetPart<ElevatorSwitch>();
		part.TopLevel = 15;
		part.FloorLevel = 21;
		Z.GetCell(num, num2 - 1).AddObject(gameObject);
		if (Z.Z == 15)
		{
			Z.GetCell(num, num2).AddObject("Platform");
		}
		Z.BuildReachableMap(num, num2);
		return true;
	}
}
