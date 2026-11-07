using System;
using XRL.World;

namespace XRL;

[Obsolete("Use IComposite.WantFieldReflection")]
public interface IObjectGamestateCustomSerializer
{
	IGameStateSingleton GameLoad(SerializationReader reader);

	void GameSave(SerializationWriter writer);
}
