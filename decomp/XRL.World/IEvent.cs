namespace XRL.World;

public interface IEvent
{
	void RequestInterfaceExit();

	bool InterfaceExitRequested();

	void PreprocessChildEvent(IEvent E);

	void ProcessChildEvent(IEvent E);

	bool ActuateOn(GameObject obj);
}
