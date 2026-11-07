using ConsoleLib.Console;
using Qud.UI;
using UnityEngine.UI;
using XRL.UI;
using XRL.Wish;

[UIView("MarkovCorpusGenerator", false, true, false, null, "MarkovCorpusGenerator", false, 0, false, UICanvasHost = 1)]
[HasWishCommand]
public class MarkovCorpusGeneratorView : SingletonWindowBase<MarkovCorpusGeneratorView>
{
	private MarkovCorpusGenerator generator;

	public override void Show()
	{
		generator = gameObject.AddComponent<MarkovCorpusGenerator>();
		generator.rootObject = gameObject;
		base.Show();
	}

	public void OnCommand(string Command)
	{
		if (Command == "Back")
		{
			UIManager.getWindow("MarkovCorpusGenerator").Hide();
		}
		if (Command == "Generate")
		{
			gameObject.transform.Find("ProgressPanel").gameObject.SetActive(value: true);
			gameObject.transform.Find("ProgressPanel/ProgressLabel").gameObject.GetComponent<UnityEngine.UI.Text>().text = "Hotloading game configuration...";
			generator.Generate();
		}
	}

	[WishCommand("corpusgenerator", null)]
	public static void WishDisplay()
	{
		GameManager.Instance.PushGameView("MarkovCorpusGenerator");
		Keyboard.getvk(MapDirectionToArrows: false);
	}
}
