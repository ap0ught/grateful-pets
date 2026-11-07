using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using XRL.Language;
using XRL.UI;
using XRL.Wish;

namespace XRL.World.Parts;

[HasWishCommand]
public abstract class IZoneLandmark : IPart
{
	public int OwnerID;

	public string OwnerBlueprint;

	public string Preposition;

	public bool Indefinite;

	public abstract string GetDisplayName();

	public abstract bool IsApplicable(int X, int Y);

	public static IZoneLandmark GetBestFor(Cell Cell)
	{
		return GetBestFor(Cell.ParentZone, Cell.X, Cell.Y);
	}

	public static IZoneLandmark GetBestFor(Zone Zone, int X, int Y)
	{
		Zone.ObjectEnumerator enumerator = Zone.IterateObjects().GetEnumerator();
		while (enumerator.MoveNext())
		{
			IZoneLandmark partDescendedFrom = enumerator.Current.GetPartDescendedFrom<IZoneLandmark>();
			if (partDescendedFrom != null && partDescendedFrom.IsApplicable(X, Y))
			{
				return partDescendedFrom;
			}
		}
		return null;
	}

	public virtual void Append(StringBuilder Builder, GameObject Object = null)
	{
		Zone currentZone = ParentObject.CurrentZone;
		string displayName = GetDisplayName();
		if (!Preposition.IsNullOrEmpty())
		{
			Builder.Compound(Preposition);
		}
		if (Object != null && (OwnerBlueprint == Object.Blueprint || (Object.HasID && OwnerID == Object.BaseID)))
		{
			Builder.Compound(Object.its);
		}
		else
		{
			GameObject gameObject = null;
			if (OwnerID != 0)
			{
				gameObject = currentZone.FindObjectByID(OwnerID);
			}
			if (gameObject == null && !OwnerBlueprint.IsNullOrEmpty())
			{
				gameObject = currentZone.FindObject(OwnerBlueprint);
			}
			if (gameObject != null)
			{
				Builder.Compound(Grammar.MakePossessive(gameObject.GetReferenceDisplayName(int.MaxValue, null, null, NoColor: false, Stripped: true, ColorOnly: false, WithoutTitles: true, Short: true)));
			}
			else if (Indefinite)
			{
				Builder.Compound(Grammar.IndefiniteArticleShouldBeAn(displayName) ? "an" : "a");
			}
			else
			{
				Builder.Compound("the");
			}
		}
		Builder.Compound(displayName);
	}

	[WishCommand("showlandmarks", null)]
	public static void WishRender()
	{
		Zone activeZone = The.ActiveZone;
		List<IZoneLandmark> list = new List<IZoneLandmark>();
		Zone.ObjectEnumerator enumerator = activeZone.IterateObjects().GetEnumerator();
		while (enumerator.MoveNext())
		{
			IZoneLandmark partDescendedFrom = enumerator.Current.GetPartDescendedFrom<IZoneLandmark>();
			if (partDescendedFrom != null)
			{
				list.Add(partDescendedFrom);
			}
		}
		if (list.IsNullOrEmpty())
		{
			return;
		}
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
		scrapBuffer.RenderBase();
		int i = 0;
		int count = list.Count;
		for (; i < activeZone.Height; i++)
		{
			for (int j = 0; j < activeZone.Width; j++)
			{
				for (int k = 0; k < count; k++)
				{
					if (list[k].IsApplicable(j, i))
					{
						ConsoleChar consoleChar = scrapBuffer[j, i];
						consoleChar.Char = (char)(k + 48);
						consoleChar.Foreground = The.Color.Red;
						consoleChar.Background = The.Color.Green;
						consoleChar.Detail = The.Color.Blue;
					}
				}
			}
		}
		scrapBuffer.Draw();
		Keyboard.getch();
	}

	[WishCommand("landmark", null)]
	public static void WishCurrent()
	{
		IZoneLandmark bestFor = GetBestFor(The.PlayerCell);
		if (bestFor == null)
		{
			Popup.ShowFail("You are nut currently in a landmark location.");
			return;
		}
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("You are");
		bestFor.Append(stringBuilder, The.Player);
		stringBuilder.Append('.');
		Popup.Show(Event.FinalizeString(stringBuilder));
	}
}
