using System;

namespace XRL.World;

[Serializable]
public class CachedZoneConnection : ZoneConnection
{
	public string TargetDirection;

	public CachedZoneConnection(string _TD, int _X, int _Y, string _Type, string _Object)
	{
		TargetDirection = _TD;
		X = _X;
		Y = _Y;
		Type = _Type;
		Object = _Object;
	}

	public override string ToString()
	{
		return TargetDirection + " " + Type + " " + Object;
	}
}
