using System;
using XRL.Messages;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainTeleport_UnitBlink : ProceduralCookingEffectUnit
{
	public int Tier = 1;

	public override void Init(GameObject target)
	{
		Tier = Stat.Random(20, 25);
	}

	public override string GetDescription()
	{
		return "Whenever @thisCreature take@s avoidable damage, there's a " + Tier + "% chance @they teleport to a random space on the map instead.";
	}

	public override string GetTemplatedDescription()
	{
		return "Whenever @thisCreature take@s avoidable damage, there's a 20-25% chance @they teleport to a random space on the map instead.";
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.RegisterEffectEvent(parent, "BeforeApplyDamage");
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		Object.UnregisterEffectEvent(parent, "BeforeApplyDamage");
	}

	public override void FireEvent(Event E)
	{
		if (!(E.ID == "BeforeApplyDamage") || parent == null)
		{
			return;
		}
		GameObject gameObject = parent.Object;
		if (gameObject == null)
		{
			return;
		}
		Cell currentCell = gameObject.CurrentCell;
		if (currentCell == null || currentCell.ParentZone == null || currentCell.ParentZone.IsWorldMap())
		{
			return;
		}
		Damage damage = E.GetParameter("Damage") as Damage;
		if (!damage.HasAttribute("Unavoidable") && IComponent<GameObject>.CheckRealityDistortionUsability(gameObject, null, gameObject) && Tier.in100())
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Owner");
			if (gameObjectParameter != null && gameObjectParameter.IsPlayer())
			{
				MessageQueue.AddPlayerMessage("Fate intervenes and you deal no damage to " + gameObject.t() + ".", 'r');
			}
			damage.Amount = 0;
			gameObject.RandomTeleport(Swirl: true);
		}
	}
}
