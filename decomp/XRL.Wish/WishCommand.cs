using System;

namespace XRL.Wish;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class WishCommand : Attribute
{
	public string Command;

	public string Regex;

	public WishCommand(string Command = null, string Regex = null)
	{
		this.Command = Command;
		this.Regex = Regex;
	}
}
