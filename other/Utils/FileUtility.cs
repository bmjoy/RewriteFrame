using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Leyoutech.Utility
{
	public static class FileUtility
	{
		private const string LOG_TAG = "FileU";

#if UNITY_EDITOR
		/// <summary>
		/// 把相对于项目的路径转换为绝对路径
		/// </summary>
		public static string ConvertProjectPathToAbsolutePath(string projectPath)
		{
			string dataPath = Application.dataPath;
			return dataPath.Substring(0, dataPath.Length - "Assets".Length) + projectPath;
		}
#endif

		/// <summary>
		/// 检查一个目录是否存在，如果不存在则创建这个目录
		/// </summary>
		/// <returns>
		/// 目录是否存在(Create后)
		/// </returns>
		public static bool ExistsDirectoryOrCreate(string directory)
		{
			try
			{
				if (!Directory.Exists(directory))
				{
					Directory.CreateDirectory(directory);
				}
				return Directory.Exists(directory);
			}
			catch (System.Exception e)
			{
				DebugUtility.LogError(LOG_TAG, string.Format("ExistsDirectoryAndCreate({0}) Exception:\n{1}", directory, e.ToString()));
				return false;
			}
		}

		/// <summary>
		/// 返回filePath指定的文件夹中所有子文件夹的名字列表
		/// </summary>
		/// <param name="filePath">绝对路径</param>
		/// <param name="recursive">是否递归</param>
		/// <returns></returns>
		public static List<string> FindAllDirectories(string diretoryPath, bool recursive)
		{
			List<string> dirList = new List<string>(Directory.EnumerateDirectories(diretoryPath));
			if (recursive)
			{
				List<string> subDirList = new List<string>();
				foreach (string dir in dirList)
				{
					subDirList.AddRange(FindAllDirectories(dir, true));
				}

				dirList.AddRange(subDirList);
			}

			return dirList;
		}

		/// <summary>
		/// 返回filePath指定的文件夹中所有文件的名字列表
		/// </summary>
		/// <param name="filePath">绝对路径</param>
		/// <param name="recursive">是否递归</param>
		/// <returns></returns>
		public static List<string> FindAllFiles(string diretoryPath, bool recursive)
		{
			List<string> fileList = new List<string>(Directory.EnumerateFiles(diretoryPath));
			if (recursive)
			{
				List<string> dirList = FindAllDirectories(diretoryPath, true);
				foreach (string dir in dirList)
				{
					fileList.AddRange(FindAllFiles(dir, false));
				}
			}

			return fileList;
		}

		/// <summary>
		/// 获取文件扩展名
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static string GetExtension(string filePath)
		{
			int extensionIndex = filePath.LastIndexOf('.');
			if (extensionIndex == -1)
				return "";

			extensionIndex += 1;

			return filePath.Substring(extensionIndex, filePath.Length - extensionIndex);
		}

		/// <summary>
		/// 从文件路径中获取文件名 (不包含扩展名)
		/// 比如 d:\123\abc.txt
		/// 返回 abc
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static string ExtractFileNameFromAbsolutePath(string filePath)
		{
			// 句号Index
			int pointIndex = filePath.LastIndexOf('.');
			if (pointIndex == -1)
				return "";

			// 斜线Index
			int backlashIndex = filePath.LastIndexOf('\\');
			int slashIndex = filePath.LastIndexOf('/');
			int lastSlashIndex = slashIndex > backlashIndex ? slashIndex : backlashIndex;
			lastSlashIndex += 1;

			string fileName = filePath.Substring(lastSlashIndex, pointIndex - lastSlashIndex);
			return fileName;
		}

		/// <summary>
		/// 绝对路径转化为项目的相对路径
		/// 输入: E:\\eternity_project\\client\\Assets\\SharedArtworks\\Spacecraft\\Ships_Player\\Prefabs
		/// 输出: Assets\\SharedArtworks\\Spacecraft\\Ships_Player\\Prefabs
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static string AbsolutePathToRelativeDataPath(string filePath)
		{
			int assetsIndex = filePath.LastIndexOf("Assets");
			if (assetsIndex == -1)
				return "";

			return filePath.Substring(assetsIndex, filePath.Length - assetsIndex);
		}

		/// <summary>
		/// 删除文件
		/// </summary>
		/// <param name="filePath"></param>
		public static void DeleteFile(string filePath)
		{
			try
			{
				if (File.Exists(filePath))
				{
					File.Delete(filePath);
				}
			}
			catch (System.Exception e)
			{
				DebugUtility.LogError(LOG_TAG, string.Format("DeleteFile ({0}) Exception:\n{1}", filePath, e.ToString()));
			}
		}

		/// <summary>
		/// 拷贝目录
		/// </summary>
		public static void CloneDirectory(string sourcePath, string destPath)
		{
			if (!Directory.Exists(destPath))
			{
				Directory.CreateDirectory(destPath);
			}

			foreach (string directory in Directory.GetDirectories(sourcePath))
			{
				string directoryName = Path.GetFileName(directory);
				string iterChildPath = Path.Combine(destPath, directoryName);
				CloneDirectory(directory, iterChildPath);
			}

			foreach (string file in Directory.GetFiles(sourcePath))
			{
				File.Copy(file, Path.Combine(destPath, Path.GetFileName(file)));
			}
		}

		/// <summary>
		/// 写入文件 需要处理异常
		/// </summary>
		/// <param name="fileFullName">文件完整路径</param>
		/// <param name="graph">需要保存的数据</param>
		public static void WriteToBinaryFile(string fileFullName, object graph)
		{
			FileStream fs = null;
			try
			{
				if (File.Exists(fileFullName))
				{
					File.Delete(fileFullName);
				}
				fs = new FileStream(fileFullName, FileMode.OpenOrCreate, FileAccess.Write);
				BinaryFormatter bf = new BinaryFormatter();
				bf.Serialize(fs, graph);
			}
			catch (Exception ex)
			{
				throw (ex);
			}
			finally
			{
				if (fs != null)
				{
					fs.Close();
				}
			}
		}

		/// <summary>
		/// 读取文件 需要处理异常
		/// </summary>
		/// <param name="fileFullName">文件完整路径</param>
		/// <returns>读取到的数据</returns>
		public static object ReadFromBinaryFile(string fileFullName)
		{
			FileStream fs = null;
			try
			{
				fs = new FileStream(fileFullName, FileMode.OpenOrCreate);
				BinaryFormatter bf = new BinaryFormatter();
				return bf.Deserialize(fs);
			}
			catch (Exception ex)
			{
				throw (ex);
			}
			finally
			{
				if (fs != null)
				{
					fs.Close();
				}
			}
		}

		/// <summary>
		/// 计算文件md5
		/// </summary>
		public static bool TryCalculateFileMD5(string file, out string md5)
		{
			try
			{
				using (var md5Cryptography = System.Security.Cryptography.MD5.Create())
				{
					using (var stream = File.OpenRead(file))
					{
						System.Text.StringBuilder sb = new System.Text.StringBuilder();
						byte[] hash = md5Cryptography.ComputeHash(stream);
						for (int iByte = 0; iByte < hash.Length; iByte++)
						{
							sb.Append(hash[iByte].ToString("X2"));
						}
						md5 = sb.ToString();
						return true;
					}
				}
			}
			catch (Exception e)
			{
				DebugUtility.LogError(LOG_TAG, $"Calculate file({file}) md5 exception:\n{e.ToString()}");
				md5 = string.Empty;
				return false;
			}
		}
	}
}