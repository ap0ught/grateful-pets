using System;
using System.Net.Mail;

namespace AiUnity.NLog.Core.Internal;

internal class MySmtpClient : SmtpClient, ISmtpClient, IDisposable
{
	public new void Dispose()
	{
	}

	void ISmtpClient.Send(MailMessage msg)
	{
		Send(msg);
	}
}
