using System;

namespace XRL.World.Effects;

[Serializable]
public class BasicTriggeredCookingEffect : Effect
{
	public string wellFedMessage = "You eat the meal. It's tastier than usual.";

	public bool removed;

	public BasicTriggeredCookingEffect()
	{
		DisplayName = "{{W|well fed}}";
		Duration = 1;
	}

	public override int GetEffectType()
	{
		return 67108868;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDescription()
	{
		return "{{w|metabolized effect}}";
	}

	public virtual void ApplyEffect(GameObject Object)
	{
	}

	public virtual void RemoveEffect(GameObject Object)
	{
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.FireEvent(Event.New("ApplyBasicTriggeredCookingEffect")))
		{
			ApplyEffect(Object);
			return true;
		}
		return false;
	}

	public override void Remove(GameObject Object)
	{
		if (Duration > 0)
		{
			Duration = 0;
		}
		if (!removed)
		{
			RemoveEffect(Object);
			removed = true;
		}
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EndTurn");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			Duration--;
		}
		return true;
	}
}
