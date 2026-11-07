using System;
using XRL.World;

namespace XRL;

[Obsolete("Use IComposite")]
public interface IGamestatePostload
{
	void OnGamestatePostload(XRLGame game, SerializationReader reader);
}
