using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class AnimatedMaterialGeneric : IPart
{
	public static readonly int DEFAULT_ICON_COLOR_PRIORITY = 40;

	public int FrameOffset;

	public float SpeedMultiplier = 1.7f;

	public int AnimationLength = 60;

	public int LowFrameOffset = 1;

	public int HighFrameOffset = 1;

	public int ForegroundPriority = DEFAULT_ICON_COLOR_PRIORITY;

	public int DetailPriority = DEFAULT_ICON_COLOR_PRIORITY;

	public int BackgroundPriority = DEFAULT_ICON_COLOR_PRIORITY;

	public string ColorStringAnimationFrames;

	[NonSerialized]
	private List<int> ColorStringAnimationTimes;

	[NonSerialized]
	private List<string> ColorStringAnimationColors;

	public string BackgroundStringAnimationFrames;

	[NonSerialized]
	private List<int> BackgroundStringAnimationTimes;

	[NonSerialized]
	private List<string> BackgroundStringAnimationColors;

	public string DetailColorAnimationFrames;

	[NonSerialized]
	private List<int> DetailColorAnimationTimes;

	[NonSerialized]
	private List<string> DetailColorAnimationColors;

	public string TileColorAnimationFrames;

	[NonSerialized]
	private List<int> TileColorAnimationTimes;

	[NonSerialized]
	private List<string> TileColorAnimationColors;

	public string RenderStringAnimationFrames;

	[NonSerialized]
	private List<int> RenderStringAnimationTimes;

	[NonSerialized]
	private List<string> RenderStringAnimationColors;

	public string TileAnimationFrames;

	[NonSerialized]
	private List<int> TileAnimationTimes;

	[NonSerialized]
	private List<string> TileAnimationColors;

	public string RequiresOperationalActivePart;

	[NonSerialized]
	private bool? HasOperationalActivePart;

	public string RequiresUnpoweredActivePart;

	[NonSerialized]
	private bool? HasUnpoweredActivePart;

	public bool RequiresAnyUnpoweredActivePart;

	[NonSerialized]
	private bool? HasAnyUnpoweredActivePart;

	public bool ActivePartStatusIgnoreCharge;

	public bool ActivePartStatusIgnoreBreakage;

	public bool ActivePartStatusIgnoreRust;

	public bool ActivePartStatusIgnoreEMP;

	public bool ActivePartStatusIgnoreRealityStabilization;

	public bool ActivePartStatusIgnoreSubject;

	public bool ActivePartStatusIgnoreLocallyDefinedFailure;

	public string RequiresEvent;

	public string RequiresInverseEvent;

	[NonSerialized]
	private bool? HasEvent;

	[NonSerialized]
	private bool? HasInverseEvent;

	public string RequiresEffect;

	public string RequiresInverseEffect;

	[NonSerialized]
	private bool? HasEffect;

	[NonSerialized]
	private bool? HasInverseEffect;

	public int HFlipPermyriadChancePerFrame;

	public bool HFlipChanceMultiplyByWindSpeed;

	public int VFlipPermyriadChancePerFrame;

	public bool VFlipChanceMultiplyByWindSpeed;

	public bool HFlipActive;

	public bool VFlipActive;

	public string AltRenderString;

	public int AltRenderStringPermyriadChancePerFrame;

	public bool AltRenderStringChanceMultiplyByWindSpeed;

	public bool AltRenderStringActive;

	public int LastFrame;

	public bool MustBeUnderstood;

	[NonSerialized]
	private bool? IsUnderstood;

	private int lastFrame = int.MinValue;

	public AnimatedMaterialGeneric()
	{
		FrameOffset = Stat.RandomCosmetic(0, 60);
	}

	public override bool SameAs(IPart p)
	{
		AnimatedMaterialGeneric animatedMaterialGeneric = p as AnimatedMaterialGeneric;
		if (animatedMaterialGeneric.AnimationLength != AnimationLength)
		{
			return false;
		}
		if (animatedMaterialGeneric.LowFrameOffset != LowFrameOffset)
		{
			return false;
		}
		if (animatedMaterialGeneric.HighFrameOffset != HighFrameOffset)
		{
			return false;
		}
		if (animatedMaterialGeneric.ColorStringAnimationFrames != ColorStringAnimationFrames)
		{
			return false;
		}
		if (animatedMaterialGeneric.BackgroundStringAnimationFrames != BackgroundStringAnimationFrames)
		{
			return false;
		}
		if (animatedMaterialGeneric.DetailColorAnimationFrames != DetailColorAnimationFrames)
		{
			return false;
		}
		if (animatedMaterialGeneric.TileColorAnimationFrames != TileColorAnimationFrames)
		{
			return false;
		}
		if (animatedMaterialGeneric.RenderStringAnimationFrames != RenderStringAnimationFrames)
		{
			return false;
		}
		if (animatedMaterialGeneric.TileAnimationFrames != TileAnimationFrames)
		{
			return false;
		}
		if (animatedMaterialGeneric.RequiresOperationalActivePart != RequiresOperationalActivePart)
		{
			return false;
		}
		if (animatedMaterialGeneric.RequiresUnpoweredActivePart != RequiresUnpoweredActivePart)
		{
			return false;
		}
		if (animatedMaterialGeneric.RequiresAnyUnpoweredActivePart != RequiresAnyUnpoweredActivePart)
		{
			return false;
		}
		if (animatedMaterialGeneric.ActivePartStatusIgnoreCharge != ActivePartStatusIgnoreCharge)
		{
			return false;
		}
		if (animatedMaterialGeneric.ActivePartStatusIgnoreBreakage != ActivePartStatusIgnoreBreakage)
		{
			return false;
		}
		if (animatedMaterialGeneric.ActivePartStatusIgnoreRust != ActivePartStatusIgnoreRust)
		{
			return false;
		}
		if (animatedMaterialGeneric.ActivePartStatusIgnoreEMP != ActivePartStatusIgnoreEMP)
		{
			return false;
		}
		if (animatedMaterialGeneric.ActivePartStatusIgnoreRealityStabilization != ActivePartStatusIgnoreRealityStabilization)
		{
			return false;
		}
		if (animatedMaterialGeneric.ActivePartStatusIgnoreLocallyDefinedFailure != ActivePartStatusIgnoreLocallyDefinedFailure)
		{
			return false;
		}
		if (animatedMaterialGeneric.ActivePartStatusIgnoreSubject != ActivePartStatusIgnoreSubject)
		{
			return false;
		}
		if (animatedMaterialGeneric.RequiresEvent != RequiresEvent)
		{
			return false;
		}
		if (animatedMaterialGeneric.RequiresInverseEvent != RequiresInverseEvent)
		{
			return false;
		}
		if (animatedMaterialGeneric.RequiresEffect != RequiresEffect)
		{
			return false;
		}
		if (animatedMaterialGeneric.RequiresInverseEffect != RequiresInverseEffect)
		{
			return false;
		}
		if (animatedMaterialGeneric.HFlipPermyriadChancePerFrame != HFlipPermyriadChancePerFrame)
		{
			return false;
		}
		if (animatedMaterialGeneric.HFlipChanceMultiplyByWindSpeed != HFlipChanceMultiplyByWindSpeed)
		{
			return false;
		}
		if (animatedMaterialGeneric.VFlipPermyriadChancePerFrame != VFlipPermyriadChancePerFrame)
		{
			return false;
		}
		if (animatedMaterialGeneric.VFlipChanceMultiplyByWindSpeed != VFlipChanceMultiplyByWindSpeed)
		{
			return false;
		}
		if (animatedMaterialGeneric.AltRenderString != AltRenderString)
		{
			return false;
		}
		if (animatedMaterialGeneric.AltRenderStringPermyriadChancePerFrame != AltRenderStringPermyriadChancePerFrame)
		{
			return false;
		}
		if (animatedMaterialGeneric.AltRenderStringChanceMultiplyByWindSpeed != AltRenderStringChanceMultiplyByWindSpeed)
		{
			return false;
		}
		if (animatedMaterialGeneric.MustBeUnderstood != MustBeUnderstood)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		TurnProcess();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BootSequenceDoneEvent.ID && ID != BootSequenceInitializedEvent.ID && ID != CellChangedEvent.ID && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID)
		{
			return ID == PowerSwitchFlippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BootSequenceDoneEvent E)
	{
		FlushNonEffectStateCaches();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BootSequenceInitializedEvent E)
	{
		FlushNonEffectStateCaches();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CellChangedEvent E)
	{
		FlushNonEffectStateCaches();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		FlushStateCaches();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		FlushStateCaches();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(PowerSwitchFlippedEvent E)
	{
		FlushNonEffectStateCaches();
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	private ActivePartStatus? StatusOf(IActivePart p)
	{
		return p?.GetActivePartStatus(UseCharge: false, ActivePartStatusIgnoreCharge, IgnoreLiquid: false, IgnoreBootSequence: false, ActivePartStatusIgnoreBreakage, ActivePartStatusIgnoreRust, ActivePartStatusIgnoreEMP, ActivePartStatusIgnoreRealityStabilization, ActivePartStatusIgnoreSubject, ActivePartStatusIgnoreLocallyDefinedFailure, 1, null, UseChargeIfUnpowered: false, 0L);
	}

	private ActivePartStatus? StatusOf(string PartName)
	{
		return StatusOf(ParentObject.GetPart(PartName) as IActivePart);
	}

	public override bool Render(RenderEvent E)
	{
		if (!RequiresOperationalActivePart.IsNullOrEmpty())
		{
			if (!HasOperationalActivePart.HasValue)
			{
				HasOperationalActivePart = StatusOf(RequiresOperationalActivePart) == ActivePartStatus.Operational;
			}
			if (HasOperationalActivePart != true)
			{
				return true;
			}
		}
		if (!RequiresUnpoweredActivePart.IsNullOrEmpty())
		{
			if (!HasUnpoweredActivePart.HasValue)
			{
				HasUnpoweredActivePart = StatusOf(RequiresUnpoweredActivePart) == ActivePartStatus.Unpowered;
			}
			if (HasUnpoweredActivePart != true)
			{
				return true;
			}
		}
		if (RequiresAnyUnpoweredActivePart)
		{
			if (!HasAnyUnpoweredActivePart.HasValue)
			{
				if (HasUnpoweredActivePart == true)
				{
					HasAnyUnpoweredActivePart = true;
				}
				else
				{
					int i = 0;
					for (int count = ParentObject.PartsList.Count; i < count; i++)
					{
						if (ParentObject.PartsList[i] is IActivePart p && StatusOf(p) == ActivePartStatus.Unpowered)
						{
							HasAnyUnpoweredActivePart = true;
							break;
						}
					}
				}
			}
			if (HasAnyUnpoweredActivePart != true)
			{
				return true;
			}
		}
		if (!RequiresEvent.IsNullOrEmpty())
		{
			if (!HasEvent.HasValue)
			{
				HasEvent = ParentObject.FireEvent(RequiresEvent);
			}
			if (HasEvent != true)
			{
				return true;
			}
		}
		if (!RequiresInverseEvent.IsNullOrEmpty())
		{
			if (!HasInverseEvent.HasValue)
			{
				HasInverseEvent = !ParentObject.FireEvent(RequiresInverseEvent);
			}
			if (HasInverseEvent != true)
			{
				return true;
			}
		}
		if (!RequiresEffect.IsNullOrEmpty())
		{
			if (!HasEffect.HasValue)
			{
				HasEffect = ParentObject.HasEffect(RequiresEffect);
			}
			if (HasEffect != true)
			{
				return true;
			}
		}
		if (!RequiresInverseEffect.IsNullOrEmpty())
		{
			if (!HasInverseEffect.HasValue)
			{
				HasInverseEffect = !ParentObject.HasEffect(RequiresInverseEffect);
			}
			if (HasInverseEffect != true)
			{
				return true;
			}
		}
		if (MustBeUnderstood)
		{
			if (!IsUnderstood.HasValue)
			{
				IsUnderstood = ParentObject.Understood();
			}
			if (IsUnderstood != true)
			{
				return true;
			}
		}
		string text = null;
		string text2 = null;
		string text3 = null;
		int num = (XRLCore.GetCurrentFrameAtFPS(60, SpeedMultiplier) + FrameOffset) % AnimationLength;
		FrameOffset = 0;
		if (num != lastFrame)
		{
			num = (lastFrame = num + Stat.RandomCosmetic(LowFrameOffset, HighFrameOffset));
		}
		if (ColorStringAnimationTimes == null && !ColorStringAnimationFrames.IsNullOrEmpty())
		{
			if (ColorStringAnimationFrames == "0=default" || ColorStringAnimationFrames == "disable")
			{
				ColorStringAnimationTimes = new List<int>();
				ColorStringAnimationColors = new List<string>();
			}
			else
			{
				string[] array = ColorStringAnimationFrames.Split(',');
				ColorStringAnimationTimes = new List<int>(array.Length);
				ColorStringAnimationColors = new List<string>(array.Length);
				string[] array2 = array;
				for (int j = 0; j < array2.Length; j++)
				{
					string[] array3 = array2[j].Split('=');
					ColorStringAnimationTimes.Add(int.Parse(array3[0]));
					ColorStringAnimationColors.Add(array3[1]);
				}
			}
		}
		if (TileColorAnimationTimes == null && !TileColorAnimationFrames.IsNullOrEmpty())
		{
			if (TileColorAnimationFrames == "0=default" || TileColorAnimationFrames == "disable")
			{
				TileColorAnimationTimes = new List<int>();
				TileColorAnimationColors = new List<string>();
			}
			else
			{
				string[] array4 = TileColorAnimationFrames.Split(',');
				TileColorAnimationTimes = new List<int>(array4.Length);
				TileColorAnimationColors = new List<string>(array4.Length);
				string[] array2 = array4;
				for (int j = 0; j < array2.Length; j++)
				{
					string[] array5 = array2[j].Split('=');
					TileColorAnimationTimes.Add(int.Parse(array5[0]));
					TileColorAnimationColors.Add(array5[1]);
				}
			}
		}
		if (E.ColorsVisible)
		{
			if (TileColorAnimationTimes != null && Globals.RenderMode == RenderModeType.Tiles)
			{
				string text4 = null;
				for (int k = 0; k < TileColorAnimationTimes.Count && num >= TileColorAnimationTimes[k]; k++)
				{
					text4 = TileColorAnimationColors[k];
				}
				switch (text4)
				{
				case "default":
					text = null;
					break;
				case "base":
					text = ParentObject.Render.GetRenderColor();
					break;
				case "liquid":
					text = "&" + ParentObject.GetLiquidColor();
					break;
				default:
					text = text4;
					break;
				case null:
					break;
				}
			}
			else if (ColorStringAnimationTimes != null)
			{
				string text5 = null;
				for (int l = 0; l < ColorStringAnimationTimes.Count && num >= ColorStringAnimationTimes[l]; l++)
				{
					text5 = ColorStringAnimationColors[l];
				}
				switch (text5)
				{
				case "default":
					text = null;
					break;
				case "base":
					text = ParentObject.Render.GetRenderColor();
					break;
				case "liquid":
					text = "&" + ParentObject.GetLiquidColor();
					break;
				default:
					text = text5;
					break;
				case null:
					break;
				}
			}
		}
		if (BackgroundStringAnimationTimes == null && !BackgroundStringAnimationFrames.IsNullOrEmpty())
		{
			string[] array6 = BackgroundStringAnimationFrames.Split(',');
			BackgroundStringAnimationTimes = new List<int>(array6.Length);
			BackgroundStringAnimationColors = new List<string>(array6.Length);
			string[] array2 = array6;
			for (int j = 0; j < array2.Length; j++)
			{
				string[] array7 = array2[j].Split('=');
				BackgroundStringAnimationTimes.Add(int.Parse(array7[0]));
				BackgroundStringAnimationColors.Add(array7[1]);
			}
		}
		if (BackgroundStringAnimationTimes != null && E.ColorsVisible)
		{
			string text6 = null;
			int m = 0;
			for (int count2 = BackgroundStringAnimationTimes.Count; m < count2 && num >= BackgroundStringAnimationTimes[m]; m++)
			{
				text6 = BackgroundStringAnimationColors[m];
			}
			if (text6 != null && text6 != "default")
			{
				text2 = text6;
			}
		}
		if (DetailColorAnimationTimes == null && !DetailColorAnimationFrames.IsNullOrEmpty())
		{
			string[] array8 = DetailColorAnimationFrames.Split(',');
			DetailColorAnimationTimes = new List<int>(array8.Length);
			DetailColorAnimationColors = new List<string>(array8.Length);
			string[] array2 = array8;
			for (int j = 0; j < array2.Length; j++)
			{
				string[] array9 = array2[j].Split('=');
				DetailColorAnimationTimes.Add(int.Parse(array9[0]));
				DetailColorAnimationColors.Add(array9[1]);
			}
		}
		if (DetailColorAnimationTimes != null && E.ColorsVisible)
		{
			string text7 = null;
			int n = 0;
			for (int count3 = DetailColorAnimationTimes.Count; n < count3 && num >= DetailColorAnimationTimes[n]; n++)
			{
				text7 = DetailColorAnimationColors[n];
			}
			switch (text7)
			{
			case "default":
				text3 = null;
				break;
			case "base":
				text3 = ParentObject.Render.DetailColor;
				break;
			case "liquid":
				text3 = ParentObject.GetLiquidColor();
				break;
			default:
				text3 = text7;
				break;
			case null:
				break;
			}
		}
		if (RenderStringAnimationTimes == null && !RenderStringAnimationFrames.IsNullOrEmpty())
		{
			string[] array10 = RenderStringAnimationFrames.Split(',');
			RenderStringAnimationTimes = new List<int>(array10.Length);
			RenderStringAnimationColors = new List<string>(array10.Length);
			string[] array2 = array10;
			for (int j = 0; j < array2.Length; j++)
			{
				string[] array11 = array2[j].Split('=');
				RenderStringAnimationTimes.Add(int.Parse(array11[0]));
				RenderStringAnimationColors.Add(array11[1]);
			}
		}
		if (RenderStringAnimationTimes != null)
		{
			string text8 = null;
			int num2 = 0;
			for (int count4 = RenderStringAnimationTimes.Count; num2 < count4 && num >= RenderStringAnimationTimes[num2]; num2++)
			{
				text8 = RenderStringAnimationColors[num2];
			}
			switch (text8)
			{
			case "default":
				E.RenderString = ParentObject.Render.RenderString;
				break;
			case "alt":
				AltRenderStringActive = !AltRenderStringActive;
				break;
			default:
				E.RenderString = text8;
				break;
			case null:
				break;
			}
		}
		if (TileAnimationTimes == null && !TileAnimationFrames.IsNullOrEmpty())
		{
			string[] array12 = TileAnimationFrames.Split(',');
			TileAnimationTimes = new List<int>(array12.Length);
			TileAnimationColors = new List<string>(array12.Length);
			string[] array2 = array12;
			for (int j = 0; j < array2.Length; j++)
			{
				string[] array13 = array2[j].Split('=');
				TileAnimationTimes.Add(int.Parse(array13[0]));
				TileAnimationColors.Add(array13[1]);
			}
		}
		if (TileAnimationTimes != null)
		{
			string text9 = null;
			int num3 = 0;
			for (int count5 = TileAnimationTimes.Count; num3 < count5 && num >= TileAnimationTimes[num3]; num3++)
			{
				text9 = TileAnimationColors[num3];
			}
			switch (text9)
			{
			case "default":
				E.Tile = ParentObject.Render.Tile;
				break;
			case "hflip":
				HFlipActive = !HFlipActive;
				break;
			case "vflip":
				VFlipActive = !VFlipActive;
				break;
			default:
				E.Tile = text9;
				break;
			case null:
				break;
			}
		}
		if (HFlipPermyriadChancePerFrame > 0)
		{
			int num4 = HFlipPermyriadChancePerFrame * Math.Abs(num - LastFrame);
			if (HFlipChanceMultiplyByWindSpeed && ParentObject.CurrentZone != null)
			{
				num4 *= ParentObject.CurrentZone.CurrentWindSpeed;
			}
			if (num4.in10000())
			{
				HFlipActive = !HFlipActive;
			}
		}
		if (VFlipPermyriadChancePerFrame > 0)
		{
			int num5 = VFlipPermyriadChancePerFrame * Math.Abs(num - LastFrame);
			if (VFlipChanceMultiplyByWindSpeed && ParentObject.CurrentZone != null)
			{
				num5 *= ParentObject.CurrentZone.CurrentWindSpeed;
			}
			if (num5.in10000())
			{
				VFlipActive = !VFlipActive;
			}
		}
		if (AltRenderStringPermyriadChancePerFrame > 0)
		{
			int num6 = AltRenderStringPermyriadChancePerFrame * Math.Abs(num - LastFrame);
			if (AltRenderStringChanceMultiplyByWindSpeed && ParentObject.CurrentZone != null)
			{
				num6 *= ParentObject.CurrentZone.CurrentWindSpeed;
			}
			if (num6.in10000())
			{
				AltRenderStringActive = !AltRenderStringActive;
			}
		}
		if (HFlipActive)
		{
			E.HFlip = !E.HFlip;
		}
		if (VFlipActive)
		{
			E.VFlip = !E.VFlip;
		}
		if (AltRenderStringActive)
		{
			E.RenderString = AltRenderString;
		}
		if (!text.IsNullOrEmpty() || !text2.IsNullOrEmpty() || !text3.IsNullOrEmpty())
		{
			E.ApplyColors(text, text2, text3, ForegroundPriority, BackgroundPriority, DetailPriority);
		}
		LastFrame = num;
		return true;
	}

	private void TurnProcess()
	{
		FlushNonEffectStateCaches();
	}

	private void FlushStateCaches()
	{
		HasEffect = null;
		HasInverseEffect = null;
		FlushNonEffectStateCaches();
	}

	private void FlushNonEffectStateCaches()
	{
		HasOperationalActivePart = null;
		HasUnpoweredActivePart = null;
		HasAnyUnpoweredActivePart = null;
		HasEvent = null;
		HasInverseEvent = null;
		IsUnderstood = null;
	}
}
