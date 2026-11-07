namespace XRL.CharacterBuilds.Qud;

public class QudChooseStartingLocationModuleData : AbstractEmbarkBuilderModuleData
{
	public string StartingLocation;

	public QudChooseStartingLocationModuleData()
	{
	}

	public QudChooseStartingLocationModuleData(string startingLocation)
	{
		StartingLocation = startingLocation;
	}
}
