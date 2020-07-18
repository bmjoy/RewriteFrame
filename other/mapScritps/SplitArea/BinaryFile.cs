using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Map
{
    /// <summary>
    /// 二进制文件打开类型
    /// </summary>
    public enum OPEN_MODE
    {
        OPEN_READ,//读取
        OPEN_WRITE,//写入
        OPEN_WRITE_CREATE,//创建并写入
    }

    /// <summary>
    /// 二进制文件读写器
    /// </summary>
    public class BinaryFile
    {
        /// <summary>
        /// 读取
        /// </summary>
        public BinaryReader m_Reader;
        /// <summary>
        /// 写入
        /// </summary>
        public BinaryWriter m_Writer;
        /// <summary>
        /// 文件格式
        /// </summary>
        public Encoding m_CurEncoding;
        /// <summary>
        /// 是否已打开
        /// </summary>
        private bool m_Opened = false;
        /// <summary>
        /// 流
        /// </summary>
        private Stream m_Stream = null;

        public BinaryFile(System.Text.Encoding encode)
        {
            m_CurEncoding = encode;
        }

        /// <summary>
        /// 打开字节流
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public bool OpenMem(byte[] buf, OPEN_MODE mode)
        {
            if (m_Opened)
                Close();

            if (null == buf)
            {
                return false;
            }

            m_Stream = new MemoryStream(buf, mode == OPEN_MODE.OPEN_READ ? false : true);
            if (m_Stream == null)
                return false;

            m_Opened = true;

            if (mode == OPEN_MODE.OPEN_READ)
            {
                m_Reader = new BinaryReader(m_Stream, m_CurEncoding);
            }
            else
            {
                m_Writer = new BinaryWriter(m_Stream, m_CurEncoding);
            }

            return true;
        }

        /// <summary>
        /// 读取字节流
        /// </summary>
        /// <param name="mem"></param>
        /// <returns></returns>
        public bool OpenRead(byte[] mem)
        {
            return OpenMem(mem, OPEN_MODE.OPEN_READ);
        }

        /// <summary>
        /// 写入字节流
        /// </summary>
        /// <param name="mem"></param>
        /// <returns></returns>
        public bool OpenWriter(byte[] mem)
        {
            return OpenMem(mem, OPEN_MODE.OPEN_WRITE);
        }

        /// <summary>
        /// 写入文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public bool OpenWrite(string path, OPEN_MODE mode = OPEN_MODE.OPEN_WRITE)
        {
            return Open(path, mode);
        }

        /// <summary>
        /// 打开
        /// </summary>
        /// <param name="path"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public  bool Open(string path, OPEN_MODE mode)
        {
            if (m_Opened)
                Close();

            FileMode filemode = FileMode.Open;
            if (mode == OPEN_MODE.OPEN_WRITE)
            {
                filemode = FileMode.OpenOrCreate;
            }
            else if (mode == OPEN_MODE.OPEN_WRITE_CREATE)
            {
                filemode = FileMode.Create;
            }

            FileAccess access = (mode == OPEN_MODE.OPEN_READ) ? FileAccess.Read : FileAccess.ReadWrite;

            if (access == FileAccess.Read)
            {
                if (!File.Exists(path))
                {
                    return false;
                }
            }
            else
            {
                if (File.Exists(path))
                {
                    File.SetAttributes(path, FileAttributes.Normal);
                }
            }

            try
            {
                m_Stream = new FileStream(path, filemode, access, FileShare.ReadWrite);
                if (m_Stream == null)
                    return false;

                m_Opened = true;
            }
            catch (Exception ex)
            {
                Debug.LogError("异常:"+ex.ToString());
            }

            if (mode == OPEN_MODE.OPEN_READ)
            {
                m_Reader = new BinaryReader(m_Stream, m_CurEncoding);
            }
            else
            {
                m_Writer = new BinaryWriter(m_Stream, m_CurEncoding);
            }

            return true;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            if (m_Writer != null)
            {
                m_Writer.Close();
            }

            if (m_Reader != null)
            {
                m_Reader.Close();
            }

            if (!m_Opened)
                return;

            if (m_Stream != null)
            {
                m_Stream.Close();
                m_Stream = null;
            }

            m_Opened = false;
        }
    }
}

