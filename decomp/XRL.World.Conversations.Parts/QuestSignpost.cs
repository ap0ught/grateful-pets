using System.Collections.Generic;
using System.Text;
using XRL.World.Parts;

namespace XRL.World.Conversations.Parts;

public class QuestSignpost : IConversationPart
{
	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation))
		{
			return ID == PrepareTextEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(PrepareTextEvent E)
	{
		XRLGame game = The.Game;
		List<GameObject> list = Event.NewGameObjectList();
		StringBuilder stringBuilder = Event.NewStringBuilder();
		GameObject speaker = The.Speaker;
		Zone parentZone = speaker.CurrentCell.ParentZone;
		Brain brain = speaker.Brain;
		Zone.ObjectEnumerator enumerator = parentZone.IterateObjects().GetEnumerator();
		while (enumerator.MoveNext())
		{
			GameObject current = enumerator.Current;
			if (brain.InSameFactionAs(current) && game.GetQuestGiverState(current) == 0)
			{
				list.Add(current);
			}
		}
		list.Remove(speaker);
		string text = "";
		bool flag = false;
		int i = 0;
		for (int num = list.Count - 1; i <= num; i++)
		{
			GameObject gameObject = list[i];
			string text2 = speaker.DescribeDirectionToward(gameObject, General: true);
			if (i > 0)
			{
				if (i == num)
				{
					stringBuilder.Append(flag ? ", or " : " or ");
				}
				else
				{
					stringBuilder.Append(", ");
				}
			}
			stringBuilder.Append("{{y|" + gameObject.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutTitles: true, ForSort: false, Short: false, BaseOnly: true) + "}}");
			IZoneLandmark bestFor = IZoneLandmark.GetBestFor(gameObject.CurrentCell);
			if (bestFor != null)
			{
				stringBuilder.Append(',');
				bestFor.Append(stringBuilder, gameObject);
			}
			if (!text2.IsNullOrEmpty())
			{
				if (bestFor == null)
				{
					stringBuilder.Append(',');
				}
				stringBuilder.Append((text2 == text) ? " also " : " ").Append(text2);
				flag = true;
				text = text2;
			}
		}
		E.Text.Replace("=questgivers=", Event.FinalizeString(stringBuilder));
		return base.HandleEvent(E);
	}
}
