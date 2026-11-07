using System;
using System.Net;
using System.Net.Mail;

namespace AiUnity.NLog.Core.Internal;

internal interface ISmtpClient : IDisposable
{
	string Host { get; set; }

	int Port { get; set; }

	int Timeout { get; set; }

	ICredentialsByHost Credentials { get; set; }

	bool EnableSsl { get; set; }

	void Send(MailMessage msg);
}
