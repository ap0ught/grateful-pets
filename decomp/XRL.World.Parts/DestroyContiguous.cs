using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class DestroyContiguous : IPart
{
	public string TargetPart;

	public string TargetTag;

	public string TargetTagValue;

	public int Chance = 100;

	public int ChanceDegradation;

	public override bool SameAs(IPart p)
	{
		DestroyContiguous destroyContiguous = p as DestroyContiguous;
		if (destroyContiguous.TargetPart != TargetPart)
		{
			return false;
		}
		if (destroyContiguous.TargetTag != TargetTag)
		{
			return false;
		}
		if (destroyContiguous.TargetTagValue != TargetTagValue)
		{
			return false;
		}
		if (destroyContiguous.Chance != Chance)
		{
			return false;
		}
		if (destroyContiguous.ChanceDegradation != ChanceDegradation)
		{
			return false;
		}
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		if (Object.HasTagOrProperty("MeleeWeapon"))
		{
			Registrar.Register("WeaponHit");
		}
		else
		{
			Registrar.Register("ProjectileEntering");
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ProjectileEntering")
		{
			if (E.GetParameter("Cell") is Cell cell)
			{
				ChainDestroy(cell, Chance, cell, E.GetGameObjectParameter("Attacker"), ParentObject.GetPart<Projectile>()?.Launcher, ParentObject);
			}
		}
		else if (E.ID == "WeaponHit")
		{
			Cell cell2 = E.GetGameObjectParameter("Defender")?.CurrentCell;
			if (cell2 != null)
			{
				ChainDestroy(cell2, Chance, cell2, E.GetGameObjectParameter("Attacker"), E.GetGameObjectParameter("Weapon"));
			}
		}
		return base.FireEvent(E);
	}

	public void ChainDestroy(Cell C, int UseChance, Cell OC, GameObject Actor, GameObject Weapon, GameObject Projectile = null)
	{
		List<GameObject> list = Event.NewGameObjectList();
		list.AddRange(C.Objects);
		bool flag = false;
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			GameObject gameObject = list[i];
			if (!Match(gameObject))
			{
				continue;
			}
			GameObject subject = gameObject;
			int chance = GetSpecialEffectChanceEvent.GetFor(Actor, Weapon, "Part DestroyContiguous Activation", UseChance, subject, Projectile);
			int j = 0;
			for (int count2 = gameObject.Count; j < count2; j++)
			{
				if (chance.in100())
				{
					if (gameObject.IsPlayer() || gameObject.HasTag("Creature"))
					{
						gameObject.Die(null, "annihilation", "You were cancelled from existence.", gameObject.It + gameObject.GetVerb("were") + " @@cancelled from existence.", C != OC);
					}
					else
					{
						gameObject.Destroy();
					}
					flag = true;
				}
			}
		}
		if (flag)
		{
			List<Cell> adjacentCells = C.GetAdjacentCells();
			int k = 0;
			for (int count3 = adjacentCells.Count; k < count3; k++)
			{
				ChainDestroy(adjacentCells[k], UseChance - ChanceDegradation, OC, Actor, Weapon, Projectile);
			}
		}
	}

	public bool Match(GameObject GO)
	{
		if (GO.IsInvalid())
		{
			return false;
		}
		if (TargetPart != null && GO.HasPart(TargetPart))
		{
			return true;
		}
		if (TargetTag != null && GO.HasTag(TargetTag) && (TargetTagValue == null || GO.GetTag(TargetTag) == TargetTagValue))
		{
			return true;
		}
		return false;
	}
}
