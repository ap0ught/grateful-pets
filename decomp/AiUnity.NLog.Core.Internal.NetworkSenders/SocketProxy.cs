using System;
using System.Net.Sockets;

namespace AiUnity.NLog.Core.Internal.NetworkSenders;

internal sealed class SocketProxy : ISocket, IDisposable
{
	private readonly Socket socket;

	public Socket UnderlyingSocket => socket;

	internal SocketProxy(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
	{
		socket = new Socket(addressFamily, socketType, protocolType);
	}

	public void Close()
	{
		socket.Close();
	}

	public bool ConnectAsync(SocketAsyncEventArgs args)
	{
		return socket.ConnectAsync(args);
	}

	public bool SendAsync(SocketAsyncEventArgs args)
	{
		return socket.SendAsync(args);
	}

	public bool SendToAsync(SocketAsyncEventArgs args)
	{
		return socket.SendToAsync(args);
	}

	public void Dispose()
	{
		((IDisposable)socket).Dispose();
	}
}
