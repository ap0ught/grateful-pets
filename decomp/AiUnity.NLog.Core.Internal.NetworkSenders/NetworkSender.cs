using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using AiUnity.NLog.Core.Common;

namespace AiUnity.NLog.Core.Internal.NetworkSenders;

internal abstract class NetworkSender : IDisposable
{
	private static int currentSendTime;

	public string Address { get; private set; }

	public int LastSendTime { get; private set; }

	protected NetworkSender(string url)
	{
		Address = url;
		LastSendTime = Interlocked.Increment(ref currentSendTime);
	}

	~NetworkSender()
	{
		Dispose(disposing: false);
	}

	public void Initialize()
	{
		DoInitialize();
	}

	public void Close(AsyncContinuation continuation)
	{
		DoClose(continuation);
	}

	public void FlushAsync(AsyncContinuation continuation)
	{
		DoFlush(continuation);
	}

	public void Send(byte[] bytes, int offset, int length, AsyncContinuation asyncContinuation)
	{
		LastSendTime = Interlocked.Increment(ref currentSendTime);
		DoSend(bytes, offset, length, asyncContinuation);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void DoInitialize()
	{
	}

	protected virtual void DoClose(AsyncContinuation continuation)
	{
		continuation(null);
	}

	protected virtual void DoFlush(AsyncContinuation continuation)
	{
		continuation(null);
	}

	protected abstract void DoSend(byte[] bytes, int offset, int length, AsyncContinuation asyncContinuation);

	protected virtual EndPoint ParseEndpointAddress(Uri uri, AddressFamily addressFamily)
	{
		UriHostNameType hostNameType = uri.HostNameType;
		if ((uint)(hostNameType - 3) <= 1u)
		{
			return new IPEndPoint(IPAddress.Parse(uri.Host), uri.Port);
		}
		IPAddress[] addressList = Dns.GetHostEntry(uri.Host).AddressList;
		foreach (IPAddress iPAddress in addressList)
		{
			if (iPAddress.AddressFamily == addressFamily || addressFamily == AddressFamily.Unspecified)
			{
				return new IPEndPoint(iPAddress, uri.Port);
			}
		}
		throw new IOException("Cannot resolve '" + uri.Host + "' to an address in '" + addressFamily.ToString() + "'");
	}

	public virtual void CheckSocket()
	{
	}

	private void Dispose(bool disposing)
	{
		if (disposing)
		{
			Close((Exception ex) =>
			{
			});
		}
	}
}
