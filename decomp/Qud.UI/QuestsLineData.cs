using System.Collections.Generic;
using System.Text;
using XRL.UI.Framework;
using XRL.World;

namespace Qud.UI;

public class QuestsLineData : PooledFrameworkDataElement<QuestsLineData>
{
	public bool expanded = true;

	public Quest quest;

	public QuestsLine line;

	public string _searchText;

	public string searchText
	{
		get
		{
			if (_searchText == null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(quest?.DisplayName);
				stringBuilder.Append(" ");
				stringBuilder.Append(quest?.QuestGiverLocationName);
				stringBuilder.Append(" ");
				stringBuilder.Append(quest?.QuestGiverName);
				stringBuilder.Append(" ");
				if (quest != null)
				{
					foreach (KeyValuePair<string, QuestStep> item in quest.StepsByID)
					{
						stringBuilder.Append(" ");
						stringBuilder.Append(item.Value.Name);
						stringBuilder.Append(" ");
						stringBuilder.Append(item.Value.Text);
					}
				}
				_searchText = stringBuilder.ToString().ToLower();
			}
			return _searchText;
		}
		set
		{
			_searchText = value;
		}
	}

	public QuestsLineData set(Quest quest, bool expanded)
	{
		this.quest = quest;
		this.expanded = expanded;
		return this;
	}

	public override void free()
	{
		_searchText = null;
		expanded = true;
		quest = null;
		base.free();
	}
}
