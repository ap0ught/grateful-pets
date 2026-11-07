using System;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace XRL.Tests;

public class VersionTests
{
	private StringBuilder Builder = new StringBuilder();

	[SetUp]
	public void Setup()
	{
		Builder.Clear();
	}

	[TestCase("7.5")]
	[TestCase("0.14.0")]
	[TestCase("1.8.106")]
	[TestCase("2.0.214.66")]
	public void TestParse(string Version)
	{
		Version version = new Version(Version);
		System.Version version2 = new System.Version(Version);
		Assert.True(version == version2, "System.Version == XRL.Version");
		int num = Version.Count((char x) => x == '.') + 1;
		Assert.AreEqual(Version, version.ToString(num), "Input == ToString()");
	}

	[TestCase("8000", "*", true)]
	[TestCase("1.0", "1.0.*", true)]
	[TestCase("1.0", "=1", true)]
	[TestCase("1.1", "=1.0", false)]
	[TestCase("1.1", "1.0.*", false)]
	[TestCase("5.0.700.345345", "2.0 - *", true)]
	[TestCase("7.5", ">7.4", true)]
	[TestCase("7.4.12", ">7.4", false)]
	[TestCase("7.4.12", ">7.4.2 <7.5", true)]
	[TestCase("40.8.89", ">37 <=40.8", true)]
	[TestCase("40.8.12", ">0.* >1.1.1.1 <40.8", false)]
	[TestCase("40.8.66", "^40.8", true)]
	[TestCase("41.0.0", "^40.8", false)]
	[TestCase("0.0.1.67", "^0.0.1", true)]
	[TestCase("0.0.2.4", "^0.0.1", false)]
	[TestCase("2.0.214.66", "<2.0.4 || 2.0.214.66 - 2.0.215", true)]
	[TestCase("2.0.11", ">2.0.11 || 2.0.7.67 - 2.0.10.*", false)]
	[TestCase("2.0.10.7", "2.0.7.67 - 2.0.10.7", true)]
	public void TestEqualsSemantic(string Version, string Semantic, bool Expected)
	{
		Version version = new Version(Version);
		bool flag = version.EqualsSemantic(Semantic);
		StringBuilder builder = Builder;
		DelimitedEnumeratorString enumerator = Semantic.DelimitedBy("||").GetEnumerator();
		while (enumerator.MoveNext())
		{
			ReadOnlySpan<char> readOnlySpan = enumerator.Current.Trim();
			if (readOnlySpan.IndexOf('-') != -1)
			{
				builder.Append("\n\"").Append(readOnlySpan).Append("\" : ")
					.Append(version.EqualsSemantic(readOnlySpan));
				continue;
			}
			DelimitedEnumeratorChar enumerator2 = readOnlySpan.DelimitedBy(' ').GetEnumerator();
			while (enumerator2.MoveNext())
			{
				ReadOnlySpan<char> readOnlySpan2 = enumerator2.Current.Trim();
				builder.Append("\n\"").Append(readOnlySpan2).Append("\" : ")
					.Append(version.EqualsSemantic(readOnlySpan2));
			}
		}
		string message = builder.ToString();
		Assert.AreEqual(Expected, flag, message);
		Assert.Pass(message);
	}
}
