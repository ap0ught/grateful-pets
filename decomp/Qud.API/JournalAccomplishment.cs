using System;
using ConsoleLib.Console;
using XRL;
using XRL.World;

namespace Qud.API;

[Serializable]
public class JournalAccomplishment : IBaseJournalEntry
{
	public long Time;

	public string Category;

	public string MuralText;

	public string GospelText;

	public string AggregateWith;

	public MuralCategory MuralCategory;

	public MuralWeight MuralWeight;

	public SnapshotRenderable[] Screenshot;

	public const int SCREENSHOT_WIDTH = 9;

	public const int SCREENSHOT_HEIGHT = 5;

	public override bool WantFieldReflection => false;

	[Obsolete]
	public string category
	{
		get
		{
			return Category;
		}
		set
		{
			Category = value;
		}
	}

	[Obsolete]
	public long time
	{
		get
		{
			return Time;
		}
		set
		{
			Time = value;
		}
	}

	[Obsolete]
	public string muralText
	{
		get
		{
			return MuralText;
		}
		set
		{
			MuralText = value;
		}
	}

	[Obsolete]
	public MuralCategory muralCategory
	{
		get
		{
			return MuralCategory;
		}
		set
		{
			MuralCategory = value;
		}
	}

	[Obsolete]
	public MuralWeight muralWeight
	{
		get
		{
			return MuralWeight;
		}
		set
		{
			MuralWeight = value;
		}
	}

	public void SetScreenshot(int x, int y, IRenderable renderable)
	{
		if (x == 5)
		{
			_ = 2;
		}
		if (Screenshot == null)
		{
			Screenshot = new SnapshotRenderable[45];
		}
		Screenshot[x + y * 9] = new SnapshotRenderable(renderable);
	}

	public void UpdateScreenshot()
	{
		if (The.Player.IsNowhere())
		{
			return;
		}
		int num = Math.Clamp(The.Player.CurrentCell.X - 4, 0, The.Player.CurrentZone.Width - 9);
		int num2 = Math.Clamp(The.Player.CurrentCell.Y - 2, 0, The.Player.CurrentZone.Height - 5);
		using RenderEvent renderEvent = RenderEvent.Pool.Get();
		Zone currentZone = The.Player.CurrentZone;
		for (int i = num2; i < num2 + 5; i++)
		{
			for (int j = num; j < num + 9; j++)
			{
				currentZone.GetCell(j, i).Render(renderEvent);
				SetScreenshot(j - num, i - num2, renderEvent);
			}
		}
	}

	public override void Write(SerializationWriter Writer)
	{
		base.Write(Writer);
		Writer.WriteOptimized(Time);
		Writer.WriteOptimized(Category);
		Writer.WriteOptimized(MuralText);
		Writer.WriteOptimized(GospelText);
		Writer.WriteOptimized(AggregateWith);
		Writer.WriteOptimized((int)MuralCategory);
		Writer.WriteOptimized((int)MuralWeight);
		int num = Screenshot?.Length ?? 0;
		Writer.WriteOptimized(num);
		for (int i = 0; i < num; i++)
		{
			Writer.WriteComposite(Screenshot[i]);
		}
	}

	public override void Read(SerializationReader Reader)
	{
		base.Read(Reader);
		Time = Reader.ReadOptimizedInt64();
		Category = Reader.ReadOptimizedString();
		MuralText = Reader.ReadOptimizedString();
		GospelText = Reader.ReadOptimizedString();
		AggregateWith = Reader.ReadOptimizedString();
		MuralCategory = (MuralCategory)Reader.ReadOptimizedInt32();
		MuralWeight = (MuralWeight)Reader.ReadOptimizedInt32();
		int num = Reader.ReadOptimizedInt32();
		if (num <= 0)
		{
			Screenshot = Array.Empty<SnapshotRenderable>();
			return;
		}
		Screenshot = new SnapshotRenderable[num];
		for (int i = 0; i < num; i++)
		{
			Screenshot[i] = Reader.ReadComposite<SnapshotRenderable>();
		}
	}
}
