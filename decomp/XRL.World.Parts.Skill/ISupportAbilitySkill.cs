using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public abstract class ISupportAbilitySkill : BaseSkill
{
	[NonSerialized]
	public ActivatedAbilityEntry AbilityEntry;

	public abstract string AbilityCommand { get; }

	public virtual string SupportType => AbilityCommand;

	public abstract ActivatedAbilityEntry RequireAbility();

	public override void Initialize()
	{
		base.Initialize();
		AbilityEntry = ParentObject.GetActivatedAbilityByCommand(AbilityCommand) ?? RequireAbility();
	}

	public override void Remove()
	{
		base.Remove();
		NeedPartSupportEvent.Send(ParentObject, SupportType, this);
	}

	public override void FinalizeRead(SerializationReader Reader)
	{
		base.FinalizeRead(Reader);
		if (AbilityEntry == null)
		{
			AbilityEntry = ParentObject.GetActivatedAbilityByCommand(AbilityCommand);
		}
	}

	public override void FinalizeCopy(GameObject Source, bool CopyEffects, bool CopyID, Func<GameObject, GameObject> MapInv)
	{
		base.FinalizeCopy(Source, CopyEffects, CopyID, MapInv);
		if (AbilityEntry == null)
		{
			AbilityEntry = ParentObject.GetActivatedAbilityByCommand(AbilityCommand);
		}
	}

	public override bool WantEvent(int ID, int Cascade)
	{
		if (!base.WantEvent(ID, Cascade))
		{
			return ID == PooledEvent<PartSupportEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(PartSupportEvent E)
	{
		if (E.Skip != this && E.Type == SupportType)
		{
			return false;
		}
		return base.HandleEvent(E);
	}
}
