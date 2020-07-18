#if UNITY_EDITOR
using Map;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class CSVReader
{
    private ExcelPackage m_CsvPackage;
    private ExcelWorksheet m_WorkSheet;
    private ESheetReader m_SheetReader;
    private int m_ColumnCount;
    private int m_RowCount;
    private Dictionary<string, int> m_KeyToRowDic = new  Dictionary<string, int>();

    private void Clear()
    {
        m_ColumnCount = 0;
        m_RowCount = 0;
        m_CsvPackage = null;
        m_WorkSheet = null;
        m_SheetReader = null;
        m_KeyToRowDic.Clear();
    }

    public void OpenCSV(string csvPath,string sheetName)
    {
        Clear();
        string[][] resultdata = null;
        ExcelTextFormat format = new ExcelTextFormat();
        format.Delimiter = ',';
        format.EOL = "\n";
        format.TextQualifier = '"';
        format.Encoding = new UTF8Encoding();

        string fileData = string.Empty;
        using (StreamReader reader = new StreamReader(csvPath, Encoding.UTF8))
        {
            fileData = reader.ReadToEnd();
        }

        fileData = fileData.Replace("\r", "");

        try
        {
            using (m_CsvPackage = new ExcelPackage())
            {
                ExcelWorkbook workbook = m_CsvPackage.Workbook;
                m_WorkSheet = workbook.Worksheets.Add(sheetName);
                var range = m_WorkSheet.Cells["A1"].LoadFromText(fileData, format);
                if (workbook.Worksheets.Count > 0)
                {
                    int columnMin = m_WorkSheet.Dimension.Start.Column;
                    int rowMin = m_WorkSheet.Dimension.Start.Row;

                    m_ColumnCount = m_WorkSheet.Dimension.End.Column;
                    m_RowCount = m_WorkSheet.Dimension.End.Row;
                    resultdata = new string[m_RowCount][];

                    for (int i = rowMin; i <= m_RowCount; i++)
                    {
                        bool Arow = true;
                        int trow = i - 1;
                        resultdata[i - 1] = new string[m_ColumnCount];
                        for (int j = columnMin; j <= m_ColumnCount; j++)
                        {
                            ExcelRange data = m_WorkSheet.Cells[i, j];
                            if (data != null && data.Value != null)
                            {
                                resultdata[trow][j - 1] = data.Value.ToString();
                                if (Arow)
                                {
                                    if (!m_KeyToRowDic.ContainsKey(data.Value.ToString()))
                                        m_KeyToRowDic.Add(data.Value.ToString(), trow);
                                    Arow = false;
                                }
                            }
                        }
                    }
                    m_SheetReader = new ESheetReader(resultdata);
                }

            }

        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.StackTrace);
        }
    }

    private bool ExistSheet(string sheetName)
    {
        bool exist = false;
        for (int iSheet = 0; iSheet < m_CsvPackage.Workbook.Worksheets.Count; iSheet++)
        {
            if (m_CsvPackage.Workbook.Worksheets[iSheet + 1].Name == sheetName)
            {
                exist = true;
                break;
            }
        }

        return exist;
    }

    public void CloseCsv()
    {
        m_CsvPackage.Dispose();
        m_KeyToRowDic.Clear();
    }

    public string GetCellText(int row, int column)
    {
        return m_SheetReader.GetDataByRowAndCol(row-1,column-1);
    }

    public int GetRowCount()
    {
        return m_RowCount;
    }

    public int GetColumnCount()
    {
        return m_ColumnCount;
    }
    public bool IsHaveKey<T>(T key)
    {
        if(typeof(T)== typeof(int) || typeof(T) == typeof(string) || typeof(T) == typeof(float)||
            typeof(T) == typeof(uint) || typeof(T) == typeof(ulong) || typeof(T) == typeof(long)||
             typeof(T) == typeof(double))
        {
            if (m_KeyToRowDic.ContainsKey(key.ToString()))
                return true;
        }
        return false;
    }


    public string GetCellText<T>(T key, int column)
    {
        if (!IsHaveKey<T>(key))
            return null;

        if (m_KeyToRowDic.ContainsKey(key.ToString()))
            return GetCellText(m_KeyToRowDic[key.ToString()] + 1 , column);

        return null;
    }
}
#endif