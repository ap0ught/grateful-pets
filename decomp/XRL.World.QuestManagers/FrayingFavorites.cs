using System;
using XRL.World.Quests;

namespace XRL.World.QuestManagers;

[Serializable]
public class FrayingFavorites : QuestManager
{
	public override void FinalizeRead(SerializationReader Reader)
	{
		base.FinalizeRead(Reader);
		XRLGame game = The.Game;
		if (game != null && game.HasUnfinishedQuest("Fraying Favorites"))
		{
			game.RequireSystem<FrayingFavoritesSystem>();
		}
	}
}
