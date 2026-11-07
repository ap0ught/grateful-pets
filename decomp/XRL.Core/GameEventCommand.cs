using System.CodeDom.Compiler;
using Occult.Engine.CodeGeneration;
using XRL.Collections;
using XRL.World;

namespace XRL.Core;

[GenerateSerializationPartial]
public class GameEventCommand : IActionCommand, IComposite
{
	private static RingDeque<GameEventCommand> Pool = new RingDeque<GameEventCommand>();

	public string Command;

	public string Type;

	public int Level;

	public int Flags;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual bool WantFieldReflection => false;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(Command);
		Writer.WriteOptimized(Type);
		Writer.WriteOptimized(Level);
		Writer.WriteOptimized(Flags);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual void Read(SerializationReader Reader)
	{
		Command = Reader.ReadOptimizedString();
		Type = Reader.ReadOptimizedString();
		Level = Reader.ReadOptimizedInt32();
		Flags = Reader.ReadOptimizedInt32();
	}

	public static void Issue(string Command, string Type = null, int Level = 0, int Flags = 0)
	{
		ActionManager actionManager = The.ActionManager;
		if (!Pool.TryEject(out var Value))
		{
			Value = new GameEventCommand();
		}
		Value.Command = Command;
		Value.Type = Type;
		Value.Level = Level;
		Value.Flags = Flags;
		actionManager.EnqueueAction(Value);
	}

	public void Execute(XRLGame Game, ActionManager Manager)
	{
		GenericCommandEvent.Send(Game, Command, Type, null, null, Level, Flags);
		Pool.Enqueue(this);
	}
}
