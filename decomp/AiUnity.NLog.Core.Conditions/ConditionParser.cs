using System;
using System.Collections.Generic;
using System.Reflection;
using AiUnity.Common.Extensions;
using AiUnity.Common.Log;
using AiUnity.NLog.Core.Config;
using AiUnity.NLog.Core.Internal;
using AiUnity.NLog.Core.Layouts;

namespace AiUnity.NLog.Core.Conditions;

public class ConditionParser
{
	private readonly ConditionTokenizer tokenizer;

	private readonly ConfigurationItemFactory configurationItemFactory;

	private ConditionParser(SimpleStringReader stringReader, ConfigurationItemFactory configurationItemFactory)
	{
		this.configurationItemFactory = configurationItemFactory;
		tokenizer = new ConditionTokenizer(stringReader);
	}

	public static ConditionExpression ParseExpression(string expressionText)
	{
		return ParseExpression(expressionText, ConfigurationItemFactory.Default);
	}

	public static ConditionExpression ParseExpression(string expressionText, ConfigurationItemFactory configurationItemFactories)
	{
		if (expressionText == null)
		{
			return null;
		}
		ConditionParser conditionParser = new ConditionParser(new SimpleStringReader(expressionText), configurationItemFactories);
		ConditionExpression result = conditionParser.ParseExpression();
		if (!conditionParser.tokenizer.IsEOF())
		{
			throw new ConditionParseException("Unexpected token: " + conditionParser.tokenizer.TokenValue);
		}
		return result;
	}

	internal static ConditionExpression ParseExpression(SimpleStringReader stringReader, ConfigurationItemFactory configurationItemFactories)
	{
		return new ConditionParser(stringReader, configurationItemFactories).ParseExpression();
	}

	private ConditionMethodExpression ParsePredicate(string functionName)
	{
		List<ConditionExpression> list = new List<ConditionExpression>();
		while (!tokenizer.IsEOF() && tokenizer.TokenType != ConditionTokenType.RightParen)
		{
			list.Add(ParseExpression());
			if (tokenizer.TokenType != ConditionTokenType.Comma)
			{
				break;
			}
			tokenizer.GetNextToken();
		}
		tokenizer.Expect(ConditionTokenType.RightParen);
		try
		{
			MethodInfo methodInfo = configurationItemFactory.ConditionMethods.CreateInstance(functionName);
			return new ConditionMethodExpression(functionName, methodInfo, list);
		}
		catch (Exception ex)
		{
			if (ex.MustBeRethrown())
			{
				throw;
			}
			throw new ConditionParseException("Cannot resolve function '" + functionName + "'", ex);
		}
	}

