using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Mail;
using System.Text;
using AiUnity.Common.Attributes;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;
using AiUnity.NLog.Core.Internal;
using AiUnity.NLog.Core.Layouts;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.Targets;

[Target("Email")]
[Preserve]
public class MailTarget : TargetWithLayoutHeaderAndFooter
{
	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	[RequiredParameter]
	[Display("From", "From email address.", false, 0)]
	public Layout From { get; set; }

	[RequiredParameter]
	[Display("To", "To email address.", false, 0)]
	public Layout To { get; set; }

	public Layout CC { get; set; }

	public Layout Bcc { get; set; }

	public bool AddNewLines { get; set; }

	[DefaultValue("Message from NLog on ${machinename}")]
	[RequiredParameter]
	[Display("Subject", "Subject of email.", false, 0)]
	public Layout Subject { get; set; }

	[DefaultValue("${message}")]
	public Layout Body
	{
		get
		{
			return Layout;
		}
		set
		{
			Layout = value;
		}
	}

	[DefaultValue("UTF8")]
	public Encoding Encoding { get; set; }

	[DefaultValue(false)]
	public bool Html { get; set; }

	[Display("Server", "Smtp Server address", false, 0)]
	public Layout SmtpServer { get; set; }

	[Display("Auth Mode", "Smtp authentication mode.", false, 0)]
	[DefaultValue("Basic")]
	public SmtpAuthenticationMode SmtpAuthentication { get; set; }

	[Display("Smtp username", "Smtp username for authentication", false, 0)]
	public Layout SmtpUserName { get; set; }

	[Display("Smtp password", "Smtp password for authentication", false, 0)]
	public Layout SmtpPassword { get; set; }

	[DefaultValue(false)]
	public bool EnableSsl { get; set; }

	[DefaultValue(25)]
	public int SmtpPort { get; set; }

	[DefaultValue(false)]
	public bool UseSystemNetMailSettings { get; set; }

	public Layout Priority { get; set; }

	[DefaultValue(false)]
	public bool ReplaceNewlineWithBrTagInHtml { get; set; }

	[DefaultValue(10000)]
	public int Timeout { get; set; }

	public MailTarget()
	{
		Body = "${message}${newline}";
		Subject = "Message from NLog on ${machinename}";
		Encoding = Encoding.UTF8;
		SmtpPort = 3325;
		SmtpAuthentication = SmtpAuthenticationMode.Basic;
	}

	internal virtual ISmtpClient CreateSmtpClient()
	{
		return new MySmtpClient();
	}

	protected override void Write(AsyncLogEventInfo logEvent)
	{
		Write(new AsyncLogEventInfo[1] { logEvent });
	}

	protected override void Write(AsyncLogEventInfo[] logEvents)
	{
		foreach (KeyValuePair<string, List<AsyncLogEventInfo>> item in logEvents.BucketSort((AsyncLogEventInfo c) => GetSmtpSettingsKey(c.LogEvent)))
		{
			List<AsyncLogEventInfo> value = item.Value;
			ProcessSingleMailMessage(value);
		}
	}

	private void ProcessSingleMailMessage(List<AsyncLogEventInfo> events)
	{
		try
		{
			LogEventInfo logEvent = events[0].LogEvent;
			LogEventInfo logEvent2 = events[events.Count - 1].LogEvent;
			StringBuilder stringBuilder = new StringBuilder();
			if (Header != null)
			{
				stringBuilder.Append(Header.Render(logEvent));
				if (AddNewLines)
				{
					stringBuilder.Append(Environment.NewLine);
				}
			}
			foreach (AsyncLogEventInfo @event in events)
			{
				stringBuilder.Append(Layout.Render(@event.LogEvent));
				if (AddNewLines)
				{
					stringBuilder.Append(Environment.NewLine);
				}
			}
			if (Footer != null)
			{
				stringBuilder.Append(Footer.Render(logEvent2));
				if (AddNewLines)
				{
					stringBuilder.Append(Environment.NewLine);
				}
			}
			using MailMessage mailMessage = new MailMessage();
			SetupMailMessage(mailMessage, logEvent2);
			mailMessage.Body = stringBuilder.ToString();
			if (mailMessage.IsBodyHtml && ReplaceNewlineWithBrTagInHtml)
			{
				mailMessage.Body = mailMessage.Body.Replace(Environment.NewLine, "<br/>");
			}
			using ISmtpClient smtpClient = CreateSmtpClient();
			if (!UseSystemNetMailSettings)
			{
				ConfigureMailClient(logEvent2, smtpClient);
			}
			Logger.Debug("Sending mail to {0} using {1}:{2} (ssl={3})", mailMessage.To, smtpClient.Host, smtpClient.Port, smtpClient.EnableSsl);
			Logger.Trace("  Subject: '{0}'", mailMessage.Subject);
			Logger.Trace("  From: '{0}'", mailMessage.From.ToString());
			smtpClient.Send(mailMessage);
			foreach (AsyncLogEventInfo event2 in events)
			{
				event2.Continuation(null);
			}
		}
		catch (Exception exception)
		{
			if (exception.MustBeRethrown())
			{
				throw;
			}
			foreach (AsyncLogEventInfo event3 in events)
			{
				event3.Continuation(exception);
			}
		}
	}

