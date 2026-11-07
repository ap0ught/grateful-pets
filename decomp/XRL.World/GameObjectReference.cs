using System;

namespace XRL.World;

[Serializable]
public class GameObjectReference : IComposite
{
	public int ID;

	public GameObject Object;

	public bool WantFieldReflection => false;

	public GameObjectReference()
	{
	}

	public GameObjectReference(GameObject Object)
	{
		Set(Object);
	}

	public void Set(GameObject Object)
	{
		ID = Object?.BaseID ?? 0;
		this.Object = Object;
	}

	public void Clear()
	{
		ID = 0;
		Object = null;
	}

	public void Read(SerializationReader Reader)
	{
		ID = Reader.ReadOptimizedInt32();
		Object = Reader.ReadGameObject();
	}

	public void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(ID);
		Writer.WriteGameObject(Object, Reference: true);
	}

	public static void Free(ref GameObjectReference Reference)
	{
		if (Reference != null)
		{
			Reference.ID = 0;
			Reference.Object = null;
			Reference = null;
		}
	}
}
