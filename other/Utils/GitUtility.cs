#if UNITY_STANDALONE_WIN || UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leyoutech.Utility
{
	public static class GitUtility
	{
		/// <summary>
		/// 获取版本库当前的提交Id
		/// </summary>
		/// <param name="repositoryDirectory">版本库目录</param>
		/// <param name="commitId">提交ID</param>
		public static bool TryGetCurrentCommitId(string repositoryDirectory, out string commitId)
		{
			System.Diagnostics.ProcessStartInfo processInfo = new System.Diagnostics.ProcessStartInfo("cmd.exe"
				, $"/c git rev-parse HEAD");

			processInfo.CreateNoWindow = true;
			processInfo.UseShellExecute = false;
			processInfo.RedirectStandardError = true;
			processInfo.RedirectStandardOutput = true;
			processInfo.WorkingDirectory = repositoryDirectory;
			processInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

			if (ExecuteProcessUtility.ExecuteProcess(processInfo, out int exitCode, out string output, out string error))
			{
				commitId = output.Trim();
				return true;
			}
			else
			{
				commitId = string.Empty;
				return false;
			}
		}

		/// <summary>
		/// 获取某次提交的Log
		/// </summary>
		/// <param name="repositoryDirectory">版本库目录</param>
		/// <param name="commitId">提交Id</param>
		/// <param name="format"><see cref="https://git-scm.com/book/en/v2/Git-Basics-Viewing-the-Commit-History"/></param>
		public static bool TryGetCommitLogWithCommitId(string repositoryDirectory, string commitId, string format, out string gitLog)
		{
			System.Diagnostics.ProcessStartInfo processInfo = new System.Diagnostics.ProcessStartInfo("cmd.exe"
				, $"/c git log --pretty=format:\"{format}\" {commitId} -1");

			processInfo.CreateNoWindow = true;
			processInfo.UseShellExecute = false;
			processInfo.RedirectStandardError = true;
			processInfo.RedirectStandardOutput = true;
			processInfo.WorkingDirectory = repositoryDirectory;
			processInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

			if (ExecuteProcessUtility.ExecuteProcess(processInfo, out int exitCode, out string output, out string error))
			{
				gitLog = output.Trim();
				return true;
			}
			else
			{
				gitLog = string.Empty;
				return false;
			}
		}

		/// <summary>
		/// 尝试远程分支的名字
		/// </summary>
		public static bool TryGetRemoteBranch(string repositoryDirectory, out string branch)
		{
			System.Diagnostics.ProcessStartInfo processInfo = new System.Diagnostics.ProcessStartInfo("cmd.exe"
				, "/c git rev-parse --abbrev-ref --symbolic-full-name @{u}");

			processInfo.CreateNoWindow = true;
			processInfo.UseShellExecute = false;
			processInfo.RedirectStandardError = true;
			processInfo.RedirectStandardOutput = true;
			processInfo.WorkingDirectory = repositoryDirectory;
			processInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

			if (ExecuteProcessUtility.ExecuteProcess(processInfo, out int exitCode, out string output, out string error))
			{
				branch = output.Trim();
				return true;
			}
			else
			{
				branch = string.Empty;
				return false;
			}
		}
	}
}
#endif