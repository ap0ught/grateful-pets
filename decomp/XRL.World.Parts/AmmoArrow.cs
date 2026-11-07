using System;
using System.Text;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class AmmoArrow : IAmmo
{
	public string ProjectileObject;

	public override bool SameAs(IPart p)
	{
		if ((p as AmmoArrow).ProjectileObject != ProjectileObject)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != PooledEvent<GetProjectileObjectEvent>.ID && ID != QueryEquippableListEvent.ID)
		{
			if (Options.AutogetPrimitiveAmmo)
			{
				return ID == AutoexploreObjectEvent.ID;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(AutoexploreObjectEvent E)
	{
		if (E.Command == null && Options.AutogetPrimitiveAmmo)
		{
			E.Command = ((LoadedIn != null) ? "UnloadMagazineAmmo" : "Autoget");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood())
		{
			GameObjectBlueprint blueprintIfExists = GameObjectFactory.Factory.GetBlueprintIfExists(ProjectileObject);
			if (blueprintIfExists != null)
			{
				StringBuilder stringBuilder = Event.NewStringBuilder();
				stringBuilder.Append("(").Append(blueprintIfExists.GetPartParameter("Projectile", "BasePenetration", 0) + RuleSettings.VISUAL_PENETRATION_BONUS).Append("/")
					.Append(blueprintIfExists.GetPartParameter<string>("Projectile", "BaseDamage"))
					.Append(")");
				E.AddTag(stringBuilder.ToString());
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryEquippableListEvent E)
	{
		if (E.SlotType.Contains("AmmoArrow") && !E.List.Contains(ParentObject))
		{
			E.List.Add(ParentObject);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetProjectileObjectEvent E)
	{
		if (!string.IsNullOrEmpty(ProjectileObject))
		{
			E.Projectile = GameObject.Create(ProjectileObject);
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
