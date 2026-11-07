using System;

namespace XRL;

/// <summary>A game state that will be initialized once on new game.</summary>
[AttributeUsage(AttributeTargets.Class)]
public class GameStateSingleton : Attribute
{
	/// <summary>Defaults to the type name of the class.</summary>
	public string ID;

	public GameStateSingleton()
	{
	}

	public GameStateSingleton(string ID)
	{
		this.ID = ID;
	}
}
[Obsolete("Use GameStateSingleton")]
[AttributeUsage(AttributeTargets.Class)]
public class GamestateSingleton : Attribute
{
	public string id;

	public GamestateSingleton(string ID)
	{
	}
}
