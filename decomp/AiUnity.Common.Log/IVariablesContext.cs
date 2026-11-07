namespace AiUnity.Common.Log;

public interface IVariablesContext
{
	void Set(string key, object value);

	object Get(string key);

	bool Contains(string key);

	void Remove(string key);

	void Clear();
}
