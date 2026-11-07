using System.Collections.Generic;
using NUnit.Framework;
using XRL.World.Conversations;

namespace XRL.Tests;

public class ConversationTests
{
	[TestCase("NOT (A AND B OR C) AND D OR E OR (G AND H)", "OR OR AND NOT OR AND A B C D E AND G H")]
	[TestCase("(A AND (B OR (C AND (D OR E))))", "AND A OR B AND C OR D E")]
	[TestCase("NOT (NOT A AND B OR NOT C) OR NOT NOT (NOT D AND E) AND G OR H", "OR OR NOT OR AND NOT A B NOT C AND NOT NOT AND NOT D E G H")]
	[TestCase("zANDz OR ORz AND zNOT OR NOT ANDz", "OR OR zANDz AND ORz zNOT NOT ANDz")]
	public void Shunting(string Infix, string Prefix)
	{
		List<Expression.Token> list = Expression.Tokenize(Infix);
		Expression.Shunt(list);
		Assert.AreEqual(Prefix, string.Join(' ', list));
	}
}
