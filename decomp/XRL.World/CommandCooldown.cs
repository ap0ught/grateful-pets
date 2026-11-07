using System.CodeDom.Compiler;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

/// <summary>
/// Tracks ongoing cooldowns for commands, even should the source ability be lost and regained.
/// </summary>
[GenerateSerializationPartial]
public class CommandCooldown : ITokenized, IComposite
{
	public string Command;

	public int Segments;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual bool WantFieldReflection => false;

	public int Token { get; set; }

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(Command);
		Writer.WriteOptimized(Segments);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public virtual void Read(SerializationReader Reader)
	{
		Command = Reader.ReadOptimizedString();
		Segments = Reader.ReadOptimizedInt32();
	}

	public CommandCooldown()
	{
	}

	public CommandCooldown(CommandCooldown Source)
	{
		Command = Source.Command;
		Segments = Source.Segments;
	}

	public CommandCooldown(string Command, int Segments = 0)
	{
		this.Command = Command;
		this.Segments = Segments;
	}
}
