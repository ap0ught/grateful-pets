using System;

namespace XRL.World.Parts;

[Serializable]
public class ReplaceText : IPart
{
	public string Variables;

	public string Replacements;

	public bool ReplaceInDisplayName = true;

	public bool ReplaceInDescription = true;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AfterObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AfterObjectCreatedEvent E)
	{
		Replace();
		return base.HandleEvent(E);
	}

	public void Replace()
	{
		string[] array = Variables.Split(',');
		string[] array2 = (Replacements.IsNullOrEmpty() ? null : Replacements.Split(','));
		if (ReplaceInDisplayName)
		{
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i];
				if (The.Game.HasStringGameState(text))
				{
					ParentObject.Render.DisplayName = ParentObject.Render.DisplayName.Replace(text, The.Game.GetStringGameState(text));
				}
				else if (array2 != null && array2.Length >= i)
				{
					ParentObject.Render.DisplayName = ParentObject.Render.DisplayName.Replace(text, array2[i]);
				}
			}
		}
		if (!ReplaceInDescription || !ParentObject.TryGetPart<Description>(out var Part))
		{
			return;
		}
		for (int j = 0; j < array.Length; j++)
		{
			string text2 = array[j];
			if (The.Game.HasStringGameState(text2))
			{
				Part._Short = Part._Short.Replace(text2, The.Game.GetStringGameState(text2));
			}
			else if (array2 != null && array2.Length >= j)
			{
				Part._Short = Part._Short.Replace(text2, array2[j]);
			}
		}
	}
}
