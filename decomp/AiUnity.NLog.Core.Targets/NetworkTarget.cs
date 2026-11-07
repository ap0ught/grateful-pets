using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using AiUnity.Common.Attributes;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;
using AiUnity.NLog.Core.Internal.NetworkSenders;
using AiUnity.NLog.Core.Layouts;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.Targets;

[Preserve]
public abstract class NetworkTarget : TargetWithLayout
{
	private Dictionary<string, NetworkSender> currentSenderCache = new Dictionary<string, NetworkSender>();

	private List<NetworkSender> openNetworkSenders = new List<NetworkSender>();

	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	[Display("Address", "Network address", false, 0)]
	[DefaultValue("udp://127.0.0.1:9999")]
	public Layout Address { get; set; }

	[DefaultValue(true)]
	public bool KeepConnection { get; set; }

	[DefaultValue(false)]
	public bool NewLine { get; set; }

	[DefaultValue(65000)]
	public int MaxMessageSize { get; set; }

	[DefaultValue(5)]
	public int ConnectionCacheSize { get; set; }

	[DefaultValue(0)]
	public int MaxQueueSize { get; set; }

	public NetworkTargetOverflowAction OnOverflow { get; set; }

	[DefaultValue("utf-8")]
	public Encoding Encoding { get; set; }

	internal INetworkSenderFactory SenderFactory { get; set; }

	public NetworkTarget()
	{
		SenderFactory = NetworkSenderFactory.Default;
		Encoding = Encoding.UTF8;
		OnOverflow = NetworkTargetOverflowAction.Split;
		KeepConnection = true;
		MaxMessageSize = 65000;
		ConnectionCacheSize = 5;
		Address = "udp://127.0.0.1:9999";
	}

	protected override void FlushAsync(AsyncContinuation asyncContinuation)
	{
		int remainingCount = 0;
		AsyncContinuation continuation = (Exception ex) =>
		{
			if (Interlocked.Decrement(ref remainingCount) == 0)
			{
				asyncContinuation(null);
			}
		};
		lock (openNetworkSenders)
		{
			remainingCount = openNetworkSenders.Count;
			if (remainingCount == 0)
			{
				asyncContinuation(null);
				return;
			}
			foreach (NetworkSender openNetworkSender in openNetworkSenders)
			{
				openNetworkSender.FlushAsync(continuation);
			}
		}
	}

	protected override void CloseTarget()
	{
		base.CloseTarget();
		lock (openNetworkSenders)
		{
			foreach (NetworkSender openNetworkSender in openNetworkSenders)
			{
				openNetworkSender.Close((Exception ex) =>
				{
				});
			}
			openNetworkSenders.Clear();
		}
	}

	protected override void Write(AsyncLogEventInfo logEvent)
	{
		string text = Address.Render(logEvent.LogEvent);
		byte[] bytesToWrite = GetBytesToWrite(logEvent.LogEvent);
		if (KeepConnection)
		{
			NetworkSender sender = GetCachedNetworkSender(text);
			ChunkedSend(sender, bytesToWrite, (Exception ex) =>
			{
				if (ex != null)
				{
					Logger.Error("Error when sending {0}", ex);
					ReleaseCachedConnection(sender);
				}
				logEvent.Continuation(ex);
			});
			return;
		}
		NetworkSender sender2 = SenderFactory.Create(text, MaxQueueSize);
		sender2.Initialize();
		lock (openNetworkSenders)
		{
			openNetworkSenders.Add(sender2);
			ChunkedSend(sender2, bytesToWrite, (Exception ex) =>
			{
				lock (openNetworkSenders)
				{
					openNetworkSenders.Remove(sender2);
				}
				if (ex != null)
				{
					Logger.Error("Error when sending {0}", ex);
				}
				sender2.Close((Exception ex2) =>
				{
				});
				logEvent.Continuation(ex);
			});
		}
	}

	protected virtual byte[] GetBytesToWrite(LogEventInfo logEvent)
	{
		string s = ((!NewLine) ? Layout.Render(logEvent) : (Layout.Render(logEvent) + Environment.NewLine));
		return Encoding.GetBytes(s);
	}

	private NetworkSender GetCachedNetworkSender(string address)
	{
		lock (currentSenderCache)
		{
			if (currentSenderCache.TryGetValue(address, out var value))
			{
				value.CheckSocket();
				return value;
			}
			if (currentSenderCache.Count >= ConnectionCacheSize)
			{
				int num = int.MaxValue;
				NetworkSender networkSender = null;
				foreach (KeyValuePair<string, NetworkSender> item in currentSenderCache)
				{
					if (item.Value.LastSendTime < num)
					{
						num = item.Value.LastSendTime;
						networkSender = item.Value;
					}
				}
				if (networkSender != null)
				{
					ReleaseCachedConnection(networkSender);
				}
			}
			value = SenderFactory.Create(address, MaxQueueSize);
			value.Initialize();
			lock (openNetworkSenders)
			{
				openNetworkSenders.Add(value);
			}
			currentSenderCache.Add(address, value);
			return value;
		}
	}

	private void ReleaseCachedConnection(NetworkSender sender)
	{
		lock (currentSenderCache)
		{
			lock (openNetworkSenders)
			{
				if (openNetworkSenders.Remove(sender))
				{
					sender.Close((Exception ex) =>
					{
					});
				}
			}
			if (currentSenderCache.TryGetValue(sender.Address, out var value) && sender == value)
			{
				currentSenderCache.Remove(sender.Address);
			}
		}
	}

	private void ChunkedSend(NetworkSender sender, byte[] buffer, AsyncContinuation continuation)
	{
		int tosend = buffer.Length;
		int pos = 0;
		AsyncContinuation sendNextChunk = null;
		sendNextChunk = (Exception ex) =>
		{
			if (ex != null)
			{
				continuation(ex);
			}
			else if (tosend <= 0)
			{
				continuation(null);
			}
			else
			{
				int num = tosend;
				if (num > MaxMessageSize)
				{
					if (OnOverflow == NetworkTargetOverflowAction.Discard)
					{
						continuation(null);
						return;
					}
					if (OnOverflow == NetworkTargetOverflowAction.Error)
					{
						continuation(new OverflowException("Attempted to send a message larger than MaxMessageSize (" + MaxMessageSize + "). Actual size was: " + buffer.Length + ". Adjust OnOverflow and MaxMessageSize parameters accordingly."));
						return;
					}
					num = MaxMessageSize;
				}
				int offset = pos;
				tosend -= num;
				pos += num;
				sender.Send(buffer, offset, num, sendNextChunk);
			}
		};
		sendNextChunk(null);
	}
}