	private void ConfigureMailClient(LogEventInfo lastEvent, ISmtpClient client)
	{
		client.Host = SmtpServer.Render(lastEvent);
		client.Port = SmtpPort;
		client.EnableSsl = EnableSsl;
		client.Timeout = Timeout;
		if (SmtpAuthentication == SmtpAuthenticationMode.Ntlm)
		{
			Logger.Trace("  Using NTLM authentication.");
			client.Credentials = CredentialCache.DefaultNetworkCredentials;
		}
		else if (SmtpAuthentication == SmtpAuthenticationMode.Basic)
		{
			string text = SmtpUserName.Render(lastEvent);
			string text2 = SmtpPassword.Render(lastEvent);
			Logger.Trace("  Using basic authentication: Username='{0}' Password='{1}'", text, new string('*', text2.Length));
			client.Credentials = new NetworkCredential(text, text2);
		}
	}

	private string GetSmtpSettingsKey(LogEventInfo logEvent)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(From.Render(logEvent));
		stringBuilder.Append("|");
		stringBuilder.Append(To.Render(logEvent));
		stringBuilder.Append("|");
		if (CC != null)
		{
			stringBuilder.Append(CC.Render(logEvent));
		}
		stringBuilder.Append("|");
		if (Bcc != null)
		{
			stringBuilder.Append(Bcc.Render(logEvent));
		}
		stringBuilder.Append("|");
		if (SmtpServer != null)
		{
			stringBuilder.Append(SmtpServer.Render(logEvent));
		}
		if (SmtpPassword != null)
		{
			stringBuilder.Append(SmtpPassword.Render(logEvent));
		}
		stringBuilder.Append("|");
		if (SmtpUserName != null)
		{
			stringBuilder.Append(SmtpUserName.Render(logEvent));
		}
		return stringBuilder.ToString();
	}

	private void SetupMailMessage(MailMessage msg, LogEventInfo logEvent)
	{
		msg.From = new MailAddress(From.Render(logEvent));
		string[] array = To.Render(logEvent).Split(';');
		foreach (string addresses in array)
		{
			msg.To.Add(addresses);
		}
		if (Bcc != null)
		{
			array = Bcc.Render(logEvent).Split(';');
			foreach (string addresses2 in array)
			{
				msg.Bcc.Add(addresses2);
			}
		}
		if (CC != null)
		{
			array = CC.Render(logEvent).Split(';');
			foreach (string addresses3 in array)
			{
				msg.CC.Add(addresses3);
			}
		}
		msg.Subject = Subject.Render(logEvent).Trim();
		msg.BodyEncoding = Encoding;
		msg.IsBodyHtml = Html;
		if (Priority != null)
		{
			string value = Priority.Render(logEvent);
			try
			{
				msg.Priority = (MailPriority)Enum.Parse(typeof(MailPriority), value, ignoreCase: true);
			}
			catch
			{
				Logger.Warn("Could not convert '{0}' to MailPriority, valid values are Low, Normal and High. Using normal priority as fallback.");
				msg.Priority = MailPriority.Normal;
			}
		}
	}
}
