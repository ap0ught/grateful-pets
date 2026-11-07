using System;

namespace XRL.World.Parts;

[Serializable]
public class SpawnWithEffect : IPart
{
	public string Effect;

	public int Chance = 100;

	[NonSerialized]
	private Type _effectType;

	private Type effectType
	{
		get
		{
			if (_effectType == null)
			{
				if (!Effect.Contains("."))
				{
					_effectType = ModManager.ResolveType("XRL.World.Effects." + Effect);
				}
				else
				{
					_effectType = ModManager.ResolveType(Effect);
				}
			}
			return _effectType;
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AfterObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AfterObjectCreatedEvent E)
	{
		if (!string.IsNullOrEmpty(Effect) && Chance.in100())
		{
			Type type = effectType;
			if (ParentObject.HasEffect(type))
			{
				ParentObject.GetEffect(type).Duration = 9999;
			}
			else
			{
				Effect effect = Activator.CreateInstance(type) as Effect;
				effect.Duration = 9999;
				ParentObject.ApplyEffect(effect);
			}
		}
		ParentObject.RemovePart(this);
		return base.HandleEvent(E);
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}
}
