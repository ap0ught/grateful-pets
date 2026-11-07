using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ConsoleLib.Console;
using XRL.Messages;
using XRL.Rules;

namespace XRL.UI;

public class ParticleManager
{
	public TextConsole _TextConsole;

	public ScreenBuffer _ScreenBuffer;

	public CleanQueue<BannerText> Banners = new CleanQueue<BannerText>();

	public List<TextParticle> Particles = new List<TextParticle>();

	public List<TileParticle> TileParticles = new List<TileParticle>();

	public List<RadialTextParticle> RadialParticles = new List<RadialTextParticle>();

	public List<SinusoidalTextParticle> SinusoidalParticles = new List<SinusoidalTextParticle>();

	[NonSerialized]
	public static CleanQueue<TextParticle> textParticlesQueue = new CleanQueue<TextParticle>();

	[NonSerialized]
	public static CleanQueue<TileParticle> tileParticlesQueue = new CleanQueue<TileParticle>();

	[NonSerialized]
	public static CleanQueue<SinusoidalTextParticle> sinParticlesQueue = new CleanQueue<SinusoidalTextParticle>();

	[NonSerialized]
	public static CleanQueue<RadialTextParticle> radParticlesQueue = new CleanQueue<RadialTextParticle>();

	private StringBuilder SB = new StringBuilder(1024);

	private List<TextParticle> Removed = new List<TextParticle>();

	private List<TileParticle> TileRemoved = new List<TileParticle>();

	private List<RadialTextParticle> RadialRemoved = new List<RadialTextParticle>();

	private List<SinusoidalTextParticle> SinRemoved = new List<SinusoidalTextParticle>();

	private static Stopwatch frameTimer = null;

	private static long lastElapsed = 0L;

	private static long accumulator = 0L;

	public void reset()
	{
		Banners = new CleanQueue<BannerText>();
		Particles = new List<TextParticle>();
		TileParticles = new List<TileParticle>();
		RadialParticles = new List<RadialTextParticle>();
		SinusoidalParticles = new List<SinusoidalTextParticle>();
	}

	public void Init(TextConsole TextConsole_, ScreenBuffer ScreenBuffer_)
	{
		_TextConsole = TextConsole_;
		_ScreenBuffer = ScreenBuffer_;
	}

	public void AddBanner(string Text, BannerTextType Type)
	{
		BannerText bannerText = new BannerText();
		bannerText.Block = new TextBlock(Text, 40, 25);
		bannerText.Type = Type;
		bannerText.x = 40 - bannerText.Block.Width / 2;
		bannerText.y = 23 - bannerText.Block.Height;
		Banners.Enqueue(bannerText);
		MessageQueue.AddPlayerMessage(Text);
	}

	public void AddRadial(string Text, float x, float y, float r, float d, float rdel, float ddel)
	{
		AddRadial(Text, x, y, r, d, rdel, ddel, 999, 0L);
	}

	public void Add(string Text, float x, float y, float xDel, float yDel)
	{
		Add(Text, x, y, xDel, yDel, 999, 0f, 0f, 0L);
	}

	public void Add(string Text, float x, float y, float xDel, float yDel, int Life)
	{
		Add(Text, x, y, xDel, yDel, Life, 0f, 0f, 0L);
	}

	public void Add(string Text, float x, float y, float xDel, float yDel, int Life, float xDelDel, float yDelDel, long delayMS = 0L)
	{
		if (Particles.Count <= 1000)
		{
			if (Text == "@" && Options.GetOption("OptionDisableTextWarpEffects") == "Yes")
			{
				Text = "*";
			}
			TextParticle textParticle = ((textParticlesQueue.Count <= 0) ? new TextParticle() : textParticlesQueue.Dequeue());
			textParticle.x = x;
			textParticle.y = y;
			textParticle.xDel = xDel;
			textParticle.yDel = yDel;
			textParticle.xDelDel = xDelDel;
			textParticle.yDelDel = yDelDel;
			textParticle.Text = Text;
			textParticle.TextBlock = new TextBlock(Text, 12, 8);
			textParticle.Life = Life;
			textParticle.DelayMS = delayMS;
			Particles.Add(textParticle);
		}
	}

