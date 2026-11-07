using System;

namespace XRL.World;

[AttributeUsage(AttributeTargets.Field)]
public class FieldSaveVersion : Attribute
{
	private int _minimumSaveVersion = -1;

	public virtual int minimumSaveVersion => _minimumSaveVersion;

	public FieldSaveVersion()
	{
	}

	public FieldSaveVersion(int min)
	{
		_minimumSaveVersion = min;
	}
}