	private ConditionExpression ParseLiteralExpression()
	{
		if (tokenizer.IsToken(ConditionTokenType.LeftParen))
		{
			tokenizer.GetNextToken();
			ConditionExpression result = ParseExpression();
			tokenizer.Expect(ConditionTokenType.RightParen);
			return result;
		}
		if (tokenizer.IsToken(ConditionTokenType.Minus))
		{
			tokenizer.GetNextToken();
			if (!tokenizer.IsNumber())
			{
				throw new ConditionParseException("Number expected, got " + tokenizer.TokenType);
			}
			string tokenValue = tokenizer.TokenValue;
			tokenizer.GetNextToken();
			if (tokenValue.IndexOf('.') >= 0)
			{
				return new ConditionLiteralExpression(0.0 - double.Parse(tokenValue));
			}
			return new ConditionLiteralExpression(-int.Parse(tokenValue));
		}
		if (tokenizer.IsNumber())
		{
			string tokenValue2 = tokenizer.TokenValue;
			tokenizer.GetNextToken();
			if (tokenValue2.IndexOf('.') >= 0)
			{
				return new ConditionLiteralExpression(double.Parse(tokenValue2));
			}
			return new ConditionLiteralExpression(int.Parse(tokenValue2));
		}
		if (tokenizer.TokenType == ConditionTokenType.String)
		{
			ConditionLayoutExpression result2 = new ConditionLayoutExpression(Layout.FromString(tokenizer.StringTokenValue, configurationItemFactory));
			tokenizer.GetNextToken();
			return result2;
		}
		if (tokenizer.TokenType == ConditionTokenType.Keyword)
		{
			string text = tokenizer.EatKeyword();
			if (string.Compare(text, "level", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return new ConditionLevelExpression();
			}
			if (string.Compare(text, "logger", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return new ConditionLoggerNameExpression();
			}
			if (string.Compare(text, "message", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return new ConditionMessageExpression();
			}
			if (string.Compare(text, "loglevel", StringComparison.OrdinalIgnoreCase) == 0)
			{
				tokenizer.Expect(ConditionTokenType.Dot);
				return new ConditionLiteralExpression(tokenizer.EatKeyword().ToEnum((LogLevels)0));
			}
			if (string.Compare(text, "true", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return new ConditionLiteralExpression(true);
			}
			if (string.Compare(text, "false", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return new ConditionLiteralExpression(false);
			}
			if (string.Compare(text, "null", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return new ConditionLiteralExpression(null);
			}
			if (tokenizer.TokenType == ConditionTokenType.LeftParen)
			{
				tokenizer.GetNextToken();
				return ParsePredicate(text);
			}
		}
		throw new ConditionParseException("Unexpected token: " + tokenizer.TokenValue);
	}

	private ConditionExpression ParseBooleanRelation()
	{
		ConditionExpression conditionExpression = ParseLiteralExpression();
		if (tokenizer.IsToken(ConditionTokenType.EqualTo))
		{
			tokenizer.GetNextToken();
			return new ConditionRelationalExpression(conditionExpression, ParseLiteralExpression(), ConditionRelationalOperator.Equal);
		}
		if (tokenizer.IsToken(ConditionTokenType.NotEqual))
		{
			tokenizer.GetNextToken();
			return new ConditionRelationalExpression(conditionExpression, ParseLiteralExpression(), ConditionRelationalOperator.NotEqual);
		}
		if (tokenizer.IsToken(ConditionTokenType.LessThan))
		{
			tokenizer.GetNextToken();
			return new ConditionRelationalExpression(conditionExpression, ParseLiteralExpression(), ConditionRelationalOperator.Less);
		}
		if (tokenizer.IsToken(ConditionTokenType.GreaterThan))
		{
			tokenizer.GetNextToken();
			return new ConditionRelationalExpression(conditionExpression, ParseLiteralExpression(), ConditionRelationalOperator.Greater);
		}
		if (tokenizer.IsToken(ConditionTokenType.LessThanOrEqualTo))
		{
			tokenizer.GetNextToken();
			return new ConditionRelationalExpression(conditionExpression, ParseLiteralExpression(), ConditionRelationalOperator.LessOrEqual);
		}
		if (tokenizer.IsToken(ConditionTokenType.GreaterThanOrEqualTo))
		{
			tokenizer.GetNextToken();
			return new ConditionRelationalExpression(conditionExpression, ParseLiteralExpression(), ConditionRelationalOperator.GreaterOrEqual);
		}
		return conditionExpression;
	}

	private ConditionExpression ParseBooleanPredicate()
	{
		if (tokenizer.IsKeyword("not") || tokenizer.IsToken(ConditionTokenType.Not))
		{
			tokenizer.GetNextToken();
			return new ConditionNotExpression(ParseBooleanPredicate());
		}
		return ParseBooleanRelation();
	}

	private ConditionExpression ParseBooleanAnd()
	{
		ConditionExpression conditionExpression = ParseBooleanPredicate();
		while (tokenizer.IsKeyword("and") || tokenizer.IsToken(ConditionTokenType.And))
		{
			tokenizer.GetNextToken();
			conditionExpression = new ConditionAndExpression(conditionExpression, ParseBooleanPredicate());
		}
		return conditionExpression;
	}

	private ConditionExpression ParseBooleanOr()
	{
		ConditionExpression conditionExpression = ParseBooleanAnd();
		while (tokenizer.IsKeyword("or") || tokenizer.IsToken(ConditionTokenType.Or))
		{
			tokenizer.GetNextToken();
			conditionExpression = new ConditionOrExpression(conditionExpression, ParseBooleanAnd());
		}
		return conditionExpression;
	}

	private ConditionExpression ParseBooleanExpression()
	{
		return ParseBooleanOr();
	}

	private ConditionExpression ParseExpression()
	{
		return ParseBooleanExpression();
	}
}
