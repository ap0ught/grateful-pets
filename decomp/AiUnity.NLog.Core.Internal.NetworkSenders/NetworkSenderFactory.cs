using System;
using System.Net.Sockets;

namespace AiUnity.NLog.Core.Internal.NetworkSenders;

internal class NetworkSenderFactory : INetworkSenderFactory
{
	public static readonly INetworkSenderFactory Default = new NetworkSenderFactory();

	public NetworkSender Create(string url, int maxQueueSize)
	{
		if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
		{
			return new HttpNetworkSender(url);
		}
		if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
		{
			return new HttpNetworkSender(url);
		}
		if (url.StartsWith("tcp://", StringComparison.OrdinalIgnoreCase))
		{
			return new TcpNetworkSender(url, AddressFamily.Unspecified)
			{
				MaxQueueSize = maxQueueSize
			};
		}
		if (url.StartsWith("tcp4://", StringComparison.OrdinalIgnoreCase))
		{
			return new TcpNetworkSender(url, AddressFamily.InterNetwork)
			{
				MaxQueueSize = maxQueueSize
			};
		}
		if (url.StartsWith("tcp6://", StringComparison.OrdinalIgnoreCase))
		{
			return new TcpNetworkSender(url, AddressFamily.InterNetworkV6)
			{
				MaxQueueSize = maxQueueSize
			};
		}
		if (url.StartsWith("udp://", StringComparison.OrdinalIgnoreCase))
		{
			return new UdpNetworkSender(url, AddressFamily.Unspecified);
		}
		if (url.StartsWith("udp4://", StringComparison.OrdinalIgnoreCase))
		{
			return new UdpNetworkSender(url, AddressFamily.InterNetwork);
		}
		if (url.StartsWith("udp6://", StringComparison.OrdinalIgnoreCase))
		{
			return new UdpNetworkSender(url, AddressFamily.InterNetworkV6);
		}
		throw new ArgumentException("Unrecognized network address", "url");
	}
}
