namespace XRL.World.Effects;

public class CookingDomainArtifact_OnIdentify : ProceduralCookingEffectWithTrigger
{
	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature identify an artifact,";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<ExamineSuccessEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(ExamineSuccessEvent E)
	{
		if (E.Complete)
		{
			Trigger();
		}
		return base.HandleEvent(E);
	}
}
