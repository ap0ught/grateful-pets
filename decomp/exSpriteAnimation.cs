using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(exSprite))]
[AddComponentMenu("ex2D/Sprite Animation")]
public class exSpriteAnimation : MonoBehaviour
{
	public exSpriteAnimationClip defaultAnimation;

	public List<exSpriteAnimationClip> animations = new List<exSpriteAnimationClip>();

	public bool playAutomatically = true;

	private Dictionary<string, exSpriteAnimationState> nameToState;

	private exSpriteAnimationState curAnimation;

	private exSprite sprite_;

	private exTextureInfo defaultTextureInfo;

	private int lastFrameIndex = -1;

	private int curIndex = -1;

	public exSprite sprite => sprite_;

	public exSpriteAnimationState this[string _name]
	{
		get
		{
			Init();
			if (nameToState.TryGetValue(_name, out var value))
			{
				return value;
			}
			return null;
		}
	}

	private void Awake()
	{
		Init();
		if (enabled && playAutomatically && defaultAnimation != null)
		{
			Play(defaultAnimation.name, 0f);
		}
	}

	private void LateUpdate()
	{
		if (curAnimation != null)
		{
			float deltaTime = Time.deltaTime * curAnimation.speed;
			Step(deltaTime);
		}
	}

	private void OnDisable()
	{
		Stop();
	}

	public void Play(string _name)
	{
		Play(_name, 0f);
	}

	public void Play(string _name, float _time)
	{
		exSpriteAnimationState animation = GetAnimation(_name);
		if (animation != null)
		{
			Play(animation, _time);
		}
	}

	public void PlayByFrame(string _name, int _frame)
	{
		exSpriteAnimationState animation = GetAnimation(_name);
		if (animation != null)
		{
			float num = 1f / animation.clip.frameRate;
			float time = (float)_frame * num;
			Play(animation, time);
		}
	}

	public void Stop()
	{
		Stop(curAnimation);
		curAnimation = null;
	}

	public void Stop(exSpriteAnimationState _animState)
	{
		if (_animState != null)
		{
			if (_animState == curAnimation)
			{
				curAnimation = null;
			}
			_animState.time = 0f;
			switch (_animState.stopAction)
			{
			case exSpriteAnimationClip.StopAction.DefaultSprite:
				sprite_.textureInfo = defaultTextureInfo;
				break;
			case exSpriteAnimationClip.StopAction.Hide:
				sprite_.enabled = false;
				break;
			case exSpriteAnimationClip.StopAction.Destroy:
				Object.Destroy(gameObject);
				break;
			case exSpriteAnimationClip.StopAction.DoNothing:
				break;
			}
		}
	}

	public void SetDefaultSprite()
	{
		if (sprite_ != null)
		{
			sprite_.textureInfo = defaultTextureInfo;
		}
	}

	public void UpdateDefaultSprite(exTextureInfo _textureInfo)
	{
		defaultTextureInfo = _textureInfo;
	}

	public void PlayDefault()
	{
		if (defaultAnimation != null)
		{
			Play(defaultAnimation.name, 0f);
		}
	}

	public exSpriteAnimationState GetAnimation(string _name)
	{
		return this[_name];
	}

	public exSpriteAnimationState GetCurrentAnimation()
	{
		return curAnimation;
	}

	public bool IsPlaying(string _name = "")
	{
		if (string.IsNullOrEmpty(_name))
		{
			if (enabled)
			{
				return curAnimation != null;
			}
			return false;
		}
		if (enabled && curAnimation != null)
		{
			return curAnimation.name == _name;
		}
		return false;
	}

	public exSpriteAnimationClip.FrameInfo GetCurFrameInfo()
	{
		if (curAnimation != null && curIndex < curAnimation.clip.frameInfos.Count)
		{
			return curAnimation.clip.frameInfos[curIndex];
		}
		return null;
	}

	public int GetCurFrameIndex()
	{
		return curIndex;
	}

	public exSpriteAnimationState AddAnimation(exSpriteAnimationClip _animClip)
	{
		return AddAnimation(_animClip.name, _animClip);
	}