	public void AddTile(string Tile, string ColorString, string DetailColor, float x, float y, float xDel, float yDel, int Life, float xDelDel, float yDelDel, bool HFlip = false, bool VFlip = false, long delayMS = 0L)
	{
		if (Particles.Count <= 1000)
		{
			TileParticle tileParticle = ((tileParticlesQueue.Count <= 0) ? new TileParticle() : tileParticlesQueue.Dequeue());
			tileParticle.x = x;
			tileParticle.y = y;
			tileParticle.xDel = xDel;
			tileParticle.yDel = yDel;
			tileParticle.xDelDel = xDelDel;
			tileParticle.yDelDel = yDelDel;
			tileParticle.Tile = Tile;
			tileParticle.ColorString = ColorString;
			tileParticle.DetailColor = DetailColor;
			tileParticle.Life = Life;
			tileParticle.bHFlipped = HFlip;
			tileParticle.bVFlipped = VFlip;
			tileParticle.DelayMS = delayMS;
			TileParticles.Add(tileParticle);
		}
	}

	public void AddSinusoidal(string Text, float x, float y, float amp, float sinstart, float sinincrement, float xamp, float yamp, float xdel, float ydel, int Life, long delayMS = 0L)
	{
		if (SinusoidalParticles.Count <= 100)
		{
			if (Text == "@" && Options.GetOption("OptionDisableTextWarpEffects") == "Yes")
			{
				Text = "*";
			}
			SinusoidalTextParticle sinusoidalTextParticle = ((sinParticlesQueue.Count <= 0) ? new SinusoidalTextParticle() : sinParticlesQueue.Dequeue());
			sinusoidalTextParticle.x = x;
			sinusoidalTextParticle.y = y;
			sinusoidalTextParticle.amp = amp;
			sinusoidalTextParticle.xamp = xamp;
			sinusoidalTextParticle.yamp = yamp;
			sinusoidalTextParticle.sinstart = sinstart;
			sinusoidalTextParticle.sinincrement = sinincrement;
			sinusoidalTextParticle.ydel = ydel;
			sinusoidalTextParticle.xdel = xdel;
			sinusoidalTextParticle.Text = Text;
			sinusoidalTextParticle.TextBlock = new TextBlock(Text, 12, 8);
			sinusoidalTextParticle.Life = Life;
			sinusoidalTextParticle.DelayMS = delayMS;
			SinusoidalParticles.Add(sinusoidalTextParticle);
		}
	}

	public void AddRadial(string Text, float x, float y, float r, float d, float rdel, float ddel, int Life, long delayMS = 0L)
	{
		if (RadialParticles.Count <= 100)
		{
			if (Text == "@" && Options.GetOption("OptionDisableTextWarpEffects") == "Yes")
			{
				Text = "*";
			}
			RadialTextParticle radialTextParticle = ((radParticlesQueue.Count <= 0) ? new RadialTextParticle() : radParticlesQueue.Dequeue());
			radialTextParticle.x = x;
			radialTextParticle.y = y;
			radialTextParticle.r = r;
			radialTextParticle.d = d;
			radialTextParticle.rdel = rdel;
			radialTextParticle.ddel = ddel;
			radialTextParticle.Text = Text;
			radialTextParticle.TextBlock = new TextBlock(Text, 12, 8);
			radialTextParticle.Life = Life;
			radialTextParticle.DelayMS = delayMS;
			RadialParticles.Add(radialTextParticle);
		}
	}

