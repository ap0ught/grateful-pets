using System;

[Flags]
public enum exUpdateFlags
{
	None = 0,
	Index = 1,
	Vertex = 2,
	UV = 4,
	Color = 8,
	Normal = 0x10,
	Text = 0x20,
	Transparent = 0x40,
	VertexAndIndex = Index | Vertex,
	AllExcludeIndex = Vertex | UV | Color | Normal | Text | Transparent,
	All = AllExcludeIndex | Index
}
