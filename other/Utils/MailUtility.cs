#if UNITY_EDITOR
#define SUPPORT_MAIL
#endif

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Leyoutech.Utility
{
	public static class MailUtility
	{
		public const string GROUP_DEBUG = "Debug";
		public const string GROUP_PUBLIC = "Public";

		private const string LOG_TAG = "Mail";
		private const string SUBJECT_STARTWITHS = "[AUTO][Eternity]";

		private static SmtpClient ms_SmtpClient;
		private static readonly string ms_FromAddress;
		private static Queue<MailMessage> ms_WaitSendMails;
		private static bool ms_IsSending;
		private static object ms_Lock;
		private static Dictionary<string, List<string>> ms_ToAddressGroups;

		static MailUtility()
		{
			ServicePointManager.ServerCertificateValidationCallback = OnServerCertificateValidationCallback;

			ms_FromAddress = SettingINI.Setting.GetValue(SettingINI.Setting.CombineKey(SettingINI.Constants.GROUP_MAIL, SettingINI.Constants.KEY_DEFAULT_FROM_ADDRESS), "debugger@huangwm.com");
			string[] smtpClientSettings = SettingINI.Setting.GetValue(SettingINI.Setting.CombineKey(SettingINI.Constants.GROUP_MAIL, SettingINI.Constants.KEY_DEFAULT_SMTP_CLIENT), "smtp.mxhichina.com:debugger@huangwm.com:Debug199396").Split(':');

			ms_SmtpClient = new SmtpClient(smtpClientSettings[0]);
			ms_SmtpClient.Credentials = new NetworkCredential(smtpClientSettings[1], smtpClientSettings[2]) as ICredentialsByHost;
			ms_SmtpClient.EnableSsl = true;
			ms_SmtpClient.SendCompleted += OnSendCompleted;

			ms_WaitSendMails = new Queue<MailMessage>();

			ms_Lock = new object();

			ms_IsSending = false;

			ms_ToAddressGroups = new Dictionary<string, List<string>>();
			if (SettingINI.Setting.TryGetValue(SettingINI.Setting.CombineKey(SettingINI.Constants.GROUP_MAIL
				, SettingINI.Constants.KEY_TO_ADDRESS), out string toAddresssGroupsString))
			{
				string[] toAddresssGroupStrings = toAddresssGroupsString.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
				for (int iAddressGroup = 0; iAddressGroup < toAddresssGroupStrings.Length; iAddressGroup++)
				{
					string toAddresssGroupString = toAddresssGroupStrings[iAddressGroup];
					string[] toAddressssString = toAddresssGroupString.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
					if (toAddressssString.Length != 2)
					{
						continue;
					}

					string groupName = toAddressssString[0];
					if (!ms_ToAddressGroups.TryGetValue(groupName, out List<string> toAddressGroup))
					{
						toAddressGroup = new List<string>();
						ms_ToAddressGroups.Add(groupName, toAddressGroup);
					}

					toAddressGroup.AddRange(toAddressssString[1].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList());
				}
			}
		}

		public static void SendAsync(string groupName, string subject, string body, bool isBodyHtml, List<Attachment> attachments = null)
		{
			if (ms_ToAddressGroups.TryGetValue(groupName, out List<string> toAddressGroup))
			{
				SendAsync(toAddressGroup, subject, body, isBodyHtml, attachments);
			}
			else
			{
				DebugUtility.LogWarning(LOG_TAG, $"Not found mail to address group({groupName})");
			}
		}

		public static void SendAsync(List<string> toAddresss, string subject, string body, bool isBodyHtml, List<Attachment> attachments = null)
		{
			MailMessage mail = new MailMessage();

			mail.From = new MailAddress(ms_FromAddress);
			for (int iAddress = 0; iAddress < toAddresss.Count; iAddress++)
			{
				mail.To.Add(toAddresss[iAddress]);
			}
			mail.Subject = SUBJECT_STARTWITHS + subject;
			mail.Body = body;
			mail.IsBodyHtml = isBodyHtml;
			if (attachments != null)
			{
				for (int iAttachment = 0; iAttachment < attachments.Count; iAttachment++)
				{
					mail.Attachments.Add(attachments[iAttachment]);
				}
			}

			DebugUtility.Log(LOG_TAG, $"Add mail({mail.Subject}) to queue");
			lock (ms_Lock)
			{
				ms_WaitSendMails.Enqueue(mail);
			}

			// Make sure update
			EditorExtend.UpdateAnyway.GetInstance().UpdateAction -= OnUpdate;
			EditorExtend.UpdateAnyway.GetInstance().UpdateAction += OnUpdate;
		}

		public static bool IsSending()
		{
			return ms_IsSending
				|| ms_WaitSendMails.Count > 0;
		}

		private static void DoSendAsync(MailMessage mail)
		{
#if SUPPORT_MAIL
			try
			{
				lock (ms_Lock)
				{
					ms_IsSending = true;
				}
				DebugUtility.Log(LOG_TAG, $"Begin send async mail({mail.Subject})");
				ms_SmtpClient.SendAsync(mail, mail.Subject);
			}
			catch (Exception e)
			{
				lock (ms_Lock)
				{
					ms_IsSending = false;
				}
				DebugUtility.LogError(LOG_TAG, $"Send mail({mail.Subject}) Exception:\n{e.ToString()}");
			}
#else
			DebugUtility.LogWarning(LOG_TAG, "Not support mail");
#endif
		}

		private static void OnUpdate(float time, float delta)
		{
			MailMessage mail;
			lock (ms_Lock)
			{
				if (ms_IsSending
					|| ms_WaitSendMails.Count == 0)
				{
					return;
				}
				else
				{
					mail = ms_WaitSendMails.Dequeue();
				}
			}

			DoSendAsync(mail);
		}

		private static void OnSendCompleted(object sender, AsyncCompletedEventArgs e)
		{
			ms_IsSending = false;

			string userToken = e.UserState as string;
			if (e.Cancelled)
			{
				DebugUtility.LogWarning(LOG_TAG, $"Send mail({userToken}) cancelled");
			}
			else if (e.Error != null)
			{
				DebugUtility.LogError(LOG_TAG, $"Send mail({userToken}) Exception:\n{e.Error.ToString()}");
			}
			else
			{
				DebugUtility.Log(LOG_TAG, $"Sent mail({userToken}) success");
			}
		}

		private static bool OnServerCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}

		[EditorExtend.CTMenuItem("Test/Send Mail")]
		private static void TestMail()
		{
			SendAsync(GROUP_DEBUG, "TestMail A", "1", true);
			SendAsync(GROUP_DEBUG, "TestMail B", "2", true);
			SendAsync(GROUP_DEBUG, "TestMail C", "3", true);
			SendAsync(GROUP_DEBUG, "TestMail D", "4", true);
			SendAsync(GROUP_DEBUG, "TestMail E", "5", true);
		}
	}
}