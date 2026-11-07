using System;
using System.Linq;
using XRL.Rules;

namespace XRL.World.Parts;

/// When parent object takes a total of <c>DamagePer</c> <c>DamageTriggerTypes</c> over its lifetime
/// it has a <c>Chance</c> in 100 chance of creating and placing <c>Number</c> copies of <c>Blueprint</c>.
[Serializable]
public class BurnOffGas : IPart
{
	/// Total damage taken (will never exceed <c>DamagePer</c>).
	public int DamageTaken;

	/// Amount of damage needed to trigger
	public int DamagePer = 10;

	/// Chance in 100 to trigger
	public int Chance = 100;

	/// Number of things to spawn. Accepts roll formula i.e. "1d3-1" could spawn 0, 1, 2 things.
	public string Number = "1";

	/// Damage tags that will count toward counter
	public string DamageTriggerTypes = "Heat;Fire";

	/// Blueprint to spawn.  If first character is an '@' it rolls an item from the population table
	public string Blueprint;

	/// If using a population <c>Blueprint</c> this will "save" the first roll and not roll again when true.
	public bool PopulationRollsAreStatic = true;

	/// Unused???
	public bool SpawnAsDropColor = true;

	public override bool SameAs(IPart p)
	{
		BurnOffGas burnOffGas = p as BurnOffGas;
		if (burnOffGas.DamagePer != DamagePer)
		{
			return false;
		}
		if (burnOffGas.Chance != Chance)
		{
			return false;
		}
		if (burnOffGas.Number != Number)
		{
			return false;
		}
		if (burnOffGas.Blueprint != Blueprint)
		{
			return false;
		}
		if (burnOffGas.PopulationRollsAreStatic != PopulationRollsAreStatic)
		{
			return false;
		}
		if (burnOffGas.SpawnAsDropColor != SpawnAsDropColor)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeforeTookDamage");
		base.Register(Object, Registrar);
	}

	/// Handles:
	///   "BeforeTookDamage" - checks parameter Damage.Attributes for type match, increases counter, and does spawn logic.
	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeTookDamage")
		{
			Physics physics = ParentObject.Physics;
			if (physics == null || physics.CurrentCell == null)
			{
				return true;
			}
			if (!E.GetParameter<Damage>("Damage").Attributes.Any((string s) => DamageTriggerTypes.Contains(s)))
			{
				return true;
			}
			DamageTaken += E.GetParameter<Damage>("Damage").Amount;
			while (DamageTaken >= DamagePer)
			{
				DamageTaken -= DamagePer;
				if (!Chance.in100() || Blueprint.IsNullOrEmpty() || ParentObject.CurrentCell == null)
				{
					continue;
				}
				int num = 0;
				for (int num2 = Stat.Roll(Number); num < num2; num++)
				{
					string blueprint = Blueprint;
					if (blueprint.StartsWith("@"))
					{
						blueprint = PopulationManager.RollOneFrom(blueprint.Substring(1)).Blueprint;
						if (PopulationRollsAreStatic)
						{
							Blueprint = blueprint;
						}
					}
					GameObject gameObject = GameObject.Create(blueprint);
					ParentObject.CurrentCell.AddObject(gameObject);
					if (ParentObject.IsVisible())
					{
						IComponent<GameObject>.XDidY(ParentObject, "burn", "off " + gameObject.an());
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
