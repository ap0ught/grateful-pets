using System;
using System.Buffers;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using TMPro;
using XRL.UI;

namespace XRL.World.Text;

public class TextBuilder : IList<char>, ICollection<char>, IEnumerable<char>, IEnumerable, IReadOnlyList<char>, IReadOnlyCollection<char>, IReadOnlySpanConvertible<char>, IEquatable<string>, IDisposable
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static class Pool
	{
		/// <summary>Warm up the thread statics of concurrent pool from permanent threads.</summary>
		public static void Populate(int Amount = 8)
		{
			for (int i = 0; i < Amount; i++)
			{
				Bag.Add(new Entry());
			}
		}
	}

	private sealed class Entry : TextBuilder
	{
		protected override bool Finalizable => false;

		public override void Dispose(bool Destroy)
		{
			base.Dispose(Destroy);
			if (!Destroy)
			{
				Bag.Add(this);
			}
		}
	}

	[Serializable]
	public struct Enumerator : IEnumerator<char>, IEnumerator, IDisposable
	{
		private TextBuilder Builder;

		private int Index;

		private int Version;

		private char Item;

		public char Current => Item;

		object IEnumerator.Current => Item;

		public Enumerator(TextBuilder Builder)
		{
			this.Builder = Builder;
			Index = 0;
			Version = Builder.Version;
			Item = '\0';
		}

		public bool MoveNext()
		{
			TextBuilder builder = Builder;
			if (builder.Version != Version)
			{
				throw new InvalidOperationException();
			}
			int index = Index;
			if (index >= builder.Length)
			{
				Item = '\0';
				return false;
			}
			Item = builder.Buffer[index];
			Index = index + 1;
			return true;
		}

		public void Reset()
		{
			if (Builder.Version != Version)
			{
				throw new InvalidOperationException();
			}
			Index = 0;
			Item = '\0';
		}

		public void Dispose()
		{
		}
	}

	private static char[] hexVals = new char[16]
	{
		'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
		'a', 'b', 'c', 'd', 'e', 'f'
	};

	private const ulong Int32AbsoluteMinimum = 2147483648uL;

	private const ulong Int64AbsoluteMinimum = 9223372036854775808uL;

	private const int Int32Shift = 31;

	private const int Int64Shift = 63;

	private static readonly ConcurrentBag<Entry> Bag = new ConcurrentBag<Entry>();

	protected static readonly ConcurrentDictionary<TextBuilder, StackTrace> Traces = new ConcurrentDictionary<TextBuilder, StackTrace>();

	protected char[] Buffer;

	protected char[] ScratchBuffer;

	public int Length;

	public int Version;

	bool ICollection<char>.IsReadOnly => false;

	int ICollection<char>.Count => Length;

	int IReadOnlyCollection<char>.Count => Length;

	public int Capacity => Buffer.Length;

	/// <summary>If false an error will be logged when the object is finalized.</summary>
	protected virtual bool Finalizable => true;

	public char this[int Index]
	{
		get
		{
			if ((uint)Index >= (uint)Length)
			{
				throw new ArgumentOutOfRangeException();
			}
			return Buffer[Index];
		}
		set
		{
			if ((uint)Index >= (uint)Length)
			{
				throw new ArgumentOutOfRangeException();
			}
			Buffer[Index] = value;
			Version++;
		}
	}

	public char this[Index Index]
	{
		get
		{
			return Buffer[Index.GetOffset(Length)];
		}
		set
		{
			Buffer[Index.GetOffset(Length)] = value;
			Version++;
		}
	}

	public Span<char> this[System.Range Range] => AsSpan(Range.Start.GetOffset(Length), Range.End.GetOffset(Length));

	public void SetToTMP(TextMeshProUGUI tmp)
	{
		tmp.SetCharArray(Buffer, 0, Length);
	}

	public TextBuilder AppendHex2(int Value)
	{
		return Append(hexVals[(Value >> 4) & 0xF]).Append(hexVals[Value & 0xF]);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Append(char Value)
	{
		return Insert(Length, Value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Append(char Value, int Count)
	{
		return Insert(Length, Value, Count);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Append(char? Value)
	{
		return Insert(Length, Value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Append(char? Value, int Count)
	{
		return Insert(Length, Value, Count);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Append(bool Value)
	{
		return Insert(Length, Value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Append(ReadOnlySpan<char> Value)
	{
		return Insert(Length, Value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Append(ReadOnlySpan<char> Value, int Start, int Length)
	{
		return Insert(this.Length, Value, Start, Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder AppendUpper(ReadOnlySpan<char> Value)
	{
		return InsertUpper(Length, Value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder AppendLower(ReadOnlySpan<char> Value)
	{
		return InsertLower(Length, Value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Append(string Value)
	{
		return Insert(Length, Value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Append(string Value, int Start)
	{
		return Insert(Length, Value, Start);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Append(string Value, int Start, int Length)
	{
		return Insert(this.Length, Value, Start, Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Append(StringBuilder Value)
	{
		return Insert(Length, Value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Append(IReadOnlySpanConvertible<char> Value)
	{
		return Insert(Length, Value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Append(int Value)
	{
		return Insert(Length, Value, Signed: false);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Append(long Value)
	{
		return Insert(Length, Value, Signed: false);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Append(uint Value)
	{
		return Insert(Length, Value, Signed: false);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Append(ulong Value)
	{
		return Insert(Length, Value, Signed: false);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Append(float Value)
	{
		return Insert(Length, Value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Append(double Value)
	{
		return Insert(Length, Value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Append(Guid Value)
	{
		return Insert(Length, Value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Append(int Value, bool Signed)
	{
		return Insert(Length, Value, Signed);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Append(long Value, bool Signed)
	{
		return Insert(Length, Value, Signed);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Append(uint Value, bool Signed)
	{
		return Insert(Length, Value, Signed);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Append(ulong Value, bool Signed)
	{
		return Insert(Length, Value, Signed);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Append(int Value, ReadOnlySpan<char> Format, IFormatProvider Provider = null)
	{
		return Insert(Length, Value, Format, Provider);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Append(long Value, ReadOnlySpan<char> Format, IFormatProvider Provider = null)
	{
		return Insert(Length, Value, Format, Provider);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Append(uint Value, ReadOnlySpan<char> Format, IFormatProvider Provider = null)
	{
		return Insert(Length, Value, Format, Provider);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Append(ulong Value, ReadOnlySpan<char> Format, IFormatProvider Provider = null)
	{
		return Insert(Length, Value, Format, Provider);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Append(float Value, ReadOnlySpan<char> Format, IFormatProvider Provider = null)
	{
		return Insert(Length, Value, Format, Provider);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Append(double Value, ReadOnlySpan<char> Format, IFormatProvider Provider = null)
	{
		return Insert(Length, Value, Format, Provider);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Append(Guid Value, ReadOnlySpan<char> Format)
	{
		return Insert(Length, Value, Format);
	}

	/// <remarks>
	/// This will call ToString and thus allocate a discarded string.
	/// If possible implement a dedicated method for the type.
	/// </remarks>
	public TextBuilder Append(object Value)
	{
		return Append(Value.ToString());
	}

	public TextBuilder AppendByType(object Value)
	{
		return InsertByType(Length, Value);
	}

	public TextBuilder AppendLine()
	{
		return Append('\n');
	}

	public TextBuilder AppendLine(char Value)
	{
		return Append(Value).Append('\n');
	}

	public TextBuilder AppendLine(string Value)
	{
		return Append(Value).Append('\n');
	}

	public TextBuilder AppendLine(ReadOnlySpan<char> Value)
	{
		return Append(Value).Append('\n');
	}

	public TextBuilder AppendMask(int Mask, int Length = 32)
	{
		for (int num = Length - 1; num >= 0; num--)
		{
			Append(((Mask & (1 << num)) != 0) ? '1' : '0');
		}
		return this;
	}

	public TextBuilder AppendMask(int Mask, int Start, int Length)
	{
		for (int num = Start + Length - 1; num >= Start; num--)
		{
			Append(((Mask & (1 << num)) != 0) ? '1' : '0');
		}
		return this;
	}

	public TextBuilder AppendRange(IReadOnlyList<string> Range, string Separator)
	{
		int i = 0;
		for (int count = Range.Count; i < count; i++)
		{
			if (i != 0)
			{
				Append(Separator);
			}
			Append(Range[i]);
		}
		return this;
	}

	public TextBuilder AppendRange(IEnumerable<string> Range, string Separator)
	{
		bool flag = true;
		foreach (string item in Range)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				Append(Separator);
			}
			Append(item);
		}
		return this;
	}

	public TextBuilder AppendPrefixed(IReadOnlyList<string> Range, string Prefix)
	{
		int i = 0;
		for (int count = Range.Count; i < count; i++)
		{
			Append(Prefix);
			Append(Range[i]);
		}
		return this;
	}

	public TextBuilder AppendPrefixed(IEnumerable<string> Range, string Prefix)
	{
		foreach (string item in Range)
		{
			Append(Prefix);
			Append(item);
		}
		return this;
	}

	public TextBuilder Compound(string Value)
	{
		if (Length > 0)
		{
			Append(' ');
		}
		Append(Value);
		return this;
	}

	public TextBuilder Compound(string Value, string With)
	{
		if (Length > 0)
		{
			Append(With);
		}
		Append(Value);
		return this;
	}

	public TextBuilder Compound(string Value, char With)
	{
		if (Length > 0)
		{
			Append(With);
		}
		Append(Value);
		return this;
	}

	public TextBuilder Compound(char Value)
	{
		if (Length > 0)
		{
			Append(' ');
		}
		Append(Value);
		return this;
	}

	public TextBuilder Compound(char Value, string With)
	{
		if (Length > 0)
		{
			Append(With);
		}
		Append(Value);
		return this;
	}

	public TextBuilder Compound(char Value, char With)
	{
		if (Length > 0)
		{
			Append(With);
		}
		Append(Value);
		return this;
	}

	public TextBuilder Compound(int Value, bool Signed = false)
	{
		if (Length > 0)
		{
			Append(' ');
		}
		Append(Value, Signed);
		return this;
	}

	public TextBuilder Compound(int Value, char With, bool Signed = false)
	{
		if (Length > 0)
		{
			Append(With);
		}
		Append(Value, Signed);
		return this;
	}

	public TextBuilder Compound(int Value, string With, bool Signed = false)
	{
		if (Length > 0)
		{
			Append(With);
		}
		Append(Value, Signed);
		return this;
	}

	public TextBuilder Compound(IReadOnlySpanConvertible<char> Value)
	{
		if (Length > 0)
		{
			Append(' ');
		}
		Append(Value);
		return this;
	}

	public TextBuilder Compound(IReadOnlySpanConvertible<char> Value, char With)
	{
		if (Length > 0)
		{
			Append(With);
		}
		Append(Value);
		return this;
	}

	public TextBuilder Compound(IReadOnlySpanConvertible<char> Value, string With)
	{
		if (Length > 0)
		{
			Append(With);
		}
		Append(Value);
		return this;
	}

	void ICollection<char>.Add(char Value)
	{
		Append(Value);
	}

	void IList<char>.Insert(int Index, char Value)
	{
		Insert(Index, Value);
	}

	bool ICollection<char>.Remove(char Value)
	{
		int num = IndexOf(Value);
		if (num != -1)
		{
			Remove(num, 1);
			return true;
		}
		return false;
	}

	void IList<char>.RemoveAt(int Index)
	{
		Remove(Index, 1);
	}

	void ICollection<char>.Clear()
	{
		Clear();
	}

	ReadOnlySpan<char> IReadOnlySpanConvertible<char>.AsSpan()
	{
		return AsSpan();
	}

	ReadOnlySpan<char> IReadOnlySpanConvertible<char>.AsSpan(int Start)
	{
		return AsSpan(Start);
	}

	ReadOnlySpan<char> IReadOnlySpanConvertible<char>.AsSpan(int Start, int Length)
	{
		return AsSpan(Start, Length);
	}

	public TextBuilder Insert(int Index, char Value)
	{
		int length = Length;
		if (Index > length)
		{
			throw new ArgumentOutOfRangeException("Index");
		}
		char[] buffer = Buffer;
		if (length == buffer.Length)
		{
			Grow(1);
			buffer = Buffer;
		}
		if (Index < length)
		{
			Array.Copy(buffer, Index, buffer, Index + 1, length - Index);
		}
		buffer[Index] = Value;
		Length = length + 1;
		Version++;
		return this;
	}

	public TextBuilder Insert(int Index, char Value, int Count)
	{
		int length = Length;
		if (Index > length)
		{
			throw new ArgumentOutOfRangeException("Index");
		}
		if (Count < 0)
		{
			throw new ArgumentOutOfRangeException("Count");
		}
		char[] buffer = Buffer;
		if (length + Count > buffer.Length)
		{
			Grow(Count);
			buffer = Buffer;
		}
		if (Index < length)
		{
			Array.Copy(buffer, Index, buffer, Index + Count, length - Index);
		}
		for (int i = 0; i < Count; i++)
		{
			buffer[Index + i] = Value;
		}
		Length = length + Count;
		Version++;
		return this;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Insert(int Index, char? Value)
	{
		if (!Value.HasValue)
		{
			return this;
		}
		return Insert(Index, Value.Value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Insert(int Index, char? Value, int Count)
	{
		if (!Value.HasValue)
		{
			return this;
		}
		return Insert(Index, Value.Value, Count);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Insert(int Index, bool Value)
	{
		return Insert(Index, Value ? "true" : "false");
	}

	public TextBuilder Insert(int Index, ReadOnlySpan<char> Value)
	{
		int length = Length;
		if (Index > length)
		{
			throw new ArgumentOutOfRangeException("Index");
		}
		char[] buffer = Buffer;
		int length2 = Value.Length;
		if (length + length2 > buffer.Length)
		{
			Grow(length2);
			buffer = Buffer;
		}
		if (Index < length)
		{
			Array.Copy(buffer, Index, buffer, Index + length2, length - Index);
		}
		Value.CopyTo(buffer.AsSpan(Index));
		Length = length + length2;
		Version++;
		return this;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Insert(int Index, string Value)
	{
		return Insert(Index, Value ?? (Value = ""), 0, Value.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Insert(int Index, string Value, int Start)
	{
		return Insert(Index, Value ?? (Value = ""), Start, Value.Length - Start);
	}

	public TextBuilder Insert(int Index, string Value, int Start, int Length)
	{
		int length = this.Length;
		if (Index > length)
		{
			throw new ArgumentOutOfRangeException("Index");
		}
		if (Length == 0)
		{
			return this;
		}
		char[] buffer = Buffer;
		if (length + Length > buffer.Length)
		{
			Grow(Length);
			buffer = Buffer;
		}
		if (Index < length)
		{
			Array.Copy(buffer, Index, buffer, Index + Length, length - Index);
		}
		Value.CopyTo(Start, buffer, Index, Length);
		this.Length = length + Length;
		Version++;
		return this;
	}

	public TextBuilder Insert(int Index, ReadOnlySpan<char> Value, int Start, int Length)
	{
		int length = this.Length;
		if (Index > length)
		{
			throw new ArgumentOutOfRangeException("Index");
		}
		if (Length == 0)
		{
			return this;
		}
		char[] buffer = Buffer;
		if (length + Length > buffer.Length)
		{
			Grow(Length);
			buffer = Buffer;
		}
		if (Index < length)
		{
			Array.Copy(buffer, Index, buffer, Index + Length, length - Index);
		}
		Value.Slice(Start, Length).CopyTo(buffer.AsSpan(Index, Length));
		this.Length = length + Length;
		Version++;
		return this;
	}

	public TextBuilder Insert(int Index, StringBuilder Value)
	{
		int length = Length;
		if (Index > length)
		{
			throw new ArgumentOutOfRangeException("Index");
		}
		char[] buffer = Buffer;
		int length2 = Value.Length;
		if (length + length2 > buffer.Length)
		{
			Grow(length2);
			buffer = Buffer;
		}
		if (Index < length)
		{
			Array.Copy(buffer, Index, buffer, Index + length2, length - Index);
		}
		Value.CopyTo(0, buffer, Index, length2);
		Length = length + length2;
		Version++;
		return this;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Insert(int Index, IReadOnlySpanConvertible<char> Value)
	{
		return Insert(Index, Value.AsSpan());
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Insert(int Index, int Value)
	{
		return Insert(Index, Value, Signed: false);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Insert(int Index, long Value)
	{
		return Insert(Index, Value, Signed: false);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Insert(int Index, uint Value)
	{
		return InsertInternal(Index, Value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Insert(int Index, ulong Value)
	{
		return InsertInternal(Index, Value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Insert(int Index, float Value)
	{
		return Insert(Index, Value, ReadOnlySpan<char>.Empty);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Insert(int Index, double Value)
	{
		return Insert(Index, Value, ReadOnlySpan<char>.Empty);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Insert(int Index, Guid Value)
	{
		return Insert(Index, Value, ReadOnlySpan<char>.Empty);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Insert(int Index, int Value, bool Signed)
	{
		if (Value == int.MinValue)
		{
			return InsertInternal(Index, 2147483648uL, Negative: true, Signed);
		}
		return InsertInternal(Index, (ulong)((Value + (Value >> 31)) ^ (Value >> 31)), Value < 0, Signed);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Insert(int Index, long Value, bool Signed)
	{
		if (Value == long.MinValue)
		{
			return InsertInternal(Index, 9223372036854775808uL, Negative: true, Signed);
		}
		return InsertInternal(Index, (ulong)((Value + (Value >> 63)) ^ (Value >> 63)), Value < 0, Signed);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Insert(int Index, uint Value, bool Signed)
	{
		return InsertInternal(Index, Value, Negative: false, Signed);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextBuilder Insert(int Index, ulong Value, bool Signed)
	{
		return InsertInternal(Index, Value, Negative: false, Signed);
	}

	public TextBuilder Insert(int Index, int Value, ReadOnlySpan<char> Format, IFormatProvider Provider = null)
	{
		Span<char> destination = ScratchBuffer.AsSpan();
		if (!Value.TryFormat(destination, out var charsWritten, Format, Provider))
		{
			ThrowTryFormatException(Value.ToString(), Format);
		}
		return Insert(Index, destination.Slice(0, charsWritten));
	}

	public TextBuilder Insert(int Index, long Value, ReadOnlySpan<char> Format, IFormatProvider Provider = null)
	{
		Span<char> destination = ScratchBuffer.AsSpan();
		if (!Value.TryFormat(destination, out var charsWritten, Format, Provider))
		{
			ThrowTryFormatException(Value.ToString(), Format);
		}
		return Insert(Index, destination.Slice(0, charsWritten));
	}

	public TextBuilder Insert(int Index, uint Value, ReadOnlySpan<char> Format, IFormatProvider Provider = null)
	{
		Span<char> destination = ScratchBuffer.AsSpan();
		if (!Value.TryFormat(destination, out var charsWritten, Format, Provider))
		{
			ThrowTryFormatException(Value.ToString(), Format);
		}
		return Insert(Index, destination.Slice(0, charsWritten));
	}

	public TextBuilder Insert(int Index, ulong Value, ReadOnlySpan<char> Format, IFormatProvider Provider = null)
	{
		Span<char> destination = ScratchBuffer.AsSpan();
		if (!Value.TryFormat(destination, out var charsWritten, Format, Provider))
		{
			ThrowTryFormatException(Value.ToString(), Format);
		}
		return Insert(Index, destination.Slice(0, charsWritten));
	}

	public TextBuilder Insert(int Index, float Value, ReadOnlySpan<char> Format, IFormatProvider Provider = null)
	{
		Span<char> destination = ScratchBuffer.AsSpan();
		if (!Value.TryFormat(destination, out var charsWritten, Format, Provider))
		{
			ThrowTryFormatException(Value.ToString(CultureInfo.InvariantCulture), Format);
		}
		return Insert(Index, destination.Slice(0, charsWritten));
	}

	public TextBuilder Insert(int Index, double Value, ReadOnlySpan<char> Format, IFormatProvider Provider = null)
	{
		Span<char> destination = ScratchBuffer.AsSpan();
		if (!Value.TryFormat(destination, out var charsWritten, Format, Provider))
		{
			ThrowTryFormatException(Value.ToString(CultureInfo.InvariantCulture), Format);
		}
		return Insert(Index, destination.Slice(0, charsWritten));
	}

	public TextBuilder Insert(int Index, Guid Value, ReadOnlySpan<char> Format)
	{
		int length = Length;
		if (Index > length)
		{
			throw new ArgumentOutOfRangeException("Index");
		}
		char[] buffer = Buffer;
		int num = 68;
		if (length + num > buffer.Length)
		{
			Grow(num);
			buffer = Buffer;
		}
		if (Index < length)
		{
			Array.Copy(buffer, Index, buffer, Index + num, length - Index);
		}
		Span<char> destination = buffer.AsSpan(Index, num);
		if (!Value.TryFormat(destination, out var charsWritten, Format))
		{
			Array.Copy(buffer, Index + num, buffer, Index, length - Index);
			ThrowTryFormatException(Value.ToString(), Format);
		}
		if (charsWritten < num)
		{
			Array.Copy(buffer, Index + num, buffer, Index + charsWritten, length - Index);
		}
		Length = length + charsWritten;
		Version++;
		return this;
	}

	/// <inheritdoc cref="M:XRL.World.Text.TextBuilder.Append(System.Object)" path="remarks" />
	public TextBuilder Insert(int Index, object Value)
	{
		return Insert(Index, Value.ToString());
	}

	public TextBuilder InsertByType(int Index, object Value)
	{
		if (!(Value is string value))
		{
			if (!(Value is char value2))
			{
				if (!(Value is int value3))
				{
					if (!(Value is long value4))
					{
						if (!(Value is uint value5))
						{
							if (!(Value is ulong value6))
							{
								if (!(Value is float value7))
								{
									if (Value is double value8)
									{
										return Insert(Index, value8);
									}
									return Insert(Index, Value);
								}
								return Insert(Index, value7);
							}
							return Insert(Index, value6);
						}
						return Insert(Index, value5);
					}
					return Insert(Index, value4);
				}
				return Insert(Index, value3);
			}
			return Insert(Index, value2);
		}
		return Insert(Index, value);
	}

	public TextBuilder InsertUpper(int Index, ReadOnlySpan<char> Value)
	{
		int length = Length;
		if (Index > length)
		{
			throw new ArgumentOutOfRangeException("Index");
		}
		char[] buffer = Buffer;
		int length2 = Value.Length;
		if (length + length2 > buffer.Length)
		{
			Grow(length2);
			buffer = Buffer;
		}
		if (Index < length)
		{
			Array.Copy(buffer, Index, buffer, Index + length2, length - Index);
		}
		for (int i = 0; i < length2; i++)
		{
			buffer[Index++] = char.ToUpperInvariant(Value[i]);
		}
		Length = length + length2;
		Version++;
		return this;
	}

	public TextBuilder InsertLower(int Index, ReadOnlySpan<char> Value)
	{
		int length = Length;
		if (Index > length)
		{
			throw new ArgumentOutOfRangeException("Index");
		}
		char[] buffer = Buffer;
		int length2 = Value.Length;
		if (length + length2 > buffer.Length)
		{
			Grow(length2);
			buffer = Buffer;
		}
		if (Index < length)
		{
			Array.Copy(buffer, Index, buffer, Index + length2, length - Index);
		}
		for (int i = 0; i < length2; i++)
		{
			buffer[Index++] = char.ToLowerInvariant(Value[i]);
		}
		Length = length + length2;
		Version++;
		return this;
	}

	protected TextBuilder InsertInternal(int Index, ulong Value, bool Negative = false, bool Signed = false)
	{
		int length = Length;
		if (Index > length)
		{
			throw new ArgumentOutOfRangeException("Index");
		}
		char[] scratchBuffer = ScratchBuffer;
		int num = 0;
		do
		{
			ulong num2 = Value % 10;
			Value /= 10;
			scratchBuffer[num++] = (char)(48 + num2);
		}
		while (Value != 0);
		if (Negative)
		{
			scratchBuffer[num++] = '-';
		}
		else if (Signed)
		{
			scratchBuffer[num++] = '+';
		}
		char[] buffer = Buffer;
		if (length + num > buffer.Length)
		{
			Grow(num);
			buffer = Buffer;
		}
		if (Index < length)
		{
			Array.Copy(buffer, Index, buffer, Index + num, length - Index);
		}
		length += num;
		while (num > 0)
		{
			buffer[Index++] = scratchBuffer[--num];
		}
		Version++;
		Length = length;
		return this;
	}

	protected void ThrowTryFormatException(string Value, ReadOnlySpan<char> Format)
	{
		throw new ArgumentException("Unable to format '" + Value + "' to '" + Format.ToString() + "'", "Format");
	}

	public static TextBuilder Get(int Capacity = 64)
	{
		if (!Bag.TryTake(out var result))
		{
			result = new Entry();
		}
		result.Buffer = ArrayPool<char>.Shared.Rent(Capacity);
		if (Options.TextBuilderDebug)
		{
			Traces[result] = new StackTrace(1, fNeedFileInfo: true);
		}
		return result;
	}

	public static TextBuilder Get(ReadOnlySpan<char> Text)
	{
		return Get(Text.Length).Append(Text);
	}

	public static TextBuilder Get(string Text)
	{
		if (Text.IsNullOrEmpty())
		{
			return Get();
		}
		return Get(Text.Length).Append(Text);
	}

	public static TextBuilder Get(StringBuilder Text)
	{
		if (Text.IsNullOrEmpty())
		{
			return Get();
		}
		return Get(Text.Length).Append(Text);
	}

	protected TextBuilder(int ScratchCapacity = 32)
	{
		ScratchBuffer = new char[ScratchCapacity];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Span<char> AsSpan()
	{
		return new Span<char>(Buffer, 0, Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Span<char> AsSpan(int Start, int Length)
	{
		if (Start + Length > this.Length)
		{
			throw new ArgumentOutOfRangeException();
		}
		return new Span<char>(Buffer, Start, Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Span<char> AsSpan(int Start)
	{
		return new Span<char>(Buffer, Start, Length - Start);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ReadOnlySpan<char> AsReadOnlySpan()
	{
		return new ReadOnlySpan<char>(Buffer, 0, Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ReadOnlySpan<char> AsReadOnlySpan(int Start, int Length)
	{
		if (Start + Length > this.Length)
		{
			throw new ArgumentOutOfRangeException();
		}
		return new ReadOnlySpan<char>(Buffer, Start, Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ReadOnlySpan<char> AsReadOnlySpan(int Start)
	{
		return new ReadOnlySpan<char>(Buffer, Start, Length - Start);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public char[] GetArray()
	{
		return Buffer;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public char[] GetArray(int Capacity)
	{
		if (Capacity > Buffer.Length)
		{
			Grow(Capacity - Buffer.Length);
		}
		return Buffer;
	}

	protected void Grow(int Incoming)
	{
		int num = Buffer.Length;
		int minimumLength = Math.Max(num + Incoming, num * 2);
		char[] array = ArrayPool<char>.Shared.Rent(minimumLength);
		Array.Copy(Buffer, 0, array, 0, Length);
		ArrayPool<char>.Shared.Return(Buffer, clearArray: true);
		Buffer = array;
	}

	public bool Contains(char Value)
	{
		return IndexOf(Value) != -1;
	}

	public bool Contains(string Value, StringComparison Comparison = StringComparison.Ordinal)
	{
		return IndexOf(Value, Comparison) != -1;
	}

	public bool Contains(ReadOnlySpan<char> Value, StringComparison Comparison = StringComparison.Ordinal)
	{
		return IndexOf(Value, Comparison) != -1;
	}

	public bool StartsWith(char Value)
	{
		if (Length != 0)
		{
			return Buffer[0] == Value;
		}
		return false;
	}

	public bool StartsWith(ReadOnlySpan<char> Value)
	{
		return Buffer.AsSpan(0, Length).StartsWith(Value);
	}

	public bool StartsWith(ReadOnlySpan<char> Value, StringComparison Comparison)
	{
		return Buffer.AsSpan(0, Length).StartsWith(Value, Comparison);
	}

	public bool EndsWith(char Value)
	{
		if (Length != 0)
		{
			return Buffer[Length - 1] == Value;
		}
		return false;
	}

	public bool EndsWith(ReadOnlySpan<char> Value)
	{
		return Buffer.AsSpan(0, Length).EndsWith(Value);
	}

	public bool EndsWith(ReadOnlySpan<char> Value, StringComparison Comparison)
	{
		return Buffer.AsSpan(0, Length).EndsWith(Value, Comparison);
	}

	public void CopyTo(char[] Destination, int DestinationIndex)
	{
		Array.Copy(Buffer, 0, Destination, DestinationIndex, Length);
	}

	public void CopyTo(int SourceIndex, char[] Destination, int DestinationIndex, int Length)
	{
		Array.Copy(Buffer, SourceIndex, Destination, DestinationIndex, Length);
	}

	public int IndexOf(char Value)
	{
		return Array.IndexOf(Buffer, Value, 0, Length);
	}

	public int IndexOf(char Value, int Start)
	{
		return Array.IndexOf(Buffer, Value, Start, Length - Start);
	}

	public int IndexOf(char Value, int Start, int Length)
	{
		return Array.IndexOf(Buffer, Value, Start, Length);
	}

	public int IndexOf(string Value, StringComparison Comparison = StringComparison.Ordinal)
	{
		return AsSpan().IndexOf(Value, Comparison);
	}

	public int IndexOf(ReadOnlySpan<char> Value, StringComparison Comparison = StringComparison.Ordinal)
	{
		return AsSpan().IndexOf(Value, Comparison);
	}

	public int LastIndexOf(char Value)
	{
		return Array.LastIndexOf(Buffer, Value, 0, Length);
	}

	public int LastIndexOf(char Value, int Start)
	{
		return Array.LastIndexOf(Buffer, Value, Start, Length - Start);
	}

	public int LastIndexOf(char Value, int Start, int Length)
	{
		return Array.LastIndexOf(Buffer, Value, Start, Length);
	}

	public TextBuilder Remove(int Start, int Length)
	{
		int length = this.Length;
		if (Start < 0)
		{
			throw new ArgumentOutOfRangeException("Start");
		}
		if (Length < 0 || Length + Start > length)
		{
			throw new ArgumentOutOfRangeException("Length");
		}
		if (Length == 0)
		{
			return this;
		}
		if (Length == length && Start == 0)
		{
			this.Length = 0;
			return this;
		}
		Array.Copy(Buffer, Start + Length, Buffer, Start, length - Start - Length);
		this.Length = length - Length;
		Version++;
		return this;
	}

	public TextBuilder Replace(ReadOnlySpan<char> Old, ReadOnlySpan<char> New)
	{
		return Replace(Old, New, 0, Length);
	}

	public TextBuilder Replace(ReadOnlySpan<char> Old, ReadOnlySpan<char> New, int Start, int Length)
	{
		int length = this.Length;
		int length2 = Old.Length;
		int length3 = New.Length;
		if (length2 == 0)
		{
			throw new ArgumentException("Old span is empty", "Old");
		}
		if ((uint)Start > (uint)length)
		{
			throw new ArgumentOutOfRangeException("Start");
		}
		if (Length < 0 || Start > length - Length)
		{
			throw new ArgumentOutOfRangeException("Length");
		}
		while (true)
		{
			int num = Buffer.AsSpan(Start, Length).IndexOf(Old, StringComparison.Ordinal);
			if (num == -1)
			{
				break;
			}
			Start += num;
			Remove(Start, length2);
			Insert(Start, New);
			Start += length3;
			Length -= num + length3 - (length3 - length2);
		}
		return this;
	}

	public TextBuilder TrimStart()
	{
		char[] buffer = Buffer;
		int i = 0;
		int num = Length - 1;
		if (num == -1)
		{
			return this;
		}
		for (; char.IsWhiteSpace(buffer[i]); i++)
		{
		}
		int length = num - i + 1;
		Array.Copy(buffer, i, buffer, 0, length);
		Length = length;
		Version++;
		return this;
	}

	public TextBuilder TrimStart(char Value)
	{
		char[] buffer = Buffer;
		int i = 0;
		int num = Length - 1;
		if (num == -1)
		{
			return this;
		}
		for (; buffer[i] == Value; i++)
		{
		}
		int length = num - i + 1;
		Array.Copy(buffer, i, buffer, 0, length);
		Length = length;
		Version++;
		return this;
	}

	public TextBuilder TrimEnd()
	{
		char[] buffer = Buffer;
		int num = Length - 1;
		if (num == -1)
		{
			return this;
		}
		while (char.IsWhiteSpace(buffer[num]))
		{
			num--;
		}
		Length = num + 1;
		Version++;
		return this;
	}

	public TextBuilder TrimEnd(char Value)
	{
		char[] buffer = Buffer;
		int num = Length - 1;
		if (num == -1)
		{
			return this;
		}
		while (buffer[num] == Value)
		{
			num--;
		}
		Length = num + 1;
		Version++;
		return this;
	}

	public TextBuilder Trim()
	{
		char[] buffer = Buffer;
		int i = 0;
		int num = Length - 1;
		if (num == -1)
		{
			return this;
		}
		for (; char.IsWhiteSpace(buffer[i]); i++)
		{
		}
		while (num > i && char.IsWhiteSpace(buffer[num]))
		{
			num--;
		}
		int length = num - i + 1;
		Array.Copy(buffer, i, buffer, 0, length);
		Length = length;
		Version++;
		return this;
	}

	/// <summary>Trims both ends of whitespace and removes sequences of whitespace after each new line.</summary>
	public TextBuilder Unindent(bool Normalize = true)
	{
		Trim();
		char[] buffer = Buffer;
		int i = 0;
		int num = Length;
		int num2 = num;
		for (; i < num; i++)
		{
			char c = buffer[i];
			if (c == '\n')
			{
				if (i - num2 <= 1)
				{
					num2 = i;
					continue;
				}
			}
			else if (num2 >= num || char.IsWhiteSpace(c))
			{
				continue;
			}
			Remove(num2 + 1, i - num2 - 1);
			num -= i - num2 - 1;
			i = num2;
			num2 = num;
		}
		if (Normalize)
		{
			this.Normalize();
		}
		Version++;
		return this;
	}

	/// <summary>Remove carriage returns.</summary>
	public TextBuilder Normalize()
	{
		char[] buffer = Buffer;
		int num = Length;
		for (int num2 = num - 1; num2 >= 0; num2--)
		{
			if (buffer[num2] == '\r')
			{
				Array.Copy(buffer, num2 + 1, buffer, num2, num - num2 - 1);
				num--;
			}
		}
		Length = num;
		Version++;
		return this;
	}

	public bool Equals(string Other)
	{
		if (Other == null)
		{
			return false;
		}
		return Equals(Other, StringComparison.Ordinal);
	}

	public bool Equals(ReadOnlySpan<char> Other, StringComparison Comparison = StringComparison.Ordinal)
	{
		int length = Length;
		if (Other.Length != length)
		{
			return false;
		}
		return Other.Equals(Buffer.AsSpan(0, length), Comparison);
	}

	public TextBuilder Clear()
	{
		Length = 0;
		return this;
	}

	public TextBuilder Rebase(ReadOnlySpan<char> Value)
	{
		Length = 0;
		return Append(Value);
	}

	public override string ToString()
	{
		if (Length != 0)
		{
			return new string(Buffer, 0, Length);
		}
		return string.Empty;
	}

	public string ToString(int Start, int Length)
	{
		return new string(Buffer, Start, Length);
	}

	public virtual void Dispose(bool Destroy)
	{
		if (Buffer != null)
		{
			ArrayPool<char>.Shared.Return(Buffer, clearArray: true);
			Buffer = null;
			Length = 0;
			Version = 0;
		}
	}

	public virtual void Dispose()
	{
		Dispose(Destroy: false);
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<char> IEnumerable<char>.GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this);
	}

	~TextBuilder()
	{
		if (Finalizable || !GameManager.Playing)
		{
			return;
		}
		if (Options.TextBuilderDebug)
		{
			if (Traces.TryRemove(this, out var value))
			{
				MetricsManager.LogError(GetType().GetName(Full: true) + " leaked from " + value);
			}
			else
			{
				MetricsManager.LogError(GetType().GetName(Full: true) + " leaked and was not disposed, no trace was associated with this instance.");
			}
		}
		else
		{
			MetricsManager.LogError(GetType().GetName(Full: true) + " leaked and was not disposed, enable Text Builder leak analyzation in Options->Mods to get stack traces.");
		}
	}
}
