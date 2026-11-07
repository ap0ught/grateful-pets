using System;
using System.CodeDom.Compiler;
using System.Text;
using Occult.Engine.CodeGeneration;

namespace XRL.World.Encounters;

[Serializable]
[GenerateSerializationPartial]
public sealed class PsychicFaction : IComposite
{
	public string factionName;

	public string preferredMutation;

	public string mainColor;

	public string detailColor;

	public string a;

	public string e;

	public string i;

	public string o;

	public string u;

	public string A;

	public string E;

	public string I;

	public string O;

	public string U;

	public string c;

	public string f;

	public string n;

	public string t;

	public string y;

	public string B;

	public string C;

	public string Y;

	public string L;

	public string R;

	public string N;

	public int dimensionSymbol;

	public int cultSymbol;

	public string cultForm;

	public string dimensionName;

	public string dimensionSecretID;

	public int dimensionalWeaponIndex;

	public int dimensionalMissileWeaponIndex;

	public int dimensionalArmorIndex;

	public int dimensionalShieldIndex;

	public int dimensionalMiscIndex;

	public string dimensionalTraining;

	private static StringBuilder SB = new StringBuilder();

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public bool WantFieldReflection => false;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(factionName);
		Writer.WriteOptimized(preferredMutation);
		Writer.WriteOptimized(mainColor);
		Writer.WriteOptimized(detailColor);
		Writer.WriteOptimized(a);
		Writer.WriteOptimized(e);
		Writer.WriteOptimized(i);
		Writer.WriteOptimized(o);
		Writer.WriteOptimized(u);
		Writer.WriteOptimized(A);
		Writer.WriteOptimized(E);
		Writer.WriteOptimized(I);
		Writer.WriteOptimized(O);
		Writer.WriteOptimized(U);
		Writer.WriteOptimized(c);
		Writer.WriteOptimized(f);
		Writer.WriteOptimized(n);
		Writer.WriteOptimized(t);
		Writer.WriteOptimized(y);
		Writer.WriteOptimized(B);
		Writer.WriteOptimized(C);
		Writer.WriteOptimized(Y);
		Writer.WriteOptimized(L);
		Writer.WriteOptimized(R);
		Writer.WriteOptimized(N);
		Writer.WriteOptimized(dimensionSymbol);
		Writer.WriteOptimized(cultSymbol);
		Writer.WriteOptimized(cultForm);
		Writer.WriteOptimized(dimensionName);
		Writer.WriteOptimized(dimensionSecretID);
		Writer.WriteOptimized(dimensionalWeaponIndex);
		Writer.WriteOptimized(dimensionalMissileWeaponIndex);
		Writer.WriteOptimized(dimensionalArmorIndex);
		Writer.WriteOptimized(dimensionalShieldIndex);
		Writer.WriteOptimized(dimensionalMiscIndex);
		Writer.WriteOptimized(dimensionalTraining);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public void Read(SerializationReader Reader)
	{
		factionName = Reader.ReadOptimizedString();
		preferredMutation = Reader.ReadOptimizedString();
		mainColor = Reader.ReadOptimizedString();
		detailColor = Reader.ReadOptimizedString();
		a = Reader.ReadOptimizedString();
		e = Reader.ReadOptimizedString();
		i = Reader.ReadOptimizedString();
		o = Reader.ReadOptimizedString();
		u = Reader.ReadOptimizedString();
		A = Reader.ReadOptimizedString();
		E = Reader.ReadOptimizedString();
		I = Reader.ReadOptimizedString();
		O = Reader.ReadOptimizedString();
		U = Reader.ReadOptimizedString();
		c = Reader.ReadOptimizedString();
		f = Reader.ReadOptimizedString();
		n = Reader.ReadOptimizedString();
		t = Reader.ReadOptimizedString();
		y = Reader.ReadOptimizedString();
		B = Reader.ReadOptimizedString();
		C = Reader.ReadOptimizedString();
		Y = Reader.ReadOptimizedString();
		L = Reader.ReadOptimizedString();
		R = Reader.ReadOptimizedString();
		N = Reader.ReadOptimizedString();
		dimensionSymbol = Reader.ReadOptimizedInt32();
		cultSymbol = Reader.ReadOptimizedInt32();
		cultForm = Reader.ReadOptimizedString();
		dimensionName = Reader.ReadOptimizedString();
		dimensionSecretID = Reader.ReadOptimizedString();
		dimensionalWeaponIndex = Reader.ReadOptimizedInt32();
		dimensionalMissileWeaponIndex = Reader.ReadOptimizedInt32();
		dimensionalArmorIndex = Reader.ReadOptimizedInt32();
		dimensionalShieldIndex = Reader.ReadOptimizedInt32();
		dimensionalMiscIndex = Reader.ReadOptimizedInt32();
		dimensionalTraining = Reader.ReadOptimizedString();
	}

	public string Weirdify(string Text)
	{
		SB.Clear();
		bool flag = false;
		bool flag2 = false;
		int i = 0;
		for (int length = Text.Length; i < length; i++)
		{
			char c = Text[i];
			switch (c)
			{
			case '=':
				flag = !flag;
				break;
			case '*':
				flag2 = !flag2;
				break;
			default:
				if (!flag && !flag2)
				{
					c = c switch
					{
						'a' => a[0], 
						'A' => A[0], 
						'e' => e[0], 
						'E' => E[0], 
						'i' => this.i[0], 
						'I' => I[0], 
						'o' => o[0], 
						'O' => O[0], 
						'u' => u[0], 
						'U' => U[0], 
						'c' => this.c[0], 
						'f' => f[0], 
						'n' => n[0], 
						't' => t[0], 
						'y' => y[0], 
						'B' => B[0], 
						'C' => C[0], 
						'Y' => Y[0], 
						'L' => L[0], 
						'R' => R[0], 
						'N' => N[0], 
						_ => c, 
					};
				}
				break;
			}
			SB.Append(c);
		}
		return SB.ToString();
	}
}
