using System;

namespace XRL.World;

[Serializable]
public class QuestStep
{
	public const int FLAG_BASE = 1;

	public const int FLAG_FINISHED = 2;

	public const int FLAG_FAILED = 4;

	public const int FLAG_COLLAPSE = 8;

	public const int FLAG_HIDDEN = 16;

	public const int FLAG_AWARDED = 32;

	public const int FLAG_OPTIONAL = 64;

	public string ID;

	public string Name;

	public string Text;

	public string Value;

	public int XP;

	/// <summary>The display order in the UI.</summary>
	public int Ordinal;

	public int Flags = 8;

	/// <summary>This step is not inherited by the active quest and only exists in the blueprint.</summary>
	public bool Base
	{
		get
		{
			return Flags.HasBit(1);
		}
		set
		{
			Flags.SetBit(1, value);
		}
	}

	/// <summary>The completion state of this quest step, renders with a green checkmark in the UI.</summary>
	public bool Finished
	{
		get
		{
			return Flags.HasBit(2);
		}
		set
		{
			Flags.SetBit(2, value);
		}
	}

	/// <summary>This step has awarded its associated XP and will silently complete without awarding experience if re-completed.</summary>
	public bool Awarded
	{
		get
		{
			return Flags.HasBit(32);
		}
		set
		{
			Flags.SetBit(32, value);
		}
	}

	/// <summary>Whether this quest step has been failed, renders with a red cross in the UI.</summary>
	public bool Failed
	{
		get
		{
			return Flags.HasBit(4);
		}
		set
		{
			Flags.SetBit(4, value);
		}
	}

	/// <summary>Collapse the text body of this step in the UI when finished.</summary>
	public bool Collapse
	{
		get
		{
			return Flags.HasBit(8);
		}
		set
		{
			Flags.SetBit(8, value);
		}
	}

	/// <summary>This step is hidden in the UI until revealed manually.</summary>
	public bool Hidden
	{
		get
		{
			return Flags.HasBit(16);
		}
		set
		{
			Flags.SetBit(16, value);
		}
	}

	/// <summary>This step is optional and won't prevent the quest as a whole from being completing.</summary>
	public bool Optional
	{
		get
		{
			return Flags.HasBit(64);
		}
		set
		{
			Flags.SetBit(64, value);
		}
	}

	public QuestStep()
	{
	}

	public QuestStep(SerializationReader Reader)
		: this()
	{
		Load(Reader);
	}

	public override string ToString()
	{
		return ID + " n=" + Name + " t=" + Text + " xp=" + XP + " finished=" + Finished;
	}

	public void Save(SerializationWriter Writer)
	{
		Writer.Write(ID);
		Writer.Write(Name);
		Writer.Write(Text);
		Writer.Write(Value);
		Writer.WriteOptimized(XP);
		Writer.WriteOptimized(Ordinal);
		Writer.WriteOptimized(Flags);
	}

	public void Load(SerializationReader Reader)
	{
		ID = Reader.ReadString();
		Name = Reader.ReadString();
		Text = Reader.ReadString();
		Value = Reader.ReadString();
		XP = Reader.ReadOptimizedInt32();
		Ordinal = Reader.ReadOptimizedInt32();
		Flags = Reader.ReadOptimizedInt32();
	}
}
