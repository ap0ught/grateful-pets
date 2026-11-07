using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainPhase_UnitPhaseOnDamage : ProceduralCookingEffectUnit
{
	public int Tier = 1;

	public override void Init(GameObject target)
	{
		Tier = Stat.Random(15, 20);
	}

	public override string GetDescription()
	{
		return "whenever @thisCreature take@s damage, there's a " + Tier + "% chance @they start phasing for 8-10 turns.";
	}

	public override string GetTemplatedDescription()
	{
		return "whenever @thisCreature take@s damage, there's a 15-20% chance @they start phasing for 8-10 turns.";
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
		if (!(E.ID == "BeforeApplyDamage"))
		{
			return;
		}
		GameObject gameObject = parent.Object;
		if (gameObject != null && Stat.Random(1, 100) <= Tier && gameObject.CurrentCell != null && !gameObject.OnWorldMap() && !gameObject.HasEffect<Phased>() && IComponent<GameObject>.CheckRealityDistortionAccessibility(gameObject))
		{
			gameObject.ApplyEffect(new Phased(Stat.Random(8, 10)));
			for (int i = 0; i < 5; i++)
			{
				XRLCore.ParticleManager.AddRadial("&bù", gameObject.CurrentCell.X, gameObject.CurrentCell.Y, Stat.Random(0, 7), Stat.Random(5, 10), 0.01f * (float)Stat.Random(4, 6), -0.05f * (float)Stat.Random(3, 7));
			}
		}
	}
}
