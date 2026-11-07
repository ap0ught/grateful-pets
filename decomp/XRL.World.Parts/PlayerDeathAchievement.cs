using System;

namespace XRL.World.Parts;

/// <summary>
/// Widget to listen for a player death in current zone by Killer, with Weapon.
/// </summary>
[Serializable]
public class PlayerDeathAchievement : IPart
{
	public string AchievementID = "";

	public string Killer;

	public string Weapon;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		GameObject player = The.Player;
		if (player != null)
		{
			Registrar.Register(player, AfterDieEvent.ID);
		}
	}

	public override bool HandleEvent(AfterDieEvent E)
	{
		if (ParentObject.IsValid())
		{
			GameObject dying = E.Dying;
			if (dying == null || !dying.IsPlayer())
			{
				return true;
			}
			if ((dying.CurrentZone ?? The.ActiveZone) != ParentObject.CurrentZone)
			{
				return true;
			}
			if (!Killer.IsNullOrEmpty())
			{
				GameObject killer = E.Killer;
				if (killer == null)
				{
					return true;
				}
				if (!killer.GetBlueprint().DescendsFrom(Killer))
				{
					return true;
				}
			}
			if (!Weapon.IsNullOrEmpty())
			{
				GameObject weapon = E.Weapon;
				if (weapon == null)
				{
					return true;
				}
				if (!weapon.GetBlueprint().DescendsFrom(Weapon))
				{
					return true;
				}
			}
			AchievementManager.SetAchievement(AchievementID);
			ParentObject.Destroy();
		}
		return base.HandleEvent(E);
	}
}
