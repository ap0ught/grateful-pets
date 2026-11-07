using System;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: gas density produced is increased by a percentage
/// equal to ((power load - 100) / 10), i.e. 30% for the standard overload
/// power load of 400.
/// </remarks>
[Serializable]
public class GasOnEntering : IPart
{
	public string Blueprint = "AcidGas";

	public string Density = "3d10";

	public override bool SameAs(IPart p)
	{
		GasOnEntering gasOnEntering = p as GasOnEntering;
		if (gasOnEntering.Blueprint != Blueprint)
		{
			return false;
		}
		if (gasOnEntering.Density != Density)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ProjectileEntering");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ProjectileEntering" && E.GetParameter("Cell") is Cell cell)
		{
			GameObject gameObject = GameObject.Create(Blueprint);
			Gas part = gameObject.GetPart<Gas>();
			part.Density = Density.RollCached() * (100 + MyPowerLoadBonus(int.MinValue, 100, 10)) / 100;
			part.Creator = E.GetGameObjectParameter("Attacker");
			cell.AddObject(gameObject);
		}
		return base.FireEvent(E);
	}
}
