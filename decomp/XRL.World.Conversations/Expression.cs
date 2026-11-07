using System;
using System.Collections.Generic;
using XRL.Collections;

namespace XRL.World.Conversations;

public static class Expression
{
	public class Token
	{
		public static readonly Token OPEN = new Token(TokenType.OpenGroup);

		public static readonly Token CLOSE = new Token(TokenType.CloseGroup);

		public static readonly Token AND = new Token(TokenType.And);

		public static readonly Token OR = new Token(TokenType.Or);

		public static readonly Token NOT = new Token(TokenType.Not);

		private static RingDeque<Token> Output = new RingDeque<Token>(16);

		private static RingDeque<Token> Operators = new RingDeque<Token>(16);

		private static StringMap<Token> Literals = new StringMap<Token>();

		public TokenType Type;

		public byte Precedence => (byte)(((int)Type >> 4) & 7);

		public bool LeftAssociative => (Type & (TokenType)0x80) == 0;

		public bool RightAssociative => (int)(Type & (TokenType)0x80) > 0;

		public override string ToString()
		{
			return Type switch
			{
				TokenType.And => "AND", 
				TokenType.Or => "OR", 
				TokenType.Not => "NOT", 
				TokenType.OpenGroup => "(", 
				TokenType.CloseGroup => ")", 
				_ => "", 
			};
		}

		public Token(TokenType Type)
		{
			this.Type = Type;
		}

		public static bool TryGetLiteral(string Value, int Start, int Length, out Token Token)
		{
			while (Length > 0 && Value[Start] == ' ')
			{
				Start++;
				Length--;
			}
			while (Length > 0 && Value[Start + Length - 1] == ' ')
			{
				Length--;
			}
			if (Length <= 0)
			{
				Token = null;
				return false;
			}
			Token = GetLiteral(Value.AsSpan(Start, Length));
			return true;
		}

		public static Token GetLiteral(ReadOnlySpan<char> Key)
		{
			if (!Literals.TryGetValue(Key, out var Value))
			{
				string text = new string(Key);
				Value = (Literals[text] = new LiteralToken(text));
			}
			return Value;
		}

		public static List<Token> Tokenize(string Value)
		{
			RingDeque<Token> output = Output;
			int num = 0;
			int num2 = Value.Length - 1;
			Token nOT = NOT;
			int i = 0;
			int num3 = 0;
			for (; i <= num2; i++)
			{
				char c = Value[i];
				if (c == 'N' && i + 3 <= num2 && Value[i + 1] == 'O' && Value[i + 2] == 'T' && Value[i + 3] == ' ' && (i == 0 || Value[i - 1] == ' ' || Value[i - 1] == '('))
				{
					nOT = NOT;
					num3 = 3;
				}
				else if (c == '(')
				{
					nOT = OPEN;
				}
				else if (c == ')')
				{
					nOT = CLOSE;
				}
				else
				{
					if (c != ' ' || i + 4 > num2)
					{
						continue;
					}
					if (Value[i + 1] == 'A' && Value[i + 2] == 'N' && Value[i + 3] == 'D' && Value[i + 4] == ' ')
					{
						nOT = AND;
						num3 = 4;
					}
					else
					{
						if (Value[i + 1] != 'O' || Value[i + 2] != 'R' || Value[i + 3] != ' ')
						{
							continue;
						}
						nOT = OR;
						num3 = 3;
					}
				}
				if (TryGetLiteral(Value, num, i - num, out var Token))
				{
					output.Enqueue(Token);
				}
				output.Enqueue(nOT);
				i += num3;
				num = i + 1;
				num3 = 0;
			}
			if (num == 0)
			{
				return null;
			}
			if (TryGetLiteral(Value, num, num2 - num + 1, out var Token2))
			{
				output.Enqueue(Token2);
			}
			int count = output.Count;
			List<Token> list = new List<Token>(count);
			for (int j = 0; j < count; j++)
			{
				list.Add(output.Dequeue());
			}
			return list;
		}

		public static void Shunt(List<Token> Value)
		{
			RingDeque<Token> output = Output;
			RingDeque<Token> operators = Operators;
			output.Clear();
			operators.Clear();
			for (int num = Value.Count - 1; num >= 0; num--)
			{
				Token token = Value[num];
				switch (token.Type)
				{
				case TokenType.Literal:
					output.Enqueue(token);
					break;
				case TokenType.And:
				case TokenType.Or:
				case TokenType.Not:
					while (operators.Count > 0 && operators.Last.Type != TokenType.CloseGroup && (operators.Last.Precedence < token.Precedence || (operators.Last.Precedence == token.Precedence && token.RightAssociative)))
					{
						output.Enqueue(operators.Eject());
					}
					operators.Enqueue(token);
					break;
				case TokenType.CloseGroup:
					operators.Enqueue(token);
					break;
				case TokenType.OpenGroup:
				{
					bool flag = false;
					Token Value2;
					while (operators.TryEject(out Value2))
					{
						if (Value2.Type == TokenType.CloseGroup)
						{
							flag = true;
							break;
						}
						output.Enqueue(Value2);
					}
					if (!flag)
					{
						throw new ArgumentException("Mismatched parentheses");
					}
					break;
				}
				}
			}
			Token Value3;
			while (operators.TryEject(out Value3))
			{
				output.Enqueue(Value3);
			}
			Value.Clear();
			Token Value4;
			while (output.TryEject(out Value4))
			{
				Value.Add(Value4);
			}
		}
	}

