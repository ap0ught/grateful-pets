using System;
using System.Collections.Generic;
using XRL.Messages;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Cudgel_Backswing : BaseSkill
{
	[NonSerialized]
	private List<GameObject> WeaponsUsed = new List<GameObject>(1);

	[NonSerialized]
	private long BackswingSegment = -1L;

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AttackerAfterAttack");
		Registrar.Register("AttackerMeleeMiss");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AttackerAfterAttack" || E.ID == "AttackerMeleeMiss")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject Object = E.GetGameObjectParameter("Weapon");
			GameObject Object2 = E.GetGameObjectParameter("Defender");
			if (GameObject.Validate(ref Object) && GameObject.Validate(ref Object2) && Object2.HasHitpoints())
			{
				long segments = The.Game.Segments;
				if (BackswingSegment != segments)
				{
					WeaponsUsed.Clear();
					BackswingSegment = segments;
				}
				if (!WeaponsUsed.Contains(Object) && Object.IsEquippedOrDefaultOfPrimary(gameObjectParameter))
				{
					WeaponsUsed.Add(Object);
					MeleeWeapon part = Object.GetPart<MeleeWeapon>();
					if (part != null && part.Skill.Contains("Cudgel") && GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, Object, "Skill Backswing", 25, Object2).in100())
					{
						if (ParentObject.IsPlayer())
						{
							MessageQueue.AddPlayerMessage("You backswing with " + ParentObject.its_(Object) + ".", 'g');
						}
						if (Object2.IsPlayer())
						{
							MessageQueue.AddPlayerMessage(gameObjectParameter.The + gameObjectParameter.ShortDisplayName + gameObjectParameter.GetVerb("backswing") + " with " + gameObjectParameter.its_(Object) + ".", 'r');
						}
						Combat.MeleeAttackWithWeapon(gameObjectParameter, Object2, Object, gameObjectParameter.Body.FindDefaultOrEquippedItem(Object), null, 0, 0, 0, 0, 0, Primary: true);
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
