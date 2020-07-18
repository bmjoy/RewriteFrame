#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Xml.Linq;
using OfficeOpenXml;
using System.Text;

namespace Map
{
	/// <summary>
	/// 配置表基类
	/// </summary>
	[ExecuteInEditMode]
	public class BaseVO
	{
		#region 私有属性
		private ESheetReader m_CurReader;

		private int m_CurRow;
        #endregion

        #region 公开属性
        public int ID;
        public string strID;

        public static StringBuilder sm_sb = new StringBuilder();
		#endregion

		#region 公开方法

        /// <summary>
        /// 设置当前行和reader
        /// </summary>
        /// <param name="row"></param>
        /// <param name="reader"></param>
		public virtual void CopyFrom(int row, ESheetReader reader)
		{
			m_CurRow = row;
			m_CurReader = reader;
		}

		public BaseVO()
		{

		}
        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="row"></param>
        public virtual void SetSheetData(ExcelWorksheet sheet,int row)
        {
            sheet.Cells[row, 1].Value = ID;
        }
		#endregion

		#region 基类方法
        /// <summary>
        /// 类型转换int
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
		protected int GetInt(string str)
		{
			int value = -1;
			if (m_CurReader == null)
			{
				return value;
			}
			int.TryParse(m_CurReader.GetDataByRowAndName(m_CurRow, str), out value);
			return value;
		}
        /// <summary>
        /// 类型转换float
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
		protected float GetFloat(string str)
		{
			float value = -1;
			if (m_CurReader == null)
			{
				return value;
			}
			float.TryParse(m_CurReader.GetDataByRowAndName(m_CurRow, str), out value);
			return value;
		}
        /// <summary>
        /// 类型转换double
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
		protected double GetDouble(string str)
		{
			double value = -1;
			if (m_CurReader == null)
			{
				return value;
			}
			double.TryParse(m_CurReader.GetDataByRowAndName(m_CurRow, str), out value);
			return value;
		}

		protected string GetStr(string str)
		{
			string value = "";
			if (m_CurReader == null)
			{
				return value;
			}
			return m_CurReader.GetDataByRowAndName(m_CurRow, str);
		}
        /// <summary>
        /// 转换int数组
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        protected int[] GetIntArray(string str)
        {
            string[] arrayStrSplit = GetArray(str);
            if (arrayStrSplit != null && arrayStrSplit.Length > 0)
            {
                int[] arrayInt = new int[arrayStrSplit.Length];
                for (int iArray = 0; iArray < arrayStrSplit.Length; iArray++)
                {
                    arrayInt[iArray] = int.Parse(arrayStrSplit[iArray]);
                }
                return arrayInt;
            }
            return null;
        }
        /// <summary>
        /// 转换float数组
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        protected float[] GetFloatArray(string str)
        {
            string[] arrayStrSplit = GetArray(str);
            if (arrayStrSplit != null && arrayStrSplit.Length > 0)
            {
                float[] arrayFloatt = new float[arrayStrSplit.Length];
                for (int iArray = 0; iArray < arrayStrSplit.Length; iArray++)
                {
                    arrayFloatt[iArray] = float.Parse(arrayStrSplit[iArray]);
                }
                return arrayFloatt;
            }
            return null;
        }
        /// <summary>
        /// 转换double数组
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        protected double[] GetDoubleArray(string str)
        {
            string []arrayStrSplit =  GetArray(str);
            if (arrayStrSplit != null && arrayStrSplit.Length > 0)
            {
                double[] arrayDouble = new double[arrayStrSplit.Length];
                for (int iArray = 0; iArray < arrayStrSplit.Length; iArray++)
                {
                    arrayDouble[iArray] = double.Parse(arrayStrSplit[iArray]);
                }
                return arrayDouble;
            }
            return null;
        }
        /// <summary>
        /// 转换string数组
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
       private string[] GetArray(string str)
        {
            string arrayStr = m_CurReader.GetDataByRowAndName(m_CurRow, str);
            if (!string.IsNullOrEmpty(arrayStr))
            {
                if (arrayStr.Length > 0)
                {
                    arrayStr = arrayStr.Replace("[", "");
                    arrayStr = arrayStr.Replace("]", "");
                    string[] arrayStrSplit = arrayStr.Split(',');
                    return arrayStrSplit;
                }
            }
            return null;
        }
        
        protected string GetStrByIntArray(int[] intArray)
        {
            StringBuilder sb = BaseVO.sm_sb;
            sb.Clear();
            if (intArray != null && intArray.Length > 0)
            {
                sb.Append("[");
                for (int index = 0; index < intArray.Length; index++)
                {
                    sb.Append(intArray[index]);
                    if (index != intArray.Length - 1)
                    {
                        sb.Append(",");
                    }
                }
                sb.Append("]");
            }
            return sb.ToString();
        }

        protected string GetStrByStrArray(string[] strArray)
        {
            StringBuilder sb = BaseVO.sm_sb;
            sb.Clear();
            if (strArray != null && strArray.Length > 0)
            {
                sb.Append("[");
                for (int index = 0; index < strArray.Length; index++)
                {
                    sb.Append(strArray[index]);
                    if (index != strArray.Length - 1)
                    {
                        sb.Append(",");
                    }
                }
                sb.Append("]");
            }
            return sb.ToString();
        }
		#endregion

	}
}
#endif
