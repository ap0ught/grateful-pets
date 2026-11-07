namespace XRL.World;

public interface IHackingSifrahHandler
{
	void HackingResultSuccess(GameObject Actor, GameObject Object, HackingSifrah Game);

	void HackingResultExceptionalSuccess(GameObject Actor, GameObject Object, HackingSifrah Game);

	void HackingResultPartialSuccess(GameObject Actor, GameObject Object, HackingSifrah Game);

	void HackingResultFailure(GameObject Actor, GameObject Object, HackingSifrah Game);

	void HackingResultCriticalFailure(GameObject Actor, GameObject Object, HackingSifrah Game);
}
