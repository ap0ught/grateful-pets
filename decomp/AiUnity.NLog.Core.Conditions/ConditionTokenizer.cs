using System;
using System.Text;
using AiUnity.NLog.Core.Internal;

namespace AiUnity.NLog.Core.Conditions;

internal sealed class ConditionTokenizer
{
	private struct CharToTokenType(char character, ConditionTokenType tokenType)
	{
		public readonly char Character = character;

		public readonly ConditionTokenType TokenType = tokenType;
	}

	private static readonly ConditionTokenType[] charIndexToTokenType = BuildCharIndexToTokenType();

	private readonly SimpleStringReader stringReader;

	public int TokenPosition { get; private set; }

	public ConditionTokenType TokenType { get; private set; }

	public string TokenValue { get; private set; }

	public string StringTokenValue
	{
		get
		{
			string tokenValue = TokenValue;
			return tokenValue.Substring(1, tokenValue.Length - 2).Replace("''", "'");
		}
	}

	public ConditionTokenizer(SimpleStringReader stringReader)
	{
		this.stringReader = stringReader;
		TokenType = ConditionTokenType.BeginningOfInput;
		GetNextToken();
	}

	public void Expect(ConditionTokenType tokenType)
	{
		if (TokenType != tokenType)
		{
			throw new ConditionParseException("Expected token of type: " + tokenType.ToString() + ", got " + TokenType.ToString() + " (" + TokenValue + ").");
		}
		GetNextToken();
	}

	public string EatKeyword()
	{
		if (TokenType != ConditionTokenType.Keyword)
		{
			throw new ConditionParseException("Identifier expected");
		}
		string tokenValue = TokenValue;
		GetNextToken();
		return tokenValue;
	}

