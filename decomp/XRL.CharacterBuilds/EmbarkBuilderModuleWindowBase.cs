namespace XRL.CharacterBuilds;

public class EmbarkBuilderModuleWindowBase<T> : AbstractBuilderModuleWindowBase where T : AbstractEmbarkBuilderModule
{
	public T module => _module as T;
}
