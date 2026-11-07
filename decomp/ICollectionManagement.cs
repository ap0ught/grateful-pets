public interface ICollectionManagement<T>
{
	void Add(T el);

	T Get(int index);

	void RemoveRange(int index, int count);

	void RemoveAt(int index);
}
