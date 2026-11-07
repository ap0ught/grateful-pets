using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.LayoutRenderers;

[LayoutRenderer("counter", false)]
[Preserve]
public class CounterLayoutRenderer : LayoutRenderer
{
	private static Dictionary<string, int> sequences = new Dictionary<string, int>();

	[DefaultValue(1)]
	public int Value { get; set; }

	[DefaultValue(1)]
	public int Increment { get; set; }

	public string Sequence { get; set; }

	public CounterLayoutRenderer()
	{
		Increment = 1;
		Value = 1;
	}

	protected override void Append(StringBuilder builder, LogEventInfo logEvent)
	{
		int num;
		if (Sequence != null)
		{
			num = GetNextSequenceValue(Sequence, Value, Increment);
		}
		else
		{
			num = Value;
			Value += Increment;
		}
		builder.Append(num.ToString(CultureInfo.InvariantCulture));
	}

	private static int GetNextSequenceValue(string sequenceName, int defaultValue, int increment)
	{
		lock (sequences)
		{
			if (!sequences.TryGetValue(sequenceName, out var value))
			{
				value = defaultValue;
			}
			int result = value;
			value += increment;
			sequences[sequenceName] = value;
			return result;
		}
	}
}
