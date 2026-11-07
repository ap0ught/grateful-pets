using System;
using System.Collections.Generic;
using Qud.API;

namespace XRL.World.Units;

[Serializable]
public class GameObjectSecretUnit : GameObjectUnit
{
	public int Amount;

	public override void Apply(GameObject Object)
	{
		List<IBaseJournalEntry> unrevealedNotes = JournalAPI.GetUnrevealedNotes();
		for (int i = 0; i < Amount; i++)
		{
			unrevealedNotes.RemoveRandomElement()?.Reveal();
		}
	}

	public override void Reset()
	{
		base.Reset();
		Amount = 0;
	}

	public override string GetDescription(bool Inscription = false)
	{
		return $"Reveals {Amount} secrets on creation";
	}
}