	public bool IsKeyword(string keyword)
	{
		if (TokenType != ConditionTokenType.Keyword)
		{
			return false;
		}
		if (!TokenValue.Equals(keyword, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		return true;
	}

	public bool IsEOF()
	{
		if (TokenType != ConditionTokenType.EndOfInput)
		{
			return false;
		}
		return true;
	}

	public bool IsNumber()
	{
		return TokenType == ConditionTokenType.Number;
	}

	public bool IsToken(ConditionTokenType tokenType)
	{
		return TokenType == tokenType;
	}

	public void GetNextToken()
	{
		if (TokenType == ConditionTokenType.EndOfInput)
		{
			throw new ConditionParseException("Cannot read past end of stream.");
		}
		SkipWhitespace();
		TokenPosition = TokenPosition;
		int num = PeekChar();
		if (num == -1)
		{
			TokenType = ConditionTokenType.EndOfInput;
			return;
		}
		char c = (char)num;
		if (char.IsDigit(c))
		{
			ParseNumber(c);
			return;
		}
		switch (c)
		{
		case '\'':
			ParseSingleQuotedString(c);
			break;
		default:
			if (!char.IsLetter(c))
			{
				if (c == '}' || c == ':')
				{
					TokenType = ConditionTokenType.EndOfInput;
					break;
				}
				TokenValue = c.ToString();
				if (c == '<')
				{
					ReadChar();
					switch (PeekChar())
					{
					case 62:
						TokenType = ConditionTokenType.NotEqual;
						TokenValue = "<>";
						ReadChar();
						break;
					case 61:
						TokenType = ConditionTokenType.LessThanOrEqualTo;
						TokenValue = "<=";
						ReadChar();
						break;
					default:
						TokenType = ConditionTokenType.LessThan;
						TokenValue = "<";
						break;
					}
					break;
				}
				if (c == '>')
				{
					ReadChar();
					if (PeekChar() == 61)
					{
						TokenType = ConditionTokenType.GreaterThanOrEqualTo;
						TokenValue = ">=";
						ReadChar();
					}
					else
					{
						TokenType = ConditionTokenType.GreaterThan;
						TokenValue = ">";
					}
					break;
				}
				if (c == '!')
				{
					ReadChar();
					if (PeekChar() == 61)
					{
						TokenType = ConditionTokenType.NotEqual;
						TokenValue = "!=";
						ReadChar();
					}
					else
					{
						TokenType = ConditionTokenType.Not;
						TokenValue = "!";
					}
					break;
				}
				if (c == '&')
				{
					ReadChar();
					if (PeekChar() == 38)
					{
						TokenType = ConditionTokenType.And;
						TokenValue = "&&";
						ReadChar();
						break;
					}
					throw new ConditionParseException("Expected '&&' but got '&'");
				}
				if (c == '|')
				{
					ReadChar();
					if (PeekChar() == 124)
					{
						TokenType = ConditionTokenType.Or;
						TokenValue = "||";
						ReadChar();
						break;
					}
					throw new ConditionParseException("Expected '||' but got '|'");
				}
				switch (c)
				{
				case '=':
					ReadChar();
					if (PeekChar() == 61)
					{
						TokenType = ConditionTokenType.EqualTo;
						TokenValue = "==";
						ReadChar();
					}
					else
					{
						TokenType = ConditionTokenType.EqualTo;
						TokenValue = "=";
					}
					break;
				case ' ':
				case '!':
				case '"':
				case '#':
				case '$':
				case '%':
				case '&':
				case '\'':
				case '(':
				case ')':
				case '*':
				case '+':
				case ',':
				case '-':
				case '.':
				case '/':
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
				case ':':
				case ';':
				case '<':
				case '>':
				case '?':
				case '@':
				case 'A':
				case 'B':
				case 'C':
				case 'D':
				case 'E':
				case 'F':
				case 'G':
				case 'H':
				case 'I':
				case 'J':
				case 'K':
				case 'L':
				case 'M':
				case 'N':
				case 'O':
				case 'P':
				case 'Q':
				case 'R':
				case 'S':
				case 'T':
				case 'U':
				case 'V':
				case 'W':
				case 'X':
				case 'Y':
				case 'Z':
				case '[':
				case '\\':
				case ']':
				case '^':
				case '_':
				case '`':
				case 'a':
				case 'b':
				case 'c':
				case 'd':
				case 'e':
				case 'f':
				case 'g':
				case 'h':
				case 'i':
				case 'j':
				case 'k':
				case 'l':
				case 'm':
				case 'n':
				case 'o':
				case 'p':
				case 'q':
				case 'r':
				case 's':
				case 't':
				case 'u':
				case 'v':
				case 'w':
				case 'x':
				case 'y':
				case 'z':
				case '{':
				case '|':
				case '}':
				case '~':
				case '\u007f':
				{
					ConditionTokenType conditionTokenType = charIndexToTokenType[(uint)c];
					if (conditionTokenType != ConditionTokenType.Invalid)
					{
						TokenType = conditionTokenType;
						TokenValue = new string(c, 1);
						ReadChar();
						break;
					}
					throw new ConditionParseException("Invalid punctuation: " + c);
				}
				default:
					throw new ConditionParseException("Invalid token: " + c);
				}
				break;
			}
			goto case '_';
		case '_':
			ParseKeyword(c);
			break;
		}
	}

	private static ConditionTokenType[] BuildCharIndexToTokenType()
	{
		CharToTokenType[] array = new CharToTokenType[6]
		{
			new CharToTokenType('(', ConditionTokenType.LeftParen),
			new CharToTokenType(')', ConditionTokenType.RightParen),
			new CharToTokenType('.', ConditionTokenType.Dot),
			new CharToTokenType(',', ConditionTokenType.Comma),
			new CharToTokenType('!', ConditionTokenType.Not),
			new CharToTokenType('-', ConditionTokenType.Minus)
		};
		ConditionTokenType[] array2 = new ConditionTokenType[128];
		for (int i = 0; i < 128; i++)
		{
			array2[i] = ConditionTokenType.Invalid;
		}
		CharToTokenType[] array3 = array;
		for (int j = 0; j < array3.Length; j++)
		{
			CharToTokenType charToTokenType = array3[j];
			array2[(uint)charToTokenType.Character] = charToTokenType.TokenType;
		}
		return array2;
	}

	private void ParseSingleQuotedString(char ch)
	{
		TokenType = ConditionTokenType.String;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(ch);
		ReadChar();
		int num;
		while ((num = PeekChar()) != -1)
		{
			ch = (char)num;
			stringBuilder.Append((char)ReadChar());
			if (ch == '\'')
			{
				if (PeekChar() != 39)
				{
					break;
				}
				stringBuilder.Append('\'');
				ReadChar();
			}
		}
		if (num == -1)
		{
			throw new ConditionParseException("String literal is missing a closing quote character.");
		}
		TokenValue = stringBuilder.ToString();
	}

	private void ParseKeyword(char ch)
	{
		TokenType = ConditionTokenType.Keyword;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(ch);
		ReadChar();
		int num;
		while ((num = PeekChar()) != -1 && ((ushort)num == 95 || (ushort)num == 45 || char.IsLetterOrDigit((char)num)))
		{
			stringBuilder.Append((char)ReadChar());
		}
		TokenValue = stringBuilder.ToString();
	}

	private void ParseNumber(char ch)
	{
		TokenType = ConditionTokenType.Number;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(ch);
		ReadChar();
		int num;
		while ((num = PeekChar()) != -1)
		{
			ch = (char)num;
			if (!char.IsDigit(ch) && ch != '.')
			{
				break;
			}
			stringBuilder.Append((char)ReadChar());
		}
		TokenValue = stringBuilder.ToString();
	}

	private void SkipWhitespace()
	{
		int num;
		while ((num = PeekChar()) != -1 && char.IsWhiteSpace((char)num))
		{
			ReadChar();
		}
	}

	private int PeekChar()
	{
		return stringReader.Peek();
	}

	private int ReadChar()
	{
		return stringReader.Read();
	}
}
