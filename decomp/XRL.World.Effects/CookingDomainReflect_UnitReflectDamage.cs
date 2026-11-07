using System;
using System.Collections.Generic;
using XRL.Messages;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainReflect_UnitReflectDamage : ProceduralCookingEffectUnit
{
	public int Tier = 1;

	public override void Init(GameObject target)
	{
		Tier = Stat.Random(3, 4);
		base.Init(target);
	}

	public override string GetDescription()
	{
		return "Reflect " + Tier + "% damage back at @their attackers, rounded up.";
	}

	public override string GetTemplatedDescription()
	{
		return "Reflect 3-4% damage back at @their attackers, rounded up.";
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
		if (!(E.ID == "BeforeApplyDamage") || parent == null || parent.Object == null)
		{
			return;
		}
		Damage damage = E.GetParameter("Damage") as Damage;
		GameObject gameObjectParameter = E.GetGameObjectParameter("Owner");
		if (damage.Amount <= 0 || damage.HasAttribute("reflected"))
		{
			return;
		}
		int num = (int)Math.Ceiling((float)damage.Amount * (float)Tier / 100f);
		if (num > 0 && gameObjectParameter != null && gameObjectParameter != parent.Object)
		{
			Event obj = new Event("TakeDamage");
			Damage damage2 = new Damage(num);
			damage2.Attributes = new List<string>(damage.Attributes);
			if (!damage2.HasAttribute("reflected"))
			{
				damage2.Attributes.Add("reflected");
			}
			obj.AddParameter("Damage", damage2);
			obj.AddParameter("Owner", parent.Object);
			obj.AddParameter("Attacker", parent.Object);
			obj.AddParameter("Message", "from %t tiny spines!");
			if (parent.Object.IsPlayer())
			{
				MessageQueue.AddPlayerMessage("You reflect " + num + " damage back at " + gameObjectParameter.the + gameObjectParameter.ShortDisplayName + "&y.");
			}
			else if (gameObjectParameter.IsPlayer())
			{
				MessageQueue.AddPlayerMessage(parent.Object.The + parent.Object.ShortDisplayName + "&y" + parent.Object.GetVerb("reflect") + " " + num + " damage back at you.");
			}
			else if (parent.Object.IsVisible())
			{
				MessageQueue.AddPlayerMessage(parent.Object.The + parent.Object.ShortDisplayName + "&y" + parent.Object.GetVerb("reflect") + " " + num + " damage back at " + gameObjectParameter.the + gameObjectParameter.ShortDisplayName + "&y.");
			}
			gameObjectParameter.FireEvent(obj);
			parent.Object.FireEvent("ReflectedDamage");
		}
	}
}
