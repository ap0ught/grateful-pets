namespace XRL.World;

public interface IRepairSifrahHandler
{
	void RepairResultSuccess(GameObject who, GameObject obj);

	void RepairResultExceptionalSuccess(GameObject who, GameObject obj);

	void RepairResultPartialSuccess(GameObject who, GameObject obj);

	void RepairResultFailure(GameObject who, GameObject obj);

	void RepairResultCriticalFailure(GameObject who, GameObject obj);
}
