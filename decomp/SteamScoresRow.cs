using System.Collections.Generic;
using System.Threading;
using Qud.UI;
using UnityEngine;
using UnityEngine.UI;
using XRL;
using XRL.UI;
using XRL.UI.Framework;

public class SteamScoresRow : MonoBehaviour, IFrameworkControl
{
	public List<UITextSkin> TextSkins;

	public Image background;

	private FrameworkContext _context;

	public CancellationTokenSource cancelLast = new CancellationTokenSource();

	private bool? wasSelected;

	public bool isSelf;

	public FrameworkContext context => _context ?? (_context = GetComponent<FrameworkContext>());

	public void setData(FrameworkDataElement data)
	{
		if (data is HighScoresDataElement highScoresDataElement)
		{
			if (highScoresDataElement.message != null)
			{
				TextSkins[0].SetText(highScoresDataElement.message);
				TextSkins[1].SetText("");
			}
			else
			{
				cancelLast.Cancel();
				cancelLast = new CancellationTokenSource();
				showSteamRow(highScoresDataElement, cancelLast.Token);
			}
			wasSelected = null;
			Update();
		}
	}

	public async void showSteamRow(HighScoresDataElement scoreData, CancellationToken ct)
	{
		if (!ct.IsCancellationRequested)
		{
			LeaderboardEntry entry = scoreData.leaderboardEntry ?? LeaderboardEntry.GenerateEmpty();
			isSelf = entry.isCurrentUser;
			TextSkins[0].SetText($"{{{{Y|{entry.rank})}}}} {{{{{(isSelf ? 'W' : 'y')}|{entry.displayName}}}}}");
			TextSkins[1].SetText($"{entry.score}");
			string arg = await LeaderboardManager.FriendNameAsync(scoreData.leaderboardEntry);
			if (!ct.IsCancellationRequested)
			{
				TextSkins[0].SetText($"{{{{Y|{entry.rank})}}}} {{{{{(isSelf ? 'W' : 'y')}|{arg}}}}}");
			}
		}
	}

	public void Update()
	{
		bool valueOrDefault = context?.context?.IsActive() == true;
		if (valueOrDefault == wasSelected)
		{
			return;
		}
		wasSelected = valueOrDefault;
		Color darkCyan = The.Color.DarkCyan;
		darkCyan.a = (valueOrDefault ? 0.25f : 0f);
		background.color = darkCyan;
		foreach (UITextSkin textSkin in TextSkins)
		{
			if (valueOrDefault)
			{
				textSkin.color = (isSelf ? The.Color.Yellow : The.Color.Gray);
				textSkin.StripFormatting = false;
			}
			else
			{
				textSkin.color = (isSelf ? The.Color.Yellow : The.Color.Cyan);
				textSkin.StripFormatting = true;
			}
			textSkin.Apply();
		}
	}

	public NavigationContext GetNavigationContext()
	{
		return context.context;
	}
}