	public void Render(ScreenBuffer Buffer)
	{
		if (!Options.UseTextParticleVFX)
		{
			return;
		}
		if (Removed.Count > 0)
		{
			Removed.Clear();
		}
		if (TileRemoved.Count > 0)
		{
			TileRemoved.Clear();
		}
		if (RadialRemoved.Count > 0)
		{
			RadialRemoved.Clear();
		}
		if (SinRemoved.Count > 0)
		{
			SinRemoved.Clear();
		}
		Math.Max(1, Options.ThrottleAnimationRate / 20);
		for (int i = 0; i < Particles.Count && Particles[i].DelayMS <= 0; i++)
		{
			TextParticle textParticle = Particles[i];
			int num = Convert.ToInt32(textParticle.x);
			int num2 = Convert.ToInt32(textParticle.y);
			if (num < 0 || num > 79 || num2 < 0 || num2 > 24)
			{
				Removed.Add(textParticle);
			}
			if (num < 0)
			{
				num = 0;
			}
			if (num > 79)
			{
				num = 79;
			}
			if (num2 < 0)
			{
				num2 = 0;
			}
			if (num2 > 24)
			{
				num2 = 24;
			}
			if (textParticle.Life <= 0)
			{
				Removed.Add(textParticle);
			}
			Buffer.Goto(num, num2);
			if (textParticle.Text == "firefly")
			{
				Buffer.Buffer[num, num2].Char = 'ú';
				if (textParticle.Life < 10)
				{
					Buffer.Buffer[num, num2].SetColorsFromOldCharCode(ColorUtility.MakeColor(TextColor.Blue, TextColor.Black));
				}
				else if (textParticle.Life < 20)
				{
					Buffer.Buffer[num, num2].SetColorsFromOldCharCode(ColorUtility.MakeColor(ColorUtility.Bright(TextColor.Blue), TextColor.Black));
				}
				else if (textParticle.Life < 30)
				{
					Buffer.Buffer[num, num2].SetColorsFromOldCharCode(ColorUtility.MakeColor(TextColor.Cyan, TextColor.Black));
				}
				else if (textParticle.Life < 40)
				{
					Buffer.Buffer[num, num2].SetColorsFromOldCharCode(ColorUtility.MakeColor(ColorUtility.Bright(TextColor.Cyan), TextColor.Black));
				}
				else if (textParticle.Life < 50)
				{
					Buffer.Buffer[num, num2].SetColorsFromOldCharCode(ColorUtility.MakeColor(ColorUtility.Bright(TextColor.Blue), TextColor.Black));
				}
				else if (textParticle.Life < 60)
				{
					Buffer.Buffer[num, num2].SetColorsFromOldCharCode(ColorUtility.MakeColor(TextColor.Blue, TextColor.Black));
				}
			}
			else if (textParticle.Text[0] == '@')
			{
				int num3 = num;
				int num4 = num2;
				if ((float)num < textParticle.x)
				{
					num3--;
				}
				if ((float)num > textParticle.x)
				{
					num3++;
				}
				if ((float)num2 < textParticle.y)
				{
					num4--;
				}
				if ((float)num2 > textParticle.y)
				{
					num4++;
				}
				if (num3 < 0)
				{
					num3 = 0;
				}
				if (num3 > 79)
				{
					num3 = 79;
				}
				if (num4 < 0)
				{
					num4 = 0;
				}
				if (num4 > 24)
				{
					num4 = 24;
				}
				if ((float)num != textParticle.x || (float)num2 != textParticle.y)
				{
					Buffer.Buffer[num, num2].Copy(Buffer.Buffer[num3, num4]);
				}
			}
			else
			{
				Buffer.Write(textParticle.Text);
			}
		}
		for (int j = 0; j < TileParticles.Count && TileParticles[j].DelayMS <= 0; j++)
		{
			TileParticle tileParticle = TileParticles[j];
			int num5 = Convert.ToInt32(tileParticle.x);
			int num6 = Convert.ToInt32(tileParticle.y);
			if (num5 < 0 || num5 > 79 || num6 < 0 || num6 > 24)
			{
				TileRemoved.Add(tileParticle);
				continue;
			}
			if (tileParticle.Life <= 0)
			{
				TileRemoved.Add(tileParticle);
				continue;
			}
			Buffer.Goto(num5, num6);
			if (string.IsNullOrEmpty(tileParticle.Tile))
			{
				continue;
			}
			ConsoleChar consoleChar = Buffer.Buffer[num5, num6];
			consoleChar.Tile = tileParticle.Tile;
			if (!string.IsNullOrEmpty(tileParticle.ColorString))
			{
				int num7 = tileParticle.ColorString.LastIndexOf('&');
				if (num7 != -1 && ColorUtility.ColorMap.TryGetValue(tileParticle.ColorString[num7 + 1], out var value))
				{
					consoleChar.Foreground = value;
				}
				else
				{
					consoleChar.Foreground = The.Color.DarkBlack;
				}
				num7 = tileParticle.ColorString.LastIndexOf('^');
				if (num7 != -1 && ColorUtility.ColorMap.TryGetValue(tileParticle.ColorString[num7 + 1], out value))
				{
					consoleChar.Background = value;
				}
				else
				{
					consoleChar.Background = The.Color.DarkBlack;
				}
			}
			if (!string.IsNullOrEmpty(tileParticle.DetailColor))
			{
				consoleChar.Detail = ColorUtility.ColorMap[tileParticle.DetailColor[0]];
			}
			consoleChar.HFlip = tileParticle.bHFlipped;
			consoleChar.VFlip = tileParticle.bVFlipped;
		}
		for (int k = 0; k < RadialParticles.Count && RadialParticles[k].DelayMS <= 0; k++)
		{
			RadialTextParticle radialTextParticle = RadialParticles[k];
			int num8 = Convert.ToInt32(radialTextParticle.x) + (int)((double)radialTextParticle.d * Math.Cos(radialTextParticle.r));
			int num9 = Convert.ToInt32(radialTextParticle.y) + (int)((double)radialTextParticle.d * Math.Sin(radialTextParticle.r));
			if (radialTextParticle.Life <= 0)
			{
				RadialRemoved.Add(radialTextParticle);
			}
			if (radialTextParticle.d < 1f)
			{
				RadialRemoved.Add(radialTextParticle);
			}
			if (num8 < 0 || num8 > 79 || num9 < 0 || num9 > 24)
			{
				continue;
			}
			Buffer.Goto(num8, num9);
			if (radialTextParticle.Text[0] == '@')
			{
				int num10 = num8;
				int num11 = num9;
				if ((float)num8 < radialTextParticle.x)
				{
					num10--;
				}
				if ((float)num8 > radialTextParticle.x)
				{
					num10++;
				}
				if ((float)num9 < radialTextParticle.y)
				{
					num11--;
				}
				if ((float)num9 > radialTextParticle.y)
				{
					num11++;
				}
				if (num10 < 0)
				{
					num10 = 0;
				}
				if (num10 > 79)
				{
					num10 = 79;
				}
				if (num11 < 0)
				{
					num11 = 0;
				}
				if (num11 > 24)
				{
					num11 = 24;
				}
				if ((float)num8 != radialTextParticle.x || (float)num9 != radialTextParticle.y)
				{
					Buffer.Buffer[num8, num9].Char = Buffer.Buffer[num10, num11].Char;
					Buffer.Buffer[num8, num9].Tile = Buffer.Buffer[num10, num11].Tile;
					Buffer.Buffer[num8, num9].Foreground = Buffer.Buffer[num10, num11].Foreground;
					Buffer.Buffer[num8, num9].Background = Buffer.Buffer[num10, num11].Background;
				}
			}
			else
			{
				Buffer.Write(radialTextParticle.Text);
			}
		}
		for (int l = 0; l < SinusoidalParticles.Count && SinusoidalParticles[l].DelayMS <= 0; l++)
		{
			SinusoidalTextParticle sinusoidalTextParticle = SinusoidalParticles[l];
			int num12 = Convert.ToInt32(sinusoidalTextParticle.x) + (int)(Math.Sin(sinusoidalTextParticle.sinstart) * (double)sinusoidalTextParticle.amp * (double)sinusoidalTextParticle.xamp);
			int num13 = Convert.ToInt32(sinusoidalTextParticle.y) + (int)(Math.Sin(sinusoidalTextParticle.sinstart) * (double)sinusoidalTextParticle.amp * (double)sinusoidalTextParticle.yamp);
			if (sinusoidalTextParticle.Life <= 0)
			{
				SinRemoved.Add(sinusoidalTextParticle);
			}
			if (num12 < 0 || num12 > 79 || num13 < 0 || num13 > 24)
			{
				continue;
			}
			Buffer.Goto(num12, num13);
			if (sinusoidalTextParticle.Text[0] == '@')
			{
				int num14 = num12;
				int num15 = num13;
				if ((float)num12 < sinusoidalTextParticle.x)
				{
					num14--;
				}
				if ((float)num12 > sinusoidalTextParticle.x)
				{
					num14++;
				}
				if ((float)num13 < sinusoidalTextParticle.y)
				{
					num15--;
				}
				if ((float)num13 > sinusoidalTextParticle.y)
				{
					num15++;
				}
				if (num14 < 0)
				{
					num14 = 0;
				}
				if (num14 > 79)
				{
					num14 = 79;
				}
				if (num15 < 0)
				{
					num15 = 0;
				}
				if (num15 > 24)
				{
					num15 = 24;
				}
				if ((float)num12 != sinusoidalTextParticle.x || (float)num13 != sinusoidalTextParticle.y)
				{
					Buffer.Buffer[num12, num13].Char = Buffer.Buffer[num14, num15].Char;
					Buffer.Buffer[num12, num13].Tile = Buffer.Buffer[num14, num15].Tile;
					Buffer.Buffer[num12, num13].Foreground = Buffer.Buffer[num14, num15].Foreground;
					Buffer.Buffer[num12, num13].Background = Buffer.Buffer[num14, num15].Background;
				}
			}
			else
			{
				Buffer.Write(sinusoidalTextParticle.Text);
			}
		}
		if (Banners.Count > 0)
		{
			BannerText bannerText = Banners.Peek();
			string value2 = "&w";
			string value3 = "&W";
			string value4 = "&Y";
			if (bannerText.Type == BannerTextType.GoldenText)
			{
				value2 = "&w";
				value3 = "&W";
				value4 = "&Y";
			}
			if (bannerText.Complete)
			{
				bannerText.Life--;
				bannerText.Complete = true;
				int num16 = 0;
				foreach (string line in bannerText.Block.Lines)
				{
					Buffer.Goto(bannerText.x, bannerText.y + num16);
					Buffer.Write(line);
					num16++;
				}
			}
			else
			{
				bannerText.Complete = true;
				int num17 = 0;
				foreach (string line2 in bannerText.Block.Lines)
				{
					string text = ColorUtility.StripFormatting(line2);
					Buffer.Goto(bannerText.x, bannerText.y + num17);
					for (int m = 0; m < text.Length; m++)
					{
						int num18 = m / 4 * 3;
						int num19 = m / 4 * 3 + 1;
						int num20 = m / 4 * 3 + 2;
						if (bannerText.Time < num18)
						{
							bannerText.Complete = false;
							break;
						}
						if (bannerText.Time >= num20)
						{
							SB.Length = 0;
							SB.Append(value4);
							SB.Append(text[m]);
							Buffer.Write(SB.ToString());
						}
						else if (bannerText.Time >= num19)
						{
							SB.Length = 0;
							SB.Append(value3);
							SB.Append(text[m]);
							Buffer.Write(SB.ToString());
						}
						else
						{
							SB.Length = 0;
							SB.Append(value2);
							SB.Append("*");
							Buffer.Write(SB.ToString());
							AddRadial("&Y.", Buffer.X, Buffer.Y, Stat.RandomCosmetic(0, 5), Stat.RandomCosmetic(5, 10), 0f, 0.05f * (float)Stat.RandomCosmetic(3, 7), 15, 0L);
						}
					}
					num17++;
				}
				bannerText.Time++;
			}
			if (bannerText.Complete && bannerText.Life <= 0)
			{
				Banners.Dequeue();
			}
		}
		foreach (TextParticle item in Removed)
		{
			textParticlesQueue.Enqueue(item);
			Particles.Remove(item);
		}
		foreach (TileParticle item2 in TileRemoved)
		{
			tileParticlesQueue.Enqueue(item2);
			TileParticles.Remove(item2);
		}
		foreach (RadialTextParticle item3 in RadialRemoved)
		{
			radParticlesQueue.Enqueue(item3);
			RadialParticles.Remove(item3);
		}
		foreach (SinusoidalTextParticle item4 in SinRemoved)
		{
			sinParticlesQueue.Enqueue(item4);
			SinusoidalParticles.Remove(item4);
		}
	}

