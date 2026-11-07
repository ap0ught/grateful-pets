namespace XRL.CharacterBuilds.Qud;

public class QudGamemodeModuleData : AbstractEmbarkBuilderModuleData
{
	public string Mode;

	public bool DoesSupportCharacterSelection()
	{
		return Mode != "Daily";
	}
}
