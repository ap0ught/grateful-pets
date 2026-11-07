using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.AI;
using XRL.World.Anatomy;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class RustOnHit : IPart
{
	public int Chance = 15;

	public string Amount = "1";

	public string PreferPartType;

	public bool AffectInventory = true;

	public bool AffectEquipment = true;

	protected GameObject GetRandomItemFrom(GameObject Object, bool AffectInventory, bool AffectEquipment)
	{
		bool flag = AffectInventory && Object.Inventory?.Objects != null && Object.Inventory.Objects.Count > 0;
		bool flag2 = AffectEquipment && Object.Body != null && Object.Body.GetBody().GetEquippedObjectCount() > 0;
		if (!flag && !flag2)
		{
			return null;
		}
		if (flag2 && !PreferPartType.IsNullOrEmpty())
		{
			List<GameObject> list = Event.NewGameObjectList();
			foreach (BodyPart item in Object.Body.LoopPart(PreferPartType))
			{
				if (item.Equipped != null)
				{
					list.Add(item.Equipped);
				}
			}
			if (!list.IsNullOrEmpty())
			{
				return list.GetRandomElement();
			}
		}
		if (flag & flag2)
		{
			int equippedObjectCount = Object.Body.GetBody().GetEquippedObjectCount();
			int count = Object.Inventory.Objects.Count;
			if (Stat.Rnd.Next(equippedObjectCount + count) < equippedObjectCount)
			{
				return Object.Body.GetEquippedObjectsReadonly().GetRandomElement() ?? Object.Inventory.Objects.GetRandomElement();
			}
			return Object.Inventory.Objects.GetRandomElement() ?? Object.Body.GetEquippedObjectsReadonly().GetRandomElement();
		}
		if (flag)
		{
			return Object.Inventory.Objects.GetRandomElement();
		}
		return Object.Body.GetEquippedObjectsReadonly().GetRandomElement();
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		if (Object.IsCreature)
		{
			Registrar.Register("AttackerHit");
			return;
		}
		if (Object.IsProjectile)
		{
			Registrar.Register("ProjectileHit");
			return;
		}
		Registrar.Register("WeaponHit");
		Registrar.Register("WeaponThrowHit");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID.EndsWith("Hit"))
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject Object = E.GetGameObjectParameter("Defender");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Weapon");
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("Projectile");
			if (GameObject.Validate(ref Object))
			{
				int num = Amount.RollCached();
				if (num <= 0)
				{
					return base.FireEvent(E);
				}
				GameObject subject = Object;
				GameObject projectile = gameObjectParameter3;
				int chance = GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, gameObjectParameter2, "Part RustOnHit Activation", Chance, subject, projectile);
				bool flag = AffectInventory;
				bool flag2 = AffectEquipment;
				if (Object.HasRegisteredEvent("GetRustableContents"))
				{
					Event obj = Event.New("GetRustableContents", "Inventory", flag ? 1 : 0, "Equipment", flag2 ? 1 : 0);
					Object.FireEvent(obj);
					flag = obj.GetIntParameter("Inventory") >= 1;
					flag2 = obj.GetIntParameter("Equipment") >= 1;
					if (!flag && !flag2)
					{
						return base.FireEvent(E);
					}
				}
				for (int i = 0; i < num; i++)
				{
					if (chance.in100())
					{
						GetRandomItemFrom(Object, flag, flag2)?.ApplyEffect(new Rusted());
					}
				}
				if (!Object.IsHostileTowards(gameObjectParameter))
				{
					Object.AddOpinion<OpinionAttack>(gameObjectParameter, gameObjectParameter2);
				}
			}
		}
		return base.FireEvent(E);
	}
}
