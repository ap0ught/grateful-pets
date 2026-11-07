namespace XRL.World;

public interface IDisarmingSifrahHandler
{
	void DisarmingResultSuccess(GameObject Actor, GameObject Object, bool Auto);

	void DisarmingResultExceptionalSuccess(GameObject Actor, GameObject Object, bool Auto);

	void DisarmingResultPartialSuccess(GameObject Actor, GameObject Object, bool Auto);

	void DisarmingResultFailure(GameObject Actor, GameObject Object, bool Auto);

	void DisarmingResultCriticalFailure(GameObject Actor, GameObject Object, bool Auto);
}
