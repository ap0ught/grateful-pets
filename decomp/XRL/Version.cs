using System;
using Newtonsoft.Json;

namespace XRL;

[Serializable]
[JsonConverter(typeof(Converter))]
public struct Version : IComparable<Version>, IEquatable<Version>, IEquatable<System.Version>
{
	public class Converter : JsonConverter<Version>
	{
		public override void WriteJson(JsonWriter writer, Version value, JsonSerializer serializer)
		{
			writer.WriteValue(value.ToString());
		}

		public override Version ReadJson(JsonReader reader, Type objectType, Version existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.String)
			{
				return new Version((string)reader.Value);
			}
			return Zero;
		}
	}

	public static readonly Version Zero = new Version(0);

	public static readonly Version Max = new Version(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);

	public const int Length = 4;

	public int Major;

	public int Minor;

	public int Build;

	public int Revision;

	public int this[int Index]
	{
		get
		{
			return Index switch
			{
				0 => Major, 
				1 => Minor, 
				2 => Build, 
				3 => Revision, 
				_ => throw new IndexOutOfRangeException(), 
			};
		}
		set
		{
			switch (Index)
			{
			case 0:
				Major = value;
				break;
			case 1:
				Minor = value;
				break;
			case 2:
				Build = value;
				break;
			case 3:
				Revision = value;
				break;
			default:
				throw new IndexOutOfRangeException();
			}
		}
	}

	public Version(int Major, int Minor = 0, int Build = 0, int Revision = 0)
	{
		this.Major = Major;
		this.Minor = Minor;
		this.Build = Build;
		this.Revision = Revision;
	}

	/// <remarks>
	/// <see cref="T:System.Version">System.Version</see> has a strange quirk where build and revision can be undefined/-1 to where 7.0 does not equal 7.0.0.0.
	/// We do not replicate this behaviour in <see cref="T:XRL.Version">XRL.Version</see> and force a minimum zero value.
	/// </remarks>
	public Version(System.Version Version)
	{
		Major = Version.Major;
		Minor = Version.Minor;
		Build = Math.Max(Version.Build, 0);
		Revision = Math.Max(Version.Revision, 0);
	}

	public Version(string Version)
	{
		if (!TryParse(Version, out Major, out Minor, out Build, out Revision))
		{
			throw new FormatException("'" + Version + "' is not a valid version string.");
		}
	}

	public int CompareTo(Version Other)
	{
		if (Major != Other.Major)
		{
			if (Major > Other.Major)
			{
				return 1;
			}
			return -1;
		}
		if (Minor != Other.Minor)
		{
			if (Minor > Other.Minor)
			{
				return 1;
			}
			return -1;
		}
		if (Build != Other.Build)
		{
			if (Build > Other.Build)
			{
				return 1;
			}
			return -1;
		}
		if (Revision != Other.Revision)
		{
			if (Revision > Other.Revision)
			{
				return 1;
			}
			return -1;
		}
		return 0;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Major, Minor, Build, Revision);
	}

	public override bool Equals(object Other)
	{
		if (Other is Version other)
		{
			return Equals(other);
		}
		if (Other is System.Version other2)
		{
			return Equals(other2);
		}
		return false;
	}

	public bool Equals(Version Other)
	{
		if (Major == Other.Major && Minor == Other.Minor && Build == Other.Build)
		{
			return Revision == Other.Revision;
		}
		return false;
	}

	public bool Equals(System.Version Other)
	{
		if ((object)Other != null && Major == Other.Major && Minor == Other.Minor && Build == Math.Max(Other.Build, 0))
		{
			return Revision == Math.Max(Other.Revision, 0);
		}
		return false;
	}

	public bool EqualsSemantic(ReadOnlySpan<char> Range)
	{
		if (Range.IsEmpty)
		{
			return true;
		}
		if (Range.Length == 1 && (Range[0] == '*' || Range[0] == 'x' || Range[0] == 'X'))
		{
			return true;
		}
		DelimitedEnumeratorString enumerator = Range.DelimitedBy("||").GetEnumerator();
		while (enumerator.MoveNext())
		{
			ReadOnlySpan<char> readOnlySpan = enumerator.Current.Trim();
			int num = readOnlySpan.IndexOf('-');
			if (num != -1)
			{
				Version version = ParseLowSemantic(readOnlySpan.Slice(0, num).Trim());
				Version version2 = ParseHighSemantic((num >= readOnlySpan.Length - 1) ? ReadOnlySpan<char>.Empty : readOnlySpan.Slice(num + 1).Trim());
				if (this >= version && this <= version2)
				{
					return true;
				}
			}
			bool flag = true;
			DelimitedEnumeratorChar enumerator2 = readOnlySpan.DelimitedBy(' ').GetEnumerator();
			while (enumerator2.MoveNext())
			{
				ReadOnlySpan<char> readOnlySpan2 = enumerator2.Current.Trim();
				if (readOnlySpan2.IsEmpty)
				{
					continue;
				}
				Version Version = Zero;
				Version Version2 = Max;
				char c = readOnlySpan2[0];
				if (c == '^')
				{
					if (!TryParseSemantic(readOnlySpan2.Slice(1), ref Version))
					{
						flag = false;
						break;
					}
					for (int i = 0; i < 4; i++)
					{
						Version2[i] = Version[i];
						if (Version[i] != 0)
						{
							break;
						}
					}
					if (this < Version || this > Version2)
					{
						flag = false;
						break;
					}
				}
				else if (c == '<')
				{
					if (readOnlySpan2[1] == '=')
					{
						if (!TryParseSemantic(readOnlySpan2.Slice(2), ref Version2) || this > Version2)
						{
							flag = false;
							break;
						}
					}
					else if (!TryParseSemantic(readOnlySpan2.Slice(1), ref Version) || this >= Version)
					{
						flag = false;
						break;
					}
				}
				else if (c == '>')
				{
					if (readOnlySpan2[1] == '=')
					{
						if (!TryParseSemantic(readOnlySpan2.Slice(2), ref Version) || this < Version)
						{
							flag = false;
							break;
						}
					}
					else if (!TryParseSemantic(readOnlySpan2.Slice(1), ref Version2) || this <= Version2)
					{
						flag = false;
						break;
					}
				}
				else
				{
					ReadOnlySpan<char> text = ((c == '=') ? readOnlySpan2.Slice(1) : readOnlySpan2);
					if (!TryParseSemantic(text, ref Version) || !TryParseSemantic(text, ref Version2) || this < Version || this > Version2)
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsZero()
	{
		return this == Zero;
	}

	public override string ToString()
	{
		return ToString((Revision == 0) ? 3 : 4);
	}

	public string ToString(int Fields)
	{
		switch (Fields)
		{
		case 0:
			return string.Empty;
		case 1:
			return Major.ToString();
		default:
		{
			int num = Major.CountDigits() + Minor.CountDigits() + 1;
			int Index = 0;
			if (Fields >= 3)
			{
				num += Build.CountDigits() + 1;
			}
			if (Fields >= 4)
			{
				num += Revision.CountDigits() + 1;
			}
			Span<char> Text = stackalloc char[num];
			Text.Insert(ref Index, Major);
			Text[Index++] = '.';
			Text.Insert(ref Index, Minor);
			if (Fields >= 3)
			{
				Text[Index++] = '.';
				Text.Insert(ref Index, Build);
			}
			if (Fields >= 4)
			{
				Text[Index++] = '.';
				Text.Insert(ref Index, Revision);
			}
			return new string(Text);
		}
		}
	}

	public static implicit operator Version(System.Version Version)
	{
		return new Version(Version);
	}

	public static bool operator ==(Version First, Version Second)
	{
		return First.Equals(Second);
	}

	public static bool operator !=(Version First, Version Second)
	{
		return !First.Equals(Second);
	}

	public static bool operator <(Version First, Version Second)
	{
		return First.CompareTo(Second) < 0;
	}

	public static bool operator <=(Version First, Version Second)
	{
		return First.CompareTo(Second) <= 0;
	}

	public static bool operator >(Version First, Version Second)
	{
		return First.CompareTo(Second) > 0;
	}

	public static bool operator >=(Version First, Version Second)
	{
		return First.CompareTo(Second) >= 0;
	}

	public static bool TryParse(ReadOnlySpan<char> Text, out Version Version)
	{
		Version = default;
		return TryParse(Text, out Version.Major, out Version.Minor, out Version.Build, out Version.Revision);
	}

	public static bool TryParse(ReadOnlySpan<char> Text, out int Major, out int Minor, out int Build, out int Revision)
	{
		Major = (Minor = (Build = (Revision = 0)));
		if (Text.Length == 0)
		{
			return false;
		}
		int num = Text.IndexOf('.');
		if (num == -1)
		{
			return int.TryParse(Text, out Major);
		}
		if (!int.TryParse(Text.Slice(0, num), out Major))
		{
			return false;
		}
		Text = Text.Slice(num + 1);
		num = Text.IndexOf('.');
		if (num == -1)
		{
			return int.TryParse(Text, out Minor);
		}
		if (!int.TryParse(Text.Slice(0, num), out Minor))
		{
			return false;
		}
		Text = Text.Slice(num + 1);
		num = Text.IndexOf('.');
		if (num == -1)
		{
			return int.TryParse(Text, out Build);
		}
		if (!int.TryParse(Text.Slice(0, num), out Build))
		{
			return false;
		}
		Text = Text.Slice(num + 1);
		num = Text.IndexOf('.');
		return int.TryParse((num == -1) ? Text : Text.Slice(0, num), out Revision);
	}

	public static bool TryParseSemantic(ReadOnlySpan<char> Text, ref Version Version)
	{
		int num = 0;
		DelimitedEnumeratorChar enumerator = Text.DelimitedBy('.').GetEnumerator();
		while (enumerator.MoveNext())
		{
			ReadOnlySpan<char> current = enumerator.Current;
			if (current.IsEmpty || current[0] == '*' || current[0] == 'x' || current[0] == 'X')
			{
				break;
			}
			if (!int.TryParse(current, out var result))
			{
				return false;
			}
			Version[num] = result;
			num++;
		}
		return true;
	}

	public static Version ParseLowSemantic(ReadOnlySpan<char> Text)
	{
		Version Version = Zero;
		TryParseSemantic(Text, ref Version);
		return Version;
	}

	public static Version ParseHighSemantic(ReadOnlySpan<char> Text)
	{
		Version Version = Max;
		TryParseSemantic(Text, ref Version);
		return Version;
	}
}