	public class LiteralToken : Token
	{
		public string Value;

		public LiteralToken(string Value)
			: base(TokenType.Literal)
		{
			this.Value = Value;
		}

		public override string ToString()
		{
			return Value;
		}
	}

	public abstract class Node : IDisposable
	{
		public abstract bool Eval();

		public abstract void Dispose();
	}

	public class PredicateNode : Node
	{
		private static RingDeque<PredicateNode> Pool = new RingDeque<PredicateNode>();

		public IConversationElement Element;

		public PredicateReceiver Receiver;

		public string Value;

		public static PredicateNode Get(IConversationElement Element, PredicateReceiver Receiver, string Value)
		{
			if (!Pool.TryDequeue(out var Value2))
			{
				Value2 = new PredicateNode();
			}
			Value2.Element = Element;
			Value2.Receiver = Receiver;
			Value2.Value = Value;
			return Value2;
		}

		public override bool Eval()
		{
			return Receiver(Element, Value);
		}

		public override void Dispose()
		{
			Value = null;
			Pool.Enqueue(this);
		}
	}

	public class NotNode : Node
	{
		private static RingDeque<NotNode> Pool = new RingDeque<NotNode>();

		public Node Operand;

		public static NotNode Get(Node Operand)
		{
			if (!Pool.TryDequeue(out var Value))
			{
				Value = new NotNode();
			}
			Value.Operand = Operand;
			return Value;
		}

		public override bool Eval()
		{
			return !Operand.Eval();
		}

		public override void Dispose()
		{
			Operand.Dispose();
			Operand = null;
			Pool.Enqueue(this);
		}
	}

	public class AndNode : Node
	{
		private static RingDeque<AndNode> Pool = new RingDeque<AndNode>();

		public Node Left;

		public Node Right;

		public static AndNode Get(Node Left, Node Right)
		{
			if (!Pool.TryDequeue(out var Value))
			{
				Value = new AndNode();
			}
			Value.Left = Left;
			Value.Right = Right;
			return Value;
		}

		public override bool Eval()
		{
			if (Left.Eval())
			{
				return Right.Eval();
			}
			return false;
		}

		public override void Dispose()
		{
			Left.Dispose();
			Left = null;
			Right.Dispose();
			Right = null;
			Pool.Enqueue(this);
		}
	}

	public class OrNode : Node
	{
		private static RingDeque<OrNode> Pool = new RingDeque<OrNode>();

		public Node Left;

		public Node Right;

		public static OrNode Get(Node Left, Node Right)
		{
			if (!Pool.TryDequeue(out var Value))
			{
				Value = new OrNode();
			}
			Value.Left = Left;
			Value.Right = Right;
			return Value;
		}

		public override bool Eval()
		{
			if (!Left.Eval())
			{
				return Right.Eval();
			}
			return true;
		}

		public override void Dispose()
		{
			Left.Dispose();
			Left = null;
			Right.Dispose();
			Right = null;
			Pool.Enqueue(this);
		}
	}

	public const byte PREC_SHIFT = 4;

	public const byte PREC_MASK = 7;

	public const byte RIGHT_ASSOC = 128;

	public static Node CreateTree(List<Token> Tokens, IConversationElement Element, PredicateReceiver Receiver)
	{
		int Index = 0;
		return CreateNode(Tokens, Element, Receiver, ref Index);
	}

	private static Node CreateNode(List<Token> Tokens, IConversationElement Element, PredicateReceiver Receiver, ref int Index)
	{
		if (Index >= Tokens.Count)
		{
			return null;
		}
		Token token = Tokens[Index++];
		if (token.Type == TokenType.Not)
		{
			return NotNode.Get(CreateNode(Tokens, Element, Receiver, ref Index));
		}
		if (token.Type == TokenType.And)
		{
			Node left = CreateNode(Tokens, Element, Receiver, ref Index);
			Node right = CreateNode(Tokens, Element, Receiver, ref Index);
			return AndNode.Get(left, right);
		}
		if (token.Type == TokenType.Or)
		{
			Node left2 = CreateNode(Tokens, Element, Receiver, ref Index);
			Node right2 = CreateNode(Tokens, Element, Receiver, ref Index);
			return OrNode.Get(left2, right2);
		}
		return PredicateNode.Get(Element, Receiver, token.ToString());
	}

	public static List<Token> Tokenize(string Value)
	{
		return Token.Tokenize(Value);
	}

	public static void Shunt(List<Token> Value)
	{
		Token.Shunt(Value);
	}

	public static bool? Evaluate(string Value, IConversationElement Element, PredicateReceiver Receiver)
	{
		List<Token> list = Tokenize(Value);
		if (list.IsNullOrEmpty())
		{
			return null;
		}
		Shunt(list);
		using Node node = CreateTree(list, Element, Receiver);
		return node.Eval();
	}

	public static bool? Evaluate(string Value, IConversationElement Element, ActionReceiver Receiver)
	{
		List<Token> list = Tokenize(Value);
		if (list.IsNullOrEmpty())
		{
			return null;
		}
		foreach (Token item in list)
		{
			if (item.Type == TokenType.Literal && item is LiteralToken literalToken)
			{
				Receiver(Element, literalToken.Value);
			}
		}
		return true;
	}
}
