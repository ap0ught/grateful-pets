using System;
using XRL.World;

namespace XRL;

[Obsolete("Use IComposite")]
public interface IGamestatePostsave
{
	void OnGamestatePostsave(XRLGame game, SerializationWriter writer);
}
