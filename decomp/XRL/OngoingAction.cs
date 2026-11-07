namespace XRL;

public abstract class OngoingAction : IOngoingAction
{
	public virtual string GetDescription()
	{
		return "acting";
	}

	public virtual bool IsMovement()
	{
		return false;
	}

	public virtual bool IsCombat()
	{
		return false;
	}

	public virtual bool IsExploration()
	{
		return false;
	}

	public virtual bool IsGathering()
	{
		return false;
	}

	public virtual bool IsResting()
	{
		return false;
	}

	public virtual bool IsRateLimited()
	{
		return false;
	}

	public virtual bool Continue()
	{
		return true;
	}

	public virtual string GetInterruptBecause()
	{
		return null;
	}

	public virtual bool ShouldHostilesInterrupt()
	{
		return true;
	}

	public virtual void Interrupt()
	{
	}

	public virtual void Resume()
	{
	}

	public virtual bool CanComplete()
	{
		return true;
	}

	public virtual void Complete()
	{
	}

	public virtual void End()
	{
	}
}
