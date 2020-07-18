#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using System;
using System.IO;
using OfficeOpenXml;
using System.Text;
using System.Reflection;

namespace Map
{
	/// <summary>
	/// 配置模板
	/// </summary>
	/// <typeparam name="T"></typeparam>
	 [ExecuteInEditMode]
	public class ConfigVO<T> where T : BaseVO
	{
		#region 单例
		private static ConfigVO<T> instance = null;
		public static ConfigVO<T> Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new ConfigVO<T>();
				}
				return instance;
			}
		}
		#endregion

		#region 私有属性
		/// <summary>
		/// 存的是vo数据
		/// </summary>
		private List<T> voList;
		private Dictionary<int, T> voDir;
        /// <summary>
        /// 有的是用string作为键值
        /// </summary>
        private Dictionary<string, T> voStrDir;
		/// <summary>
		/// 真实数据起始行
		/// </summary>
		private const int ACTUALDATA_ROW = 5;
        /// <summary>
        /// 属性对应的行
        /// </summary>
        private const int PROPERTY_ROW = 3;
		#endregion

		#region 私有方法
		/// <summary>
		/// 初始化
		/// </summary>
		private void Init()
		{
			try
			{
				string voName = typeof(T).Name;
				if (EditorGamingMapData.m_VoNameDic != null && !EditorGamingMapData.m_VoNameDic.ContainsKey(voName))
				{
					return;
				}
				List<string> fileNameList = EditorGamingMapData.m_VoNameDic[voName];
				if (fileNameList == null || fileNameList.Count != 2)
				{
					return;
				}
                //ESheetReader reader = EditorConfigData.GetXlsxFileSheetReader(fileNameList[0], fileNameList[1]);
                ESheetReader reader = EditorConfigData.GetCSVFileSheetReader(fileNameList[0], fileNameList[1]);
                if (reader == null)
				{
					return;
				}

				voDir = new Dictionary<int, T>();
				voList = new List<T>();
                voStrDir = new Dictionary<string, T>();
                T tmp;
				for (int i = ACTUALDATA_ROW-1; i < reader.GetRowCount(); i++)
				{
					tmp = Activator.CreateInstance<T>();
					tmp.CopyFrom(i, reader);
					
                    if(!string.IsNullOrEmpty(tmp.strID))
                    {
                        if(!voStrDir.ContainsKey(tmp.strID))
                        {
                            voStrDir.Add(tmp.strID,tmp);
                            voList.Add(tmp);
                        }
                    }
                    else
                    {
                        if (!voDir.ContainsKey(tmp.ID) && tmp.ID>0)
                        {
                            voDir.Add(tmp.ID, tmp);
                            voList.Add(tmp);
                        }
                    }
				}
			}
			catch (Exception e)
			{

			}
		}
		#endregion

		#region 公开方法

        public T GetData(string strId)
        {
            if(voStrDir == null)
            {
                Init();
            }
            if (voStrDir == null || !voStrDir.ContainsKey(strId))
            {
                if (voStrDir == null)
                {
                    Debug.LogError("读表异常");
                }
                return null;
            }
            return voStrDir[strId];
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
		public T GetData(int id)
		{
			if (voDir == null)
			{
				Init();
			}
			if (voDir == null || !voDir.ContainsKey(id))
			{
                if(voDir == null)
                {
                    Debug.LogError("读表异常");
                }
				return null;
			}
			return voDir[id];
		}

        public void AddData(T data,bool useStr = false)
        {
            if(useStr)
            {
                if(!voStrDir.ContainsKey(data.strID))
                {
                    voStrDir.Add(data.strID, data);
                    voList.Add(data);
                }
            }
            else
            {
                if (!voDir.ContainsKey(data.ID))
                {
                    voDir.Add(data.ID, data);
                    voList.Add(data);
                }
            }
            
        }
        
        /// <summary>
        /// 只针对保存 有重复的id
        /// </summary>
        /// <param name="dataList"></param>
        public void SetWithRepeatId(List<T> dataList)
        {
            if (dataList == null)
            {
                return;
            }
            voList = dataList;
            voDir.Clear();
            voStrDir.Clear();
        }

        public void Set(List<T> dataList,bool useStr = false)
        {
            if(dataList == null)
            {
                return;
            }
            voList = dataList;
            if(voDir == null)
            {
                voDir = new Dictionary<int, T>();
            }
            
            if(voStrDir == null)
            {
                voStrDir = new Dictionary<string, T>();
            }
            voDir.Clear();
            voStrDir.Clear();
            for (int iVo = 0;iVo<dataList.Count;iVo++)
            {
                T data = dataList[iVo];
                if(useStr)
                {
                    voStrDir.Add(data.strID,data);
                }
                else
                {
                    voDir.Add(data.ID, data);
                }
                
            }
        }

		public Dictionary<int, T> GetData()
		{
			if (voDir == null)
			{
				Init();
			}
			return voDir;
		}

		public List<T> GetList()
		{
			if (voList == null)
			{
				Init();
			}
			return voList;
		}

        public void ResetData()
        {
            if(voList != null)
            {
                voList.Clear();
            }
            if(voDir != null)
            {
                voDir.Clear();
            }
        }
		/// <summary>
		/// 清除数据
		/// </summary>
		public void Clear()
		{
			voList = null;
			voDir = null;
		}

        /// <summary>
        /// 保存至xlsx
        /// </summary>
        public void SaveXlsx()
        {
            string voName = typeof(T).Name;
            if (EditorGamingMapData.m_VoNameDic != null && !EditorGamingMapData.m_VoNameDic.ContainsKey(voName))
            {
                return;
            }
            List<string> fileNameList = EditorGamingMapData.m_VoNameDic[voName];
            if (fileNameList == null || fileNameList.Count != 2)
            {
                return;
            }

            string path = EditorConfigData.GetExcelFilePath(fileNameList[0]);
            FileInfo fileInfo = new FileInfo(path);
            ExcelPackage ep = new ExcelPackage(fileInfo);
            ExcelWorksheet sheet = ep.Workbook.Worksheets[1];

            for (int iSheet = ACTUALDATA_ROW; iSheet <= sheet.Dimension.End.Row; iSheet++)
            {
                object value = sheet.Cells[iSheet, 1].Value;
                if (value == null)
                {
                    sheet.DeleteRow(iSheet, 1, true);
                    iSheet--;
                    continue;
                }

                int id = 0;
                int.TryParse(sheet.Cells[iSheet, 1].Value.ToString(), out id);
                if (!voDir.ContainsKey(id))
                {
                    sheet.DeleteRow(iSheet, 1, true);
                    iSheet--;
                }
            }

            int rowCount = sheet.Dimension.End.Row;
            int colCount = sheet.Dimension.End.Column;

            for (int iVo = 0; iVo < voList.Count; iVo++)
            {
                T vo = voList[iVo];
                int row = GetValueRow(sheet, vo.ID);
                if (row == -1)
                {
                    rowCount++;
                    sheet.InsertRow(rowCount, colCount);
                    row = rowCount;
                }
                vo.SetSheetData(sheet, row);
            }
            ep.SaveAs(fileInfo);
        }

        /// <summary>
        /// 保存至csv
        /// </summary>
        public void SaveCSV()
        {
            string voName = typeof(T).Name;
            if (EditorGamingMapData.m_VoNameDic != null && !EditorGamingMapData.m_VoNameDic.ContainsKey(voName))
            {
                return;
            }
            List<string> fileNameList = EditorGamingMapData.m_VoNameDic[voName];
            if(fileNameList == null || fileNameList.Count != 2)
            {
                return;
            }

            string path = EditorConfigData.GetExcelFilePath(fileNameList[0]);
            FileInfo fileInfo = new FileInfo(path);
            ExcelTextFormat format = new ExcelTextFormat();
            format.Delimiter = ',';
            format.EOL = "\n";
            format.TextQualifier = '"';
            format.Encoding = new UTF8Encoding();

            string fileData = string.Empty;
            using (StreamReader reader = new StreamReader(path, Encoding.UTF8))
            {
                fileData = reader.ReadToEnd();
            }

            fileData = fileData.Replace("\r", "");

            System.String[][] resultdata = null;//保存title
            using (ExcelPackage ep = new ExcelPackage())
            {
                ExcelWorksheet sheet = ep.Workbook.Worksheets.Add(fileNameList[1]);
                sheet.Cells["A1"].LoadFromText(fileData, format);
                int columnMin = sheet.Dimension.Start.Column;
                int rowMin = sheet.Dimension.Start.Row;
                int columnCount = sheet.Dimension.End.Column; //工作区结束列
                int rowCount = sheet.Dimension.End.Row; //工作区结束行号
                resultdata = new System.String[ACTUALDATA_ROW-1][];

                for (int i = rowMin; i <= ACTUALDATA_ROW - 1; i++)
                {
                    resultdata[i - 1] = new System.String[columnCount];
                    for (int j = columnMin; j <= columnCount; j++)
                    {
                        ExcelRange data = sheet.Cells[i, j];
                        if (data != null && data.Value != null)
                        {
                            resultdata[i - 1][j - 1] = data.Value.ToString();
                        }
                    }
                }

                if (fileInfo.Exists)
                {
                    fileInfo.Delete();
                }

                using (FileStream fs = new FileStream(path,FileMode.Append,FileAccess.Write))
                {
                    using (var stream = new MemoryStream(256))
                    {
                        using (var writer = new StreamWriter(fs, Encoding.UTF8))
                        {
                            if (resultdata != null)
                            {
                                for (int iResult = 0; iResult < resultdata.Length; iResult++)
                                {
                                    for (int jResult = 0; jResult < resultdata[iResult].Length; jResult++)
                                    {
                                        writer.Write(resultdata[iResult][jResult]);
                                        if (jResult != resultdata[iResult].Length - 1)
                                        {
                                            writer.Write(",");
                                        }
                                    }
                                    writer.Write("\n");
                                }
                            }

                            for (int iVo = 0; iVo < voList.Count; iVo++)
                            {
                                T vo = voList[iVo];
                                FieldInfo[] fi = typeof(T).GetFields();
                                WriteObjectToLine<T>(vo, fi, writer);
                                //if (iVo != voList.Count - 1)
                                {
                                    writer.Write("\n");
                                }

                            }
                            writer.Flush();
                        }
                    }
                }
            }
        }

        public static char[] quotedChars = new char[] { ',', ';', '\n' };

        private  void WriteObjectToLine<T1>(T1 obj, FieldInfo[] fi, TextWriter w)
        {
            bool firstCol = true;
            foreach (FieldInfo f in fi)
            {
                if(f.Name.Equals("ID") || f.Name.Equals("strID"))//因为继承父类 读取不会按顺序写入
                {
                    continue;
                }
                if (firstCol)
                    firstCol = false;
                else
                    w.Write(",");

                var value = f.GetValue(obj);
                string val = "";
                if (value != null)
                {
                    val = f.GetValue(obj).ToString();
                    if (val.IndexOfAny(quotedChars) != -1)
                    {
                        val = string.Format("\"{0}\"", val);
                    }
                }
                w.Write(val);
            }
        }

        void AddRow(ExcelWorksheet worksheet, string[][] row_data)
        {
            if(row_data != null && row_data.Length>0)
            {
                for(int iRow = 0;iRow<row_data.Length;iRow++)
                {
                    for(int jRow = 0;jRow< row_data[iRow].Length;jRow++ )
                    {
                        worksheet.Cells[iRow + 1, jRow + 1].Value = row_data[iRow][jRow];
                    }
                }
            }
        }

        public int GetValueColumn(ExcelWorksheet sheet, object value)
        {
            int column = -1;
            int colCount = sheet.Dimension.End.Column;
            for (int i = 1; i <= colCount; i++)
            {
                if (ObjectEqual(sheet.Cells[PROPERTY_ROW, i].Value, value))
                {
                    column = i;
                    break;
                }
            }

            return column;
        }

        public int GetValueRow(ExcelWorksheet sheet,object value)
        {
            int row = -1;
            int rowCount = sheet.Dimension.End.Row;
            for (int iRow = ACTUALDATA_ROW; iRow <= rowCount; iRow++)
            {
                if (ObjectEqual(sheet.Cells[iRow, 1].Value, value))
                {
                    row = iRow;
                    break;
                }
            }

            return row;
        }

        public bool ObjectEqual(object obj1, object obj2)
        {
            if (obj1 == null || obj2 == null)
                return false;
            string value1 = obj1.ToString().Trim();
            string value2 = obj2.ToString().Trim();

            return string.Equals(value1, value2);
        }

        #endregion

    }

}
#endif