using System;

namespace XRL;

public interface IReadOnlySpanConvertible<T>
{
	ReadOnlySpan<T> AsSpan();

	ReadOnlySpan<T> AsSpan(int Start);

	ReadOnlySpan<T> AsSpan(int Start, int Length);
}
