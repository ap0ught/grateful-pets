using System;

namespace XRL.World.Parts;

[Serializable]
public class SpawnOnDeath : IPart
{
	public string Blueprint = "Bloatfly";

	public bool DoPuff = true;

	public string PuffColor = "&K";

	public SpawnOnDeath()
	{
	}

	public SpawnOnDeath(string blueprint)
	{
		Blueprint = blueprint;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeforeDeathRemoval");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDeathRemoval" && ParentObject.Physics.CurrentCell != null)
		{
			ParentObject.Physics.CurrentCell.AddObject(Blueprint);
			if (DoPuff)
			{
				ParentObject.DustPuff(PuffColor);
			}
		}
		return base.FireEvent(E);
	}
}
