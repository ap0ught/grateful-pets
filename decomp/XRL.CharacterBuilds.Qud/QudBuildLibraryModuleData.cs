namespace XRL.CharacterBuilds.Qud;

public class QudBuildLibraryModuleData : AbstractEmbarkBuilderModuleData
{
	public string BuildID;

	public QudBuildLibraryModuleData()
	{
	}

	public QudBuildLibraryModuleData(string buildID)
	{
		BuildID = buildID;
	}
}
