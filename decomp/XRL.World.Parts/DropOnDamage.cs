using System;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class DropOnDamage : IPart
{
	public int MinimumDamage;

	public int Chance = 100;

	public string Number = "1";

	public string Blueprint;

	public bool PopulationRollsAreStatic = true;

	public bool SpawnAsDropColor = true;

	public override bool SameAs(IPart p)
	{
		DropOnDamage dropOnDamage = p as DropOnDamage;
		if (dropOnDamage.MinimumDamage != MinimumDamage)
		{
			return false;
		}
		if (dropOnDamage.Chance != Chance)
		{
			return false;
		}
		if (dropOnDamage.Number != Number)
		{
			return false;
		}
		if (dropOnDamage.Blueprint != Blueprint)
		{
			return false;
		}
		if (dropOnDamage.PopulationRollsAreStatic != PopulationRollsAreStatic)
		{
			return false;
		}
		if (dropOnDamage.SpawnAsDropColor != SpawnAsDropColor)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeforeTookDamage");
		Registrar.Register("EnteredCell");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			if (SpawnAsDropColor)
			{
				SpawnAsDropColor = false;
				string blueprint = Blueprint;
				if (blueprint.Length > 0 && blueprint[0] == '@')
				{
					blueprint = PopulationManager.RollOneFrom(Blueprint.Substring(1)).Blueprint;
				}
				if (PopulationRollsAreStatic)
				{
					Blueprint = blueprint;
				}
				GameObject gameObject = GameObjectFactory.create(Blueprint);
				ParentObject.Render.DetailColor = gameObject.Render.GetForegroundColor();
			}
		}
		else if (E.ID == "BeforeTookDamage")
		{
			Cell cell = ParentObject.CurrentCell;
			if (cell == null)
			{
				return true;
			}
			if (MinimumDamage > 0 && E.GetParameter<Damage>("Damage").Amount < MinimumDamage)
			{
				return true;
			}
			if (Chance.in100() && !string.IsNullOrEmpty(Blueprint))
			{
				int i = 0;
				for (int num = Number.RollCached(); i < num; i++)
				{
					string blueprint2 = Blueprint;
					if (blueprint2.StartsWith("@"))
					{
						blueprint2 = PopulationManager.RollOneFrom(blueprint2.Substring(1)).Blueprint;
						if (PopulationRollsAreStatic)
						{
							Blueprint = blueprint2;
						}
					}
					GameObject gameObject2 = GameObject.Create(blueprint2);
					Temporary.CarryOver(ParentObject, gameObject2);
					Phase.carryOver(ParentObject, gameObject2);
					cell.AddObject(gameObject2);
					DidXToY("drop", gameObject2, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true);
				}
			}
		}
		return base.FireEvent(E);
	}
}
