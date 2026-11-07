namespace XRL;

internal interface IOngoingAction
{
	string GetDescription();

	bool IsMovement();

	bool IsCombat();

	bool IsExploration();

	bool IsGathering();

	bool IsResting();

	bool IsRateLimited();

	bool Continue();

	string GetInterruptBecause();

	bool ShouldHostilesInterrupt();

	void Interrupt();

	void Resume();

	bool CanComplete();

	void Complete();

	void End();
}
