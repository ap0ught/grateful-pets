using System;

namespace XRL.World.Parts;

[Serializable]
public class RecorporealizationBoothSpawner : IPart
{
	public string Group = "A";

	public int Period = 1;

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EnteredCell");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			Cell parameter = E.GetParameter<Cell>("Cell");
			new RecorporealizationBoothSpawnerBuilder().BuildZone(parameter.ParentZone);
			ParentObject.Destroy();
		}
		return base.FireEvent(E);
	}
}
