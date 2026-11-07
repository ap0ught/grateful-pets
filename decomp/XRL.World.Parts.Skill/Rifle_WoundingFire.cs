using System;
using System.Collections.Generic;
using XRL.UI;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Rifle_WoundingFire : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CommandWoundingFire");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandWoundingFire")
		{
			List<GameObject> missileWeapons = ParentObject.GetMissileWeapons();
			bool flag = false;
			bool flag2 = false;
			string text = null;
			GameObject gameObject = null;
			if (missileWeapons != null && missileWeapons.Count > 0)
			{
				foreach (GameObject item in missileWeapons)
				{
					MissileWeapon part = item.GetPart<MissileWeapon>();
					if (part != null && (part.Skill == "Rifle" || part.Skill == "Bow"))
					{
						flag2 = true;
						if (part.ReadyToFire())
						{
							flag = true;
							break;
						}
						if (text == null)
						{
							text = part.GetNotReadyToFireMessage();
							gameObject = part.ParentObject;
						}
					}
				}
			}
			if (!flag2)
			{
				if (ParentObject.IsPlayer())
				{
					Popup.Show("You do not have a bow or rifle equipped!");
				}
			}
			else if (!flag)
			{
				if (ParentObject.IsPlayer())
				{
					SoundManager.PlaySound(gameObject?.GetSoundTag("NoAmmoSound"));
					Popup.Show(text ?? ("You need to reload! (" + ControlManager.getCommandInputDescription("CmdReload", Options.ModernUI) + ")"));
				}
			}
			else
			{
				Combat.FireMissileWeapon(ParentObject, null, null, FireType.WoundingFire);
			}
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		return base.RemoveSkill(GO);
	}
}
