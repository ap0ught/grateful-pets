using System;
using System.Collections.Generic;
using FuzzySharp.Extractor;
using FuzzySharp.PreProcess;
using FuzzySharp.SimilarityRatio;
using FuzzySharp.SimilarityRatio.Scorer;
using FuzzySharp.SimilarityRatio.Scorer.Composite;

namespace FuzzySharp;

public static class Process
{
	private static readonly IRatioScorer s_defaultScorer = ScorerCache.Get<WeightedRatioScorer>();

	private static readonly Func<string, string> s_defaultStringProcessor = StringPreprocessorFactory.GetPreprocessor(PreprocessMode.Full);

	public static IEnumerable<ExtractedResult<string>> ExtractAll(string query, IEnumerable<string> choices, Func<string, string> processor = null, IRatioScorer scorer = null, int cutoff = 0)
	{
		if (processor == null)
		{
			processor = s_defaultStringProcessor;
		}
		if (scorer == null)
		{
			scorer = s_defaultScorer;
		}
		return ResultExtractor.ExtractWithoutOrder(query, choices, processor, scorer, cutoff);
	}

	public static IEnumerable<ExtractedResult<T>> ExtractAll<T>(T query, IEnumerable<T> choices, Func<T, string> processor, IRatioScorer scorer = null, int cutoff = 0)
	{
		if (scorer == null)
		{
			scorer = s_defaultScorer;
		}
		return ResultExtractor.ExtractWithoutOrder(query, choices, processor, scorer, cutoff);
	}

	public static IEnumerable<ExtractedResult<string>> ExtractTop(string query, IEnumerable<string> choices, Func<string, string> processor = null, IRatioScorer scorer = null, int limit = 5, int cutoff = 0)
	{
		if (processor == null)
		{
			processor = s_defaultStringProcessor;
		}
		if (scorer == null)
		{
			scorer = s_defaultScorer;
		}
		return ResultExtractor.ExtractTop(query, choices, processor, scorer, limit, cutoff);
	}

	public static IEnumerable<ExtractedResult<T>> ExtractTop<T>(T query, IEnumerable<T> choices, Func<T, string> processor, IRatioScorer scorer = null, int limit = 5, int cutoff = 0)
	{
		if (scorer == null)
		{
			scorer = s_defaultScorer;
		}
		return ResultExtractor.ExtractTop(query, choices, processor, scorer, limit, cutoff);
	}

	public static IEnumerable<ExtractedResult<string>> ExtractSorted(string query, IEnumerable<string> choices, Func<string, string> processor = null, IRatioScorer scorer = null, int cutoff = 0)
	{
		if (processor == null)
		{
			processor = s_defaultStringProcessor;
		}
		if (scorer == null)
		{
			scorer = s_defaultScorer;
		}
		return ResultExtractor.ExtractSorted(query, choices, processor, scorer, cutoff);
	}

	public static IEnumerable<ExtractedResult<T>> ExtractSorted<T>(T query, IEnumerable<T> choices, Func<T, string> processor, IRatioScorer scorer = null, int cutoff = 0)
	{
		if (scorer == null)
		{
			scorer = s_defaultScorer;
		}
		return ResultExtractor.ExtractSorted(query, choices, processor, scorer, cutoff);
	}

	public static ExtractedResult<string> ExtractOne(string query, IEnumerable<string> choices, Func<string, string> processor = null, IRatioScorer scorer = null, int cutoff = 0)
	{
		if (processor == null)
		{
			processor = s_defaultStringProcessor;
		}
		if (scorer == null)
		{
			scorer = s_defaultScorer;
		}
		return ResultExtractor.ExtractOne(query, choices, processor, scorer, cutoff);
	}

	public static ExtractedResult<T> ExtractOne<T>(T query, IEnumerable<T> choices, Func<T, string> processor, IRatioScorer scorer = null, int cutoff = 0)
	{
		if (scorer == null)
		{
			scorer = s_defaultScorer;
		}
		return ResultExtractor.ExtractOne(query, choices, processor, scorer, cutoff);
	}

	public static ExtractedResult<string> ExtractOne(string query, params string[] choices)
	{
		return ResultExtractor.ExtractOne(query, choices, s_defaultStringProcessor, s_defaultScorer);
	}
}
