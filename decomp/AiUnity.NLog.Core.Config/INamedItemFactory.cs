namespace AiUnity.NLog.Core.Config;

public interface INamedItemFactory<TInstanceType, TDefinitionType> where TInstanceType : class
{
	void RegisterDefinition(string itemName, TDefinitionType itemDefinition);

	bool TryGetDefinition(string itemName, out TDefinitionType result);

	TInstanceType CreateInstance(string itemName);

	bool TryCreateInstance(string itemName, out TInstanceType result);
}
