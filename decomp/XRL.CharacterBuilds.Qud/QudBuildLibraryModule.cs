namespace XRL.CharacterBuilds.Qud;

public class QudBuildLibraryModule : QudEmbarkBuilderModule<QudBuildLibraryModuleData>
{
	public override bool shouldBeEditable()
	{
		return builder.IsEditableGameMode();
	}

	public override bool IncludeInBuildCodes()
	{
		return false;
	}

	public override bool shouldBeEnabled()
	{
		return builder?.GetModule<QudChartypeModule>()?.data?.type == "Library";
	}

	public override void InitFromSeed(string seed)
	{
	}
}
