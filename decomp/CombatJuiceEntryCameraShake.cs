public class CombatJuiceEntryCameraShake : CombatJuiceEntry
{
	public float shakeDuration;

	public CombatJuiceEntryCameraShake(float shakeDuration)
	{
		t = 0f;
		this.shakeDuration = shakeDuration;
	}

	public override void start()
	{
		CombatJuice._cameraShake(shakeDuration);
	}
}
