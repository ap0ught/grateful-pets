using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XRL.Language;

namespace XRL.World.Effects;

[Serializable]
public class ProceduralCookingEffectWithTrigger : ProceduralCookingEffect
{
	public List<ProceduralCookingTriggeredAction> triggeredActions = new List<ProceduralCookingTriggeredAction>();

	public virtual string GetTriggerDescription()
	{
		return "[when something happens]";
	}

	public virtual string GetTemplatedTriggerDescription()
	{
		return GetTriggerDescription();
	}

	public override bool SameAs(ProceduralCookingEffect effect)
	{
		if (!base.SameAs(effect))
		{
			return false;
		}
		if (!(effect is ProceduralCookingEffectWithTrigger proceduralCookingEffectWithTrigger))
		{
			return false;
		}
		if (units.Count != effect.units.Count)
		{
			return false;
		}
		foreach (ProceduralCookingTriggeredAction action in triggeredActions)
		{
			if (!proceduralCookingEffectWithTrigger.triggeredActions.Any((ProceduralCookingTriggeredAction e) => e.GetType() == action.GetType()))
			{
				return false;
			}
		}
		return true;
	}

	public override string GetTemplatedProceduralEffectDescription()
	{
		StringBuilder stringBuilder = new StringBuilder(base.GetTemplatedProceduralEffectDescription());
		stringBuilder.Append(GetTemplatedTriggerDescription());
		stringBuilder.Append(' ');
		for (int i = 0; i < triggeredActions.Count; i++)
		{
			if (i != 0)
			{
				if (i != triggeredActions.Count - 1)
				{
					stringBuilder.Append(", ");
				}
				else
				{
					stringBuilder.Append(" and ");
				}
			}
			stringBuilder.Append(triggeredActions[i].GetTemplatedDescription());
		}
		return stringBuilder.ToString();
	}

	public override string GetProceduralEffectDescription()
	{
		StringBuilder stringBuilder = new StringBuilder(base.GetProceduralEffectDescription());
		stringBuilder.Append(GetTriggerDescription());
		stringBuilder.Append(' ');
		for (int i = 0; i < triggeredActions.Count; i++)
		{
			if (i != 0)
			{
				if (i != triggeredActions.Count - 1)
				{
					stringBuilder.Append(", ");
				}
				else
				{
					stringBuilder.Append(" and ");
				}
			}
			stringBuilder.Append(triggeredActions[i].GetDescription());
		}
		return stringBuilder.ToString();
	}

	public override void Init(GameObject target)
	{
		base.Init(target);
		foreach (ProceduralCookingTriggeredAction triggeredAction in triggeredActions)
		{
			triggeredAction.Init(target);
		}
	}

	public override Effect DeepCopy(GameObject Parent)
	{
		ProceduralCookingEffectWithTrigger proceduralCookingEffectWithTrigger = (ProceduralCookingEffectWithTrigger)base.DeepCopy(Parent);
		proceduralCookingEffectWithTrigger.triggeredActions = new List<ProceduralCookingTriggeredAction>(triggeredActions.Count);
		foreach (ProceduralCookingTriggeredAction triggeredAction in triggeredActions)
		{
			proceduralCookingEffectWithTrigger.triggeredActions.Add(triggeredAction.DeepCopy());
		}
		return proceduralCookingEffectWithTrigger;
	}

	public void Trigger()
	{
		foreach (ProceduralCookingTriggeredAction triggeredAction in triggeredActions)
		{
			triggeredAction.Apply(Object);
			string notification = triggeredAction.GetNotification();
			if (notification == null)
			{
				break;
			}
			if (Object.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage(Grammar.CapAfterNewlines(Grammar.InitCap(notification.Replace("@thisCreature", "you").Replace("@s", "").Replace("@es", "")
					.Replace("@is", "are")
					.Replace("@they", "you")
					.Replace("@their", "your")
					.Replace("@them", "you"))));
			}
			else if (Visible())
			{
				IComponent<GameObject>.AddPlayerMessage(Grammar.CapAfterNewlines(Grammar.InitCap(notification.Replace("@thisCreature", Object.IsPlural ? "these creatures" : "this creature").Replace("@s", Object.IsPlural ? "" : "s").Replace("@es", Object.IsPlural ? "" : "es")
					.Replace("@is", Object.Is)
					.Replace("@they", Object.it)
					.Replace("@their", Object.its)
					.Replace("@them", Object.them))));
			}
		}
	}

	public override void Remove(GameObject Object)
	{
		Object.FireEvent("RemoveProceduralCookingEffects");
		base.Remove(Object);
		foreach (ProceduralCookingTriggeredAction triggeredAction in triggeredActions)
		{
			triggeredAction.Remove(Object);
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		foreach (ProceduralCookingTriggeredAction triggeredAction in triggeredActions)
		{
			if (triggeredAction.WantEvent(ID, cascade))
			{
				return true;
			}
		}
		return base.WantEvent(ID, cascade);
	}

	public override bool HandleEvent(MinEvent E)
	{
		foreach (ProceduralCookingTriggeredAction triggeredAction in triggeredActions)
		{
			if (!E.Dispatch(triggeredAction))
			{
				return false;
			}
			if (!triggeredAction.HandleEvent(E))
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}
}