	public void Frame()
	{
		if (frameTimer == null)
		{
			frameTimer = new Stopwatch();
			frameTimer.Reset();
			frameTimer.Start();
		}
		long num = frameTimer.ElapsedMilliseconds - lastElapsed;
		lastElapsed = frameTimer.ElapsedMilliseconds;
		accumulator += num;
		if (accumulator > 128)
		{
			accumulator = 128L;
		}
		if (accumulator < 16)
		{
			return;
		}
		int num2 = (int)(accumulator / 16);
		accumulator -= 16 * num2;
		if (Removed.Count > 0)
		{
			Removed.Clear();
		}
		if (TileRemoved.Count > 0)
		{
			TileRemoved.Clear();
		}
		if (RadialRemoved.Count > 0)
		{
			RadialRemoved.Clear();
		}
		if (SinRemoved.Count > 0)
		{
			SinRemoved.Clear();
		}
		foreach (TextParticle particle in Particles)
		{
			if (particle.DelayMS > 0)
			{
				particle.DelayMS -= num;
				continue;
			}
			particle.Life -= num2;
			particle.x += particle.xDel * (float)num2;
			particle.y += particle.yDel * (float)num2;
			particle.xDel += particle.xDelDel * (float)num2;
			particle.yDel += particle.yDelDel * (float)num2;
			if (particle.x < 0f)
			{
				Removed.Add(particle);
			}
			else if (particle.x > 79f)
			{
				Removed.Add(particle);
			}
			else if (particle.y < 0f)
			{
				Removed.Add(particle);
			}
			else if (particle.y > 24f)
			{
				Removed.Add(particle);
			}
		}
		foreach (TileParticle tileParticle in TileParticles)
		{
			if (tileParticle.DelayMS > 0)
			{
				tileParticle.DelayMS -= num;
				continue;
			}
			tileParticle.Life -= num2;
			tileParticle.x += tileParticle.xDel * (float)num2;
			tileParticle.y += tileParticle.yDel * (float)num2;
			tileParticle.xDel += tileParticle.xDelDel * (float)num2;
			tileParticle.yDel += tileParticle.yDelDel * (float)num2;
			if (tileParticle.x < 0f)
			{
				TileRemoved.Add(tileParticle);
			}
			else if (tileParticle.x > 79f)
			{
				TileRemoved.Add(tileParticle);
			}
			else if (tileParticle.y < 0f)
			{
				TileRemoved.Add(tileParticle);
			}
			else if (tileParticle.y > 24f)
			{
				TileRemoved.Add(tileParticle);
			}
		}
		foreach (RadialTextParticle radialParticle in RadialParticles)
		{
			if (radialParticle.DelayMS > 0)
			{
				radialParticle.DelayMS -= num;
				continue;
			}
			radialParticle.Life -= num2;
			radialParticle.r += radialParticle.rdel * (float)num2;
			radialParticle.d += radialParticle.ddel * (float)num2;
			if (radialParticle.d < 1f)
			{
				RadialRemoved.Add(radialParticle);
			}
			while (radialParticle.r < 0f)
			{
				radialParticle.r += 6.28f;
			}
			while ((double)radialParticle.r > 6.28)
			{
				radialParticle.r -= 6.28f;
			}
		}
		foreach (SinusoidalTextParticle sinusoidalParticle in SinusoidalParticles)
		{
			if (sinusoidalParticle.DelayMS > 0)
			{
				sinusoidalParticle.DelayMS -= num;
				continue;
			}
			sinusoidalParticle.Life -= num2;
			sinusoidalParticle.x += sinusoidalParticle.xdel * (float)num2;
			sinusoidalParticle.y += sinusoidalParticle.ydel * (float)num2;
			sinusoidalParticle.sinstart += sinusoidalParticle.sinincrement * (float)num2;
			if (sinusoidalParticle.x < 0f)
			{
				SinRemoved.Add(sinusoidalParticle);
			}
			else if (sinusoidalParticle.x > 79f)
			{
				SinRemoved.Add(sinusoidalParticle);
			}
			else if (sinusoidalParticle.y < 0f)
			{
				SinRemoved.Add(sinusoidalParticle);
			}
			else if (sinusoidalParticle.y > 24f)
			{
				SinRemoved.Add(sinusoidalParticle);
			}
			while (sinusoidalParticle.sinstart < 0f)
			{
				sinusoidalParticle.sinstart += 6.28f;
			}
			while ((double)sinusoidalParticle.sinstart > 6.28)
			{
				sinusoidalParticle.sinstart -= 6.28f;
			}
		}
		foreach (TextParticle item in Removed)
		{
			Particles.Remove(item);
		}
		foreach (TileParticle item2 in TileRemoved)
		{
			TileParticles.Remove(item2);
		}
		foreach (SinusoidalTextParticle item3 in SinRemoved)
		{
			SinusoidalParticles.Remove(item3);
		}
		foreach (RadialTextParticle item4 in RadialRemoved)
		{
			RadialParticles.Remove(item4);
		}
	}
}
