using System;
using XRL.World;

namespace XRL;

public interface IGameStateSingleton : IComposite
{
	void Initialize()
	{
	}

	void HandleEvent(EmbarkEvent E)
	{
	}
}
[Obsolete("Use IGameStateSingleton")]
public interface IGamestateSingleton
{
	[Obsolete("Use Initialize")]
	void init()
	{
	}

	[Obsolete("Use HandleEvent with ID == QudGameBootModule.BOOTEVENT_AFTERINITIALIZEWORLDS)")]
	void worldBuild()
	{
	}
}
