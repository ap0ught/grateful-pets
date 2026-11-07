using System;
using System.Collections.Generic;
using Qud.API;

namespace XRL.World.Parts;

[Serializable]
public class RevealSecretOnRead : IPart
{
	public List<string> SecretID;

	public string Secrets
	{
		set
		{
			if (SecretID == null)
			{
				SecretID = new List<string>();
			}
			if (!value.Contains(','))
			{
				SecretID.Add(value);
				return;
			}
			DelimitedEnumeratorChar enumerator = value.DelimitedBy(',').GetEnumerator();
			while (enumerator.MoveNext())
			{
				ReadOnlySpan<char> current = enumerator.Current;
				SecretID.Add(new string(current));
			}
		}
	}

	public override bool WantEvent(int ID, int Cascade)
	{
		return ID == PooledEvent<AfterReadBookEvent>.ID;
	}

	public override bool HandleEvent(AfterReadBookEvent E)
	{
		if (!SecretID.IsNullOrEmpty())
		{
			foreach (string item in SecretID)
			{
				if (JournalAPI.NotesByID.TryGetValue(item, out var Value) && !Value.Revealed)
				{
					Value.Reveal();
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		int num = SecretID?.Count ?? 0;
		Writer.WriteOptimized(num);
		for (int i = 0; i < num; i++)
		{
			Writer.WriteOptimized(SecretID[i]);
		}
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		int num = Reader.ReadOptimizedInt32();
		if (num > 0)
		{
			SecretID = new List<string>(num);
			for (int i = 0; i < num; i++)
			{
				SecretID.Add(Reader.ReadOptimizedString());
			}
		}
	}
}
