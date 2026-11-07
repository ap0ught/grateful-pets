using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Genkit;
using Qud.API;
using XRL.Rules;
using XRL.UI;
using XRL.Wish;

namespace XRL.World.Parts;

[Serializable]
[HasGameBasedStaticCache]
[HasWishCommand]
public class MarkovBook : IPart
{
	public const int FULL_BOOK_CHANCE = 1;

	public const int NOTE_CHANCE = 10;

	public int BookSeed;

	public string BookCorpus;

	public string Title;

	public const int NUMBER_ISNER_SECRETS = 46;

	[NonSerialized]
	public List<BookPage> Pages;

	[NonSerialized]
	public string Sentence;

	[NonSerialized]
	[GameBasedStaticCache(true, false)]
	public static Dictionary<string, MarkovChainData> CorpusData = new Dictionary<string, MarkovChainData>();

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID && ID != PooledEvent<HasBeenReadEvent>.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(HasBeenReadEvent E)
	{
		if (E.Actor == The.Player && GetHasBeenRead())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Read", "read", "Read", null, 'r', FireOnActor: false, GetHasBeenRead() ? 15 : 100);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Read" && E.Actor.IsPlayer())
		{
			if (new Random(BookSeed ^ The.Game.GetWorldSeed("FullBookChance")).Next(100) < 1)
			{
				if (Pages.IsNullOrEmpty())
				{
					GenerateFormattedPage();
				}
				BookUI.ShowBook(this);
			}
			else
			{
				if (Sentence.IsNullOrEmpty())
				{
					GenerateFormattedSentence();
				}
				Popup.Show("You read one of the few legible excerpts from {{Y|" + Title + "}}:\n\n\"" + Sentence + "\"");
			}
			AfterReadBookEvent.Send(E.Actor, ParentObject, this, E);
			if (!GetHasBeenRead())
			{
				SetHasBeenRead(flag: true);
				JournalAPI.AddAccomplishment("You read " + Title + ".", "In the month of " + Calendar.GetMonth() + " of " + Calendar.GetYear() + ", =name= penned the influential book, " + Title + ".", "At a remote library near " + JournalAPI.GetLandmarkNearestPlayer().Text + ", =name= met with a group of blind scribes and together they penned the beloved codex " + Title + ".", null, "general", MuralCategory.CreatesSomething, MuralWeight.VeryLow, null, -1L);
			}
		}
		return base.HandleEvent(E);
	}

	public string GetBookKey()
	{
		return "AlreadyRead_" + BookSeed;
	}

	public bool GetHasBeenRead()
	{
		return The.Game.GetStringGameState(GetBookKey()) == "Yes";
	}

	public void SetHasBeenRead(bool flag)
	{
		if (flag)
		{
			The.Game.SetStringGameState(GetBookKey(), "Yes");
		}
		else
		{
			The.Game.SetStringGameState(GetBookKey(), "");
		}
	}

	public static void EnsureCorpusLoaded(string Corpus)
	{
		try
		{
			if (!CorpusData.TryGetValue(Corpus, out var value))
			{
				value = MarkovChainData.LoadFromFile(DataManager.FilePath(Corpus));
				CorpusData.Add(Corpus, value);
				PostprocessLoadedCorpus(value);
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("EnsureCorpusLoaded:" + Corpus, x);
		}
	}

	public static void PostprocessLoadedCorpus(MarkovChainData Corpus)
	{
		if (The.Game == null)
		{
			return;
		}
		string[] array = new string[6];
		for (int i = 0; i < 5; i++)
		{
			string text = "*Sultan" + (i + 1) + "Name*";
			array[i] = The.Game.GetStringGameState(text);
			Corpus.Replace(text, array[i]);
		}
		array[5] = "Resheph";
		Corpus.Replace("*sultan*", array);
		if (The.Game.IntGameState.TryGetValue("RuinofHouseIsner_xCoordinate", out var value) && The.Game.IntGameState.TryGetValue("RuinofHouseIsner_yCoordinate", out var value2))
		{
			Stat.ReseedFrom("HouseIsnerLore");
			for (int j = 0; j < 46; j++)
			{
				string secret = LoreGenerator.RuinOfHouseIsnerLore(value, value2);
				MarkovChain.AppendSecret(Corpus, secret);
			}
		}
	}

	public void SetContents(int Seed, string Corpus)
	{
		BookSeed = Seed;
		BookCorpus = Corpus;
		EnsureCorpusLoaded(Corpus);
		Stat.ReseedFrom(BookSeed + Hash.String("Title"));
		Title = MarkovChain.GenerateTitle(CorpusData[Corpus]);
		if (20.in100())
		{
			if (50.in100())
			{
				string[] list = new string[10] { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" };
				Title = Title + ", Vol. " + list.GetRandomElement();
			}
			else if (30.in100())
			{
				Title += ": Unabridged";
			}
			else
			{
				int num = ((Stat.Random(1, 10) == 1) ? Stat.Random(1, 100) : Stat.Random(1, 5));
				switch ((num != 11 && num != 12) ? (num % 10) : 0)
				{
				case 1:
					Title = Title + ", " + num + "st Edition";
					break;
				case 2:
					Title = Title + ", " + num + "nd Edition";
					break;
				case 3:
					Title = Title + ", " + num + "rd Edition";
					break;
				default:
					Title = Title + ", " + num + "th Edition";
					break;
				}
			}
		}
		ParentObject.Render.DisplayName = Title;
	}

	public void GeneratePages()
	{
		Pages = new List<BookPage>();
		EnsureCorpusLoaded(BookCorpus);
		Stat.ReseedFrom(BookSeed + Hash.String("Pages"));
		StringBuilder stringBuilder = new StringBuilder();
		if (10.in100())
		{
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			string data = stringBuilder.ToString();
			BookPage bookPage = new BookPage(Title, data);
			bookPage.TopMargin = 2;
			bookPage.LeftMargin = 2;
			bookPage.RightMargin = 2;
			Pages.Add(bookPage);
		}
		string data2 = "\n";
		BookPage bookPage2 = new BookPage(Title, data2);
		bookPage2.TopMargin = 2;
		bookPage2.LeftMargin = 2;
		bookPage2.RightMargin = 2;
		Pages.Add(bookPage2);
		int num = Stat.Random(5, 7);
		for (int i = 0; i < num; i++)
		{
			string text;
			do
			{
				text = MarkovChain.GenerateParagraph(CorpusData[BookCorpus]).Replace("\n", " ").Replace("  ", " ")
					.Trim();
			}
			while (text.Contains("="));
			BookPage bookPage3 = new BookPage(Title, text);
			bookPage3.TopMargin = 2;
			bookPage3.LeftMargin = 2;
			bookPage3.RightMargin = 2;
			Pages.Add(bookPage3);
		}
	}

	public void GenerateFormattedPage()
	{
		Pages = new List<BookPage>();
		EnsureCorpusLoaded(BookCorpus);
		Stat.ReseedFrom(BookSeed + Hash.String("Pages"));
		string text = "";
		if (10.in100())
		{
			text = ((!50.in100()) ? (text + "{{C|Author's note:}} " + MarkovChain.GenerateSentence(CorpusData[BookCorpus]) + "\n\n") : (text + "{{C|Editor's note:}} " + MarkovChain.GenerateSentence(CorpusData[BookCorpus]) + "\n\n"));
		}
		int i = 0;
		for (int num = Stat.Random(8, 30); i < num; i++)
		{
			text += MarkovChain.GenerateParagraph(CorpusData[BookCorpus]);
		}
		if (5.in100())
		{
			int j = 0;
			for (int num2 = Stat.Random(150, 450); j < num2; j++)
			{
				text += MarkovChain.GenerateParagraph(CorpusData[BookCorpus]);
			}
		}
		string format = "Auto";
		string margins = "1,2,2,2";
		Pages.AddRange(BookUI.AutoformatPages(Title, text, format, margins));
		_ = Regex.Matches(text, "parasangs").Count;
		text.Split(' ');
	}

	public void GenerateFormattedSentence()
	{
		EnsureCorpusLoaded(BookCorpus);
		Stat.ReseedFrom(BookSeed + Hash.String("Sentence"));
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append(MarkovChain.GenerateSentence(CorpusData[BookCorpus]));
		stringBuilder.TrimEnd();
		Sentence = Event.FinalizeString(stringBuilder);
	}

	[WishCommand("markovsultansentence", null)]
	public static void WishSultanMarkov()
	{
		string[] array = new string[5];
		for (int i = 0; i < 5; i++)
		{
			string state = "*Sultan" + (i + 1) + "Name*";
			array[i] = The.Game.GetStringGameState(state);
		}
		for (int j = 0; j < 10000; j++)
		{
			string sentence = GameText.GenerateMarkovMessageSentence();
			if (array.Any((string x) => sentence.Contains(x)))
			{
				Popup.Show(sentence);
				break;
			}
		}
	}
}
