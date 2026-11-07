namespace XRL.World.Parts;

public class MakClamSpawner : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeObjectCreatedEvent E)
	{
		E.ReplacementObject = GameObject.Create("Giant Clam");
		E.ReplacementObject.SetIntProperty("MakClam", 1);
		return base.HandleEvent(E);
	}
}