	public exSpriteAnimationState AddAnimation(string _name, exSpriteAnimationClip _animClip)
	{
		Init();
		exSpriteAnimationState value = null;
		if (nameToState.TryGetValue(_name, out value))
		{
			if ((object)value.clip == _animClip)
			{
				return value;
			}
			animations[animations.IndexOf(value.clip)] = _animClip;
		}
		else if (animations.IndexOf(_animClip) != -1)
		{
			value = nameToState[_name];
			if ((object)value.clip == _animClip)
			{
				return value;
			}
		}
		else
		{
			animations.Add(_animClip);
		}
		exSpriteAnimationState exSpriteAnimationState2 = new exSpriteAnimationState(_name, _animClip);
		nameToState[_name] = exSpriteAnimationState2;
		return exSpriteAnimationState2;
	}

	public void RemoveAnimation(exSpriteAnimationClip _animClip)
	{
		if (animations.IndexOf(_animClip) != -1)
		{
			Init();
			animations.Remove(_animClip);
			nameToState.Remove(_animClip.name);
		}
	}

	public void Sample(exSpriteAnimationState _animState)
	{
		if (_animState != null)
		{
			if (curAnimation != _animState)
			{
				curAnimation = _animState;
				lastFrameIndex = -1;
			}
			Sample();
		}
	}

	public void Step(exSpriteAnimationState _animState, float _deltaTime)
	{
		if (_animState != null)
		{
			if (curAnimation != _animState)
			{
				curAnimation = _animState;
				lastFrameIndex = -1;
			}
			Step(_deltaTime);
		}
	}

	private void Init()
	{
		if (nameToState != null)
		{
			return;
		}
		sprite_ = GetComponent<exSprite>();
		defaultTextureInfo = sprite_.textureInfo;
		nameToState = new Dictionary<string, exSpriteAnimationState>();
		for (int i = 0; i < animations.Count; i++)
		{
			exSpriteAnimationClip exSpriteAnimationClip2 = animations[i];
			if (exSpriteAnimationClip2 != null)
			{
				exSpriteAnimationState exSpriteAnimationState2 = new exSpriteAnimationState(exSpriteAnimationClip2);
				nameToState[exSpriteAnimationState2.name] = exSpriteAnimationState2;
				if ((object)defaultAnimation == exSpriteAnimationClip2)
				{
					curAnimation = exSpriteAnimationState2;
					lastFrameIndex = -1;
				}
			}
		}
	}

	private void Play(exSpriteAnimationState _animState, float _time)
	{
		curAnimation = _animState;
		if (curAnimation != null)
		{
			curIndex = -1;
			curAnimation.time = _time;
			Sample();
		}
	}

	private void Step(float _deltaTime)
	{
		if (curAnimation != null)
		{
			int num = curAnimation.frame;
			if (lastFrameIndex == num)
			{
				num++;
			}
			curAnimation.time += _deltaTime;
			Sample();
			bool flag = false;
			if (curAnimation.wrapMode == WrapMode.Once || curAnimation.wrapMode == WrapMode.Default || curAnimation.wrapMode == WrapMode.ClampForever)
			{
				if (curAnimation.speed > 0f && curAnimation.frame >= curAnimation.totalFrames)
				{
					if (curAnimation.wrapMode == WrapMode.ClampForever)
					{
						flag = false;
						curAnimation.frame = curAnimation.totalFrames;
						curAnimation.time = (float)curAnimation.frame / curAnimation.clip.frameRate;
					}
					else
					{
						flag = true;
						curAnimation.frame = curAnimation.totalFrames;
					}
				}
				else if (curAnimation.speed < 0f && curAnimation.frame < 0)
				{
					if (curAnimation.wrapMode == WrapMode.ClampForever)
					{
						flag = false;
						curAnimation.time = 0f;
						curAnimation.frame = 0;
					}
					else
					{
						flag = true;
						curAnimation.frame = 0;
					}
				}
			}
			exSpriteAnimationState exSpriteAnimationState2 = curAnimation;
			if (num <= curAnimation.frame)
			{
				curAnimation.TriggerEvents(this, num, curAnimation.frame);
				lastFrameIndex = exSpriteAnimationState2.frame;
			}
			if (flag)
			{
				Stop(exSpriteAnimationState2);
			}
		}
		else
		{
			curIndex = -1;
		}
	}

	private void Sample()
	{
		if (curAnimation != null)
		{
			int currentIndex = curAnimation.GetCurrentIndex();
			if (currentIndex >= 0 && currentIndex != curIndex)
			{
				sprite_.textureInfo = curAnimation.clip.frameInfos[currentIndex].textureInfo;
			}
			curIndex = currentIndex;
		}
		else
		{
			curIndex = -1;
		}
	}
}
