using System;
using System.Text.RegularExpressions;

namespace FuzzySharp.PreProcess;

internal class StringPreprocessorFactory
{
	private static string pattern = "[^ a-zA-Z0-9]";

	private static string Default(string input)
	{
		input = Regex.Replace(input, pattern, " ");
		input = input.ToLower();
		return input.Trim();
	}

	public static Func<string, string> GetPreprocessor(PreprocessMode mode)
	{
		return mode switch
		{
			PreprocessMode.Full => (Func<string, string>)Default, 
			PreprocessMode.None => (string s) => s, 
			_ => throw new InvalidOperationException($"Invalid string preprocessor mode: {mode}"), 
		};
	}
}
