namespace XRL.World.Effects;

public class CookingDomainRubber_OnJump : ProceduralCookingEffectWithTrigger
{
	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature jump@s,";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<JumpedEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(JumpedEvent E)
	{
		if (E.Pass == JumpedEvent.PASSES)
		{
			string sourceKey = E.SourceKey;
			if (sourceKey == null || !sourceKey.Contains("CookingDomainRubber"))
			{
				Trigger();
			}
		}
		return base.HandleEvent(E);
	}
}
