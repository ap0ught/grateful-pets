using XRL;
using XRL.World;
using XRL.World.Parts;

[PlayerMutator]
public class WishMenu_PlayerMutator : IPlayerMutator
{
	public void mutate(GameObject player)
	{
		player.RequirePart<WishMenu>();
	}
}
