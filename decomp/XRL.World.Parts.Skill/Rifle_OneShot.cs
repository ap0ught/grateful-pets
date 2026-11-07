using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Rifle_OneShot : BaseSkill
{
	public const string CMD_ID = "CommandRifleOneShot";

	private CommandCooldown CommandCooldown;

	public int Cooldown
	{
		get
		{
			return CommandCooldown.Segments;
		}
		set
		{
			if (CommandCooldown == null)
			{
				CommandCooldown = new CommandCooldown("CommandRifleOneShot");
			}
			if (value > 0 && CommandCooldown.Segments <= 0)
			{
				ParentObject.Abilities.AddCooldown(CommandCooldown);
			}
			else if (value <= 0 && CommandCooldown.Segments > 0)
			{
				ParentObject.Abilities.RemoveCooldown(CommandCooldown);
			}
			CommandCooldown.Segments = value;
		}
	}

	public override void Initialize()
	{
		if (CommandCooldown == null)
		{
			CommandCooldown = new CommandCooldown("CommandRifleOneShot");
		}
	}

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		base.Write(Basis, Writer);
		Writer.WriteTokenized(CommandCooldown);
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		base.Read(Basis, Reader);
		CommandCooldown = (CommandCooldown)Reader.ReadTokenized();
	}
}
