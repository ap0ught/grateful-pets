using System;
using XRL.World.Tinkering;

namespace XRL.World.Parts;

[Serializable]
public class ExtradimensionalLoot : IPart
{
	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeforeDeathRemoval");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDeathRemoval")
		{
			try
			{
				Cell cell = ParentObject.GetCurrentCell();
				if (cell != null && 5.in100())
				{
					GameObject mostValuableItem = ParentObject.GetMostValuableItem();
					if (mostValuableItem != null)
					{
						mostValuableItem.SplitStack(1, ParentObject);
						mostValuableItem.RemovePart<Temporary>();
						mostValuableItem.RemovePart<ExistenceSupport>();
						if (!mostValuableItem.HasPart<ModExtradimensional>())
						{
							Extradimensional part = ParentObject.GetPart<Extradimensional>();
							ItemModding.ApplyModification(mostValuableItem, new ModExtradimensional(part.WeaponModIndex, part.MissileWeaponModIndex, part.ArmorModIndex, part.ShieldModIndex, part.MiscModIndex, part.Training, part.DimensionName, part.SecretID));
							mostValuableItem.SetStringProperty("NeverStack", "1");
						}
						if (Visible())
						{
							IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("drop") + " " + mostValuableItem.an() + ", and by sheer chance " + mostValuableItem.does("quantum tunnel", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: true) + " and fully" + mostValuableItem.GetVerb("materialize") + " in this dimension.");
						}
						cell.AddObject(mostValuableItem);
					}
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogException("PsychicHunter loot", x);
			}
			return true;
		}
		return true;
	}
}
