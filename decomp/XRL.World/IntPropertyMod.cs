using System;

namespace XRL.World;

public struct IntPropertyMod : IDisposable
{
	public GameObject Object;

	public string Property;

	public int Value;

	public IntPropertyMod(GameObject Object, string Property, int Value)
	{
		this.Object = Object;
		this.Property = Property;
		this.Value = Value;
		Object.ModIntProperty(Property, Value, RemoveIfZero: true);
	}

	public void Dispose()
	{
		if (Value != 0)
		{
			Object.ModIntProperty(Property, Value * -1, RemoveIfZero: true);
			Value = 0;
		}
	}
}
