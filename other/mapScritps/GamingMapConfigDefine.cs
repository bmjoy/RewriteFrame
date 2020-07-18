#if UNITY_EDITOR
using Leyoutech.Utility;
using Map;
using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Map
{
	#region 枚举相关

    /// <summary>
    /// 战舰速度模式
    /// </summary>
    public enum SpacecraftSpeedMode
    {
        CruiseMode = 0,//巡航模式
        BattleMode,//战斗模式
        OverloadMode//过载模式
    }

	/// <summary>
	/// 配置地图类型
	/// </summary>
	public enum GamingMapType
	{
        mapInvalid = 0,
        mapMainCity,        //主城
        mapSpaceStation,
        mapNearGround,
        mapDeepSpace,
        mapPVE,
        mapCountryWar,
        mapAnnihilatePVE,	//歼灭副本
	    mapDefencePVE,		//防守副本
	    mapEscapePVE,		//逃跑副本
	    mapTeam,		    //组队副本
	    mutilTeamMap,       //多队伍副本
	    mapTotal,
	}

    public enum CollisionLayer
    {
        MainPlayer = 8,
        SkillProjectile = 9,
        HumanNPC = 10,
        SpacecraftNPC = 11,
        SpacecraftOtherPlayer = 12,
        SceneOnly = 15,
        HumanOtherPlayer = 16,
        UnselectableSpacecraft = 17
    }
    /// <summary>
    /// 触发器类型
    /// </summary>
    public enum TriggerType
    {
        Default = 0,
        Task =1
    }

    public enum KHeroType
    {
        htInvalid = -1,
        htPlayer = 0,
        htNpc = 1,
        htDynamicDecorate = 2,
        htMonster = 3,
        htStarGate = 4,
        htSpaceStation = 5,
        htBeEscortedNpc = 6,
        htEliteMonster1 = 7,
        htEliteMonster2 = 8,
        htEliteMonster3 = 9,
        htPlotMonster = 10,
        htBoss = 11,
        htSpecialMonster = 12,
        htWorldBoss = 13,
        htGeneral = 14,
        htOfflinePlayer = 15,
        htBuilding = 16,
        htPet = 17,
        htBiaoChe = 18,
        htHeroPartyBoss = 19,
        htCampFlag = 20,
        htGuard = 21,
        htProtege = 22,
        htNpcTrrgger = 23,
        htBunker = 24,
        htBattleFieldChest = 25,
        htMonsterChest = 26,
        htSceneChest = 27,
        htSceneGlobalObj = 28,
        htSpellSummonObj = 29,
        reliveObj = 30//复活npc
    }
    /// <summary>
    /// 跃迁类型
    /// </summary>
    public enum LeapType
    {
        Main = 1,//主跃迁点
        Child = 2,//副跃迁点
    }
    
    /// <summary>
    /// 寻路类型
    /// </summary>
    public enum KMapPathType
    {
        KMapPath_Invalid =0,
        KMapPath_Groud =1,
        KMapPath_Space =2
    }

    /// <summary>
    /// 行为类型
    /// </summary>
    public enum MotionType
    {
        None = 0,//不可运动
        Human = 1,//人形态
        Ship = 2//船形态
    }
	/// <summary>
	/// 点类型
	/// </summary>
	public enum LocationType
	{
		Default = 0,//默认出生点
		TeleportIn = 1,//传送门内传入点
	}

    /// <summary>
    /// 区域类型
    /// </summary>
    public enum AreaType
    {
        Normal =1,//普通区域
        Titan = 2,//泰坦区域
        Stargate = 3,//星门
        DungeonCopy = 4//副本
    }

    /// <summary>
    /// 规则类型
    /// </summary>
    public enum MissionReleaseType
    {
        NpcMission =1,//npc任务
        AreaMission=2,//区域任务
        MapMission = 3,//场景任务

    }
	#endregion
	

	#region 配置VO
    
    /// <summary>
    /// 对应icon.csv
    /// </summary>
    public class IconVO:BaseVO
    {
        public string atlas;
        public string assetName;
        public string squareName;

        public override void CopyFrom(int row, ESheetReader reader)
        {
            base.CopyFrom(row, reader);
            int.TryParse(reader.GetDataByRowAndName(row, "id(int)"), out ID);
            atlas = reader.GetDataByRowAndName(row, "atlas(string)");
            assetName = reader.GetDataByRowAndName(row, "assetName(string)");
            squareName = reader.GetDataByRowAndName(row, "squareName(string)");
        }
    }

    /// <summary>
    /// 对应npc_trigger.csv
    /// </summary>
    public class NpcTriggerVO:BaseVO
    {
        public int existtime;
        public int initialState;
        //后面的字段不加了 因为对编辑器来说没啥用

        public override void CopyFrom(int row, ESheetReader reader)
        {
            base.CopyFrom(row,reader);
            int.TryParse(reader.GetDataByRowAndName(row, "id(int)"), out ID);
            int.TryParse(reader.GetDataByRowAndName(row, "existtime(int)"), out existtime);
            int.TryParse(reader.GetDataByRowAndName(row, "initialState(int)"), out initialState);
        }
    }

    /// <summary>
    /// 对应starmap.xlsx中的Sheet1
    /// </summary>
    public class StarMapVO:BaseVO
    {
        /// <summary>
        /// 恒星ID
        /// </summary>
        public int FixedStarid;

        /// <summary>
        /// 恒星名
        /// </summary>
        public string Name;

        /// <summary>
        /// 恒星资源
        /// </summary>
        public string AssetName;

        /// <summary>
        /// 关联的id
        /// </summary>
        public int[] Relation_Id;
        /// <summary>
        /// 位置坐标
        /// </summary>
        //public Vector3 Position;

        public override void CopyFrom(int row, ESheetReader reader)
        {
            base.CopyFrom(row, reader);
            int.TryParse(reader.GetDataByRowAndName(row, "Fixedstar_id(int)"), out ID);
            FixedStarid = ID;
            Name = reader.GetDataByRowAndName(row, "name(string)");
            AssetName = reader.GetDataByRowAndName(row, "assetName(string)");
            //Position = JsonUtility.FromJson<Vector3>(reader.GetDataByRowAndName(row, "position[float]"));
            string relationIds = reader.GetDataByRowAndName(row, "relation_id[int]");
            if (!string.IsNullOrEmpty(relationIds))
            {
                if (relationIds.Length > 2)
                {
                    relationIds = relationIds.Substring(1, relationIds.Length - 2);
                }
                string[] relationIdsSplit = relationIds.Split(',');
                if (relationIdsSplit != null && relationIdsSplit.Length > 0)
                {
                    Relation_Id = new int[relationIdsSplit.Length];
                    for (int iRelation = 0; iRelation < relationIdsSplit.Length; iRelation++)
                    {
                        Relation_Id[iRelation] = int.Parse(relationIdsSplit[iRelation]);
                    }
                }
            }
        }

        public override void SetSheetData(ExcelWorksheet sheet, int row)
        {
            base.SetSheetData(sheet, row);

            sheet.Cells[row, 2].Value = Name;
            sheet.Cells[row, 3].Value = AssetName;
            //sheet.Cells[row, 4].Value = JsonUtility.ToJson(Position, true);
        }
    }

	/// <summary>
	/// 对应npc_list.xlsx中的b_npcTemplate
	/// </summary>
	public class NpcVO : BaseVO
	{
		/// <summary>
		/// NPC名称
		/// </summary>
		public string Name;

		/// <summary>
		/// 模型资源
		/// </summary>
		public int Model;

        /// <summary>
        /// npc类型
        /// </summary>
        public int NpcType;
        /// <summary>
        /// 行为类型
        /// </summary>
        public int motionType;

		public override void CopyFrom(int row, ESheetReader reader)
		{
			base.CopyFrom(row, reader);

			int.TryParse(reader.GetDataByRowAndName(row, "id(int)"), out ID);
			Name = reader.GetDataByRowAndName(row, "name(string)");
			int.TryParse(reader.GetDataByRowAndName(row, "model(int)"), out Model);
            int.TryParse(reader.GetDataByRowAndName(row, "motionType(int)"), out motionType);
            int.TryParse(reader.GetDataByRowAndName(row, "npcType(int)"),out NpcType);
        }

	}

    /// <summary>
    /// 对应group_treasure.csv表
    /// </summary>
    public class GroupTreasureVO:BaseVO
    {
        public int groupId;
        public int teamMember;
        public float offestX;
        public float offestY;
        public float offestZ;

        public override void CopyFrom(int row, ESheetReader reader)
        {
            base.CopyFrom(row, reader);
            groupId = int.Parse(reader.GetDataByRowAndName(row, "id(int)"));
            teamMember = int.Parse(reader.GetDataByRowAndName(row, "teamMember(int)"));
            offestX = float.Parse(reader.GetDataByRowAndName(row, "offsetX(float)"));
            offestY = float.Parse(reader.GetDataByRowAndName(row, "offestY(float)"));
            offestZ = float.Parse(reader.GetDataByRowAndName(row, "offestZ(float)"));
        }

        public override void SetSheetData(ExcelWorksheet sheet, int row)
        {
            base.SetSheetData(sheet, row);
            sheet.Cells[row, 1].Value = groupId;
            sheet.Cells[row, 2].Value = teamMember;
            sheet.Cells[row, 3].Value = offestX;
            sheet.Cells[row, 4].Value = offestY;
            sheet.Cells[row, 5].Value = offestZ;
        }
    }


    /// <summary>
    /// 对应group_mineral.csv表
    /// </summary>
    public class GroupMineralVO : BaseVO
    {
        public int groupId;
        public int teamMember;
        public float offestX;
        public float offestY;
        public float offestZ;

        public override void CopyFrom(int row, ESheetReader reader)
        {
            base.CopyFrom(row, reader);
            groupId = int.Parse(reader.GetDataByRowAndName(row, "id(int)"));
            teamMember = int.Parse(reader.GetDataByRowAndName(row, "teamMember(int)"));
            offestX = float.Parse(reader.GetDataByRowAndName(row, "offsetX(float)"));
            offestY = float.Parse(reader.GetDataByRowAndName(row, "offestY(float)"));
            offestZ = float.Parse(reader.GetDataByRowAndName(row, "offestZ(float)"));
        }

        public override void SetSheetData(ExcelWorksheet sheet, int row)
        {
            base.SetSheetData(sheet, row);
            sheet.Cells[row, 1].Value = groupId;
            sheet.Cells[row, 2].Value = teamMember;
            sheet.Cells[row, 3].Value = offestX;
            sheet.Cells[row, 4].Value = offestY;
            sheet.Cells[row, 5].Value = offestZ;
        }
    }

    /// <summary>
    /// 对应language_mapeditor.xlsx的c_localization
    /// </summary>
    public class LanguageMapEditorVO:BaseVO
    { 
        public string key;
        /// <summary>
        /// 备注
        /// </summary>
        public string remarks;
        /// <summary>
        /// 简体中文
        /// </summary>
        public string chs;
        /// <summary>
        /// 美式英语
        /// </summary>
        public string enUs;

        public override void CopyFrom(int row, ESheetReader reader)
        {
            base.CopyFrom(row, reader);
            key = reader.GetDataByRowAndName(row, "id(string)");
            strID = key;
            chs = reader.GetDataByRowAndName(row, "chs(string)");
            enUs = reader.GetDataByRowAndName(row, "en_us(string)");
        }

        public override void SetSheetData(ExcelWorksheet sheet, int row)
        {
            base.SetSheetData(sheet, row);
            sheet.Cells[row, 1].Value = key;
            sheet.Cells[row, 2].Value = remarks;
            sheet.Cells[row, 3].Value = chs;
            sheet.Cells[row, 4].Value = enUs;
        }
    }

	/// <summary>
	/// 对应npc_list.xlsx中的c_localization
	/// </summary>
	public class NpcListLocalizationVO : BaseVO
	{
		/// <summary>
		/// 中文文字
		/// </summary>
		public string zh_cn;


		public override void CopyFrom(int row, ESheetReader reader)
		{
			base.CopyFrom(row, reader);
			int.TryParse(reader.GetDataByRowAndName(row, "ID"), out ID);
			zh_cn = reader.GetDataByRowAndName(row, "zh_cn");
		}
        
    }


    /// <summary>
    /// 对应碰撞分层表
    /// </summary>
    public class LayerCollisionVO : BaseVO
    {
        public int LayerId;
        public string Name;

        public int MainPlayer;
        public int SkillProjectile;
        public int HumanNPC;
        public int SpacecraftNPC;
        public int SpacecraftOtherPlayer;
        public int SceneOnly;
        public int HumanOtherPlayer;

        public int UnselectableSpacecraft;

        public override void CopyFrom(int row, ESheetReader reader)
        {
            base.CopyFrom(row, reader);
            int.TryParse(reader.GetDataByRowAndName(row, "id(int)"),out ID);
            LayerId = ID;
            Name = reader.GetDataByRowAndName(row, "name(string)");
            MainPlayer = int.Parse(reader.GetDataByRowAndName(row, "mainplayer(int)"));
            SkillProjectile = int.Parse(reader.GetDataByRowAndName(row, "skillprojectile(int)"));
            HumanNPC = int.Parse(reader.GetDataByRowAndName(row, "humannpc(int)"));
            SpacecraftNPC = int.Parse(reader.GetDataByRowAndName(row, "spacecraftnpc(int)"));
            SpacecraftOtherPlayer =int.Parse(reader.GetDataByRowAndName(row, "spacecraftotherpayer(int)"));
            SceneOnly = int.Parse(reader.GetDataByRowAndName(row, "sceneonly(int)"));
            HumanOtherPlayer = int.Parse(reader.GetDataByRowAndName(row, "humanotherplayer(int)"));
            UnselectableSpacecraft = int.Parse(reader.GetDataByRowAndName(row, "unselectablespacecraft(int)"));
        }

        public override void SetSheetData(ExcelWorksheet sheet, int row)
        {
            base.SetSheetData(sheet, row);
            sheet.Cells[row, 1].Value = ID;
            sheet.Cells[row, 2].Value = Name;
            sheet.Cells[row, 3].Value = MainPlayer;
            sheet.Cells[row, 4].Value = SkillProjectile;
            sheet.Cells[row, 5].Value = HumanNPC;
            sheet.Cells[row, 6].Value = SpacecraftNPC;
            sheet.Cells[row, 7].Value = SpacecraftOtherPlayer;
            sheet.Cells[row, 8].Value = SceneOnly;
            sheet.Cells[row, 9].Value = HumanOtherPlayer;
            sheet.Cells[row, 10].Value = UnselectableSpacecraft;
        }
    }

    /// <summary>
    /// 对应teleport.xlsx中的teleport
    /// </summary>
    public class TeleportVO : BaseVO
	{
		/// <summary>
		/// 起点地图
		/// </summary>
		public int StartGamingMap;
		/// <summary>
		/// 起点地图区域
		/// </summary>
		public ulong StartGamingMapArea;

		/// <summary>
		/// 通道列表
		/// </summary>
		public int[] ChanelList;

		public override void CopyFrom(int row, ESheetReader reader)
		{
			base.CopyFrom(row, reader);
			int.TryParse(reader.GetDataByRowAndName(row, "id(int)"), out ID);
			int.TryParse(reader.GetDataByRowAndName(row, "startGamingMap(int)"), out StartGamingMap);
			ulong.TryParse(reader.GetDataByRowAndName(row, "startGamingmapArea(int)"), out StartGamingMapArea);
			string channelStr = reader.GetDataByRowAndName(row, "chanelList[int]");
			if (!string.IsNullOrEmpty(channelStr))
			{
				if (channelStr.Length > 2)
				{
					channelStr = channelStr.Substring(1, channelStr.Length - 2);
				}
				string[] channelSplit = channelStr.Split(',');
				if (channelSplit != null && channelSplit.Length > 0)
				{
					ChanelList = new int[channelSplit.Length];
					for (int iChannel = 0; iChannel < channelSplit.Length; iChannel++)
					{
						ChanelList[iChannel] = int.Parse(channelSplit[iChannel]);
					}
				}
			}
		}
	}

	/// <summary>
	/// 对应teleport.xlsx中的chanel
	/// </summary>
	public class TeleportChanelVO : BaseVO
	{
		/// <summary>
		/// 终点地图
		/// </summary>
		public uint EndGamingMap;
		/// <summary>
		/// 终点地图区域
		/// </summary>
		public ulong EndGamingMapArea;


		public override void CopyFrom(int row, ESheetReader reader)
		{

			base.CopyFrom(row, reader);
			int.TryParse(reader.GetDataByRowAndName(row, "id(int)"), out ID);
			uint.TryParse(reader.GetDataByRowAndName(row, "endGamingMap(int)"), out EndGamingMap);
			ulong.TryParse(reader.GetDataByRowAndName(row, "endGamingMapArea(int)"), out EndGamingMapArea);
		}
	}

    /// <summary>
    /// 对应mission_release.csv
    /// </summary>
    public class MissionReleaseVO:BaseVO
    {
        /// <summary>
        /// 规则类型
        /// </summary>
        public int type;


        /// <summary>
        /// 等级下限
        /// </summary>
        public int levelMin;

        /// <summary>
        /// 等级上限
        /// </summary>
        public int levelMax;

        /// <summary>
        /// 开始时间
        /// </summary>
        public string startTime;

        /// <summary>
        /// 结束时间
        /// </summary>
        public string finishTime;

        /// <summary>
        /// 任务组id
        /// </summary>
        public int missionGroupId;


        public override void CopyFrom(int row, ESheetReader reader)
        {

            base.CopyFrom(row, reader);
            int.TryParse(reader.GetDataByRowAndName(row, "id(int)"), out ID);
            int.TryParse(reader.GetDataByRowAndName(row, "releaseType(int)"), out type);
            int.TryParse(reader.GetDataByRowAndName(row, "levelMin(int)"), out levelMin);
            int.TryParse(reader.GetDataByRowAndName(row, "levelMax(int)"), out levelMax);
            startTime = reader.GetDataByRowAndName(row, "startTime(string)");
            finishTime = reader.GetDataByRowAndName(row, "finishTime(string)");
            int.TryParse(reader.GetDataByRowAndName(row, "missionGroupId(int)"), out missionGroupId);
        }
    }

	/// <summary>
	/// 对应model.xlsx
	/// </summary>
	public class ModelVO : BaseVO
	{
		/// <summary>
		/// 资源地址
		/// </summary>
		public string assetName;
        /// <summary>
        /// 模型类型
        /// </summary>
        public int type;

		public override void CopyFrom(int row, ESheetReader reader)
		{

			base.CopyFrom(row, reader);
			int.TryParse(reader.GetDataByRowAndName(row, "id(int)"), out ID);
			assetName = reader.GetDataByRowAndName(row, "assetName(string)");
            int.TryParse(reader.GetDataByRowAndName(row, "type(int)"), out type);
        }
	}


    /// <summary>
    /// 对应effect_speed.csv
    /// </summary>
    public class EffectSpeedVO:BaseVO
    {
        public int effectId;
        /// <summary>
        /// 备注
        /// </summary>
        public string remarks;
        /// <summary>
        /// 函数
        /// </summary>
        public string function;
        /// <summary>
        /// 属性
        /// </summary>
        public string attribute;

        /// <summary>
        /// 属性（中文注释）
        /// </summary>
        public string cnattribute;
        /// <summary>
        /// 值
        /// </summary>
        public float value;
        /// <summary>
        /// 计算位于层数
        /// </summary>
        public int pipeLv;

        /// <summary>
        /// 模型
        /// </summary>
        public string model;

        public override void CopyFrom(int row, ESheetReader reader)
        {
            base.CopyFrom(row, reader);
            int.TryParse(reader.GetDataByRowAndName(row, "id(int)"), out ID);
            effectId = ID;
            function = reader.GetDataByRowAndName(row, "function(string)");
            attribute = reader.GetDataByRowAndName(row, "attribute(string)");
            float.TryParse(reader.GetDataByRowAndName(row, "value(float)"), out value);
            int.TryParse(reader.GetDataByRowAndName(row, "pipeLv(int)"), out pipeLv);
            model = reader.GetDataByRowAndName(row, "model");
        }
    }

    public class ModShipDecorateVO:BaseVO
    {
        public int unitId;
        /// <summary>
        /// 碰撞类型 默认都是3 胶囊碰撞体
        /// </summary>
        public int type;

        /// <summary>
        /// 碰撞最大临界点
        /// </summary>
        public EditorDecorateSize colliderMax;

        /// <summary>
        ///碰撞最小临界点
        /// </summary>
        public EditorDecorateSize colliderMin;

        public override void CopyFrom(int row, ESheetReader reader)
        {
            base.CopyFrom(row, reader);
            int.TryParse(reader.GetDataByRowAndName(row, "id(int)"), out ID);
            unitId = ID;
            int.TryParse(reader.GetDataByRowAndName(row, "type(int)"), out type);
            colliderMax = JsonUtility.FromJson<EditorDecorateSize>(reader.GetDataByRowAndName(row, "colliderMax[float]"));
            colliderMin = JsonUtility.FromJson<EditorDecorateSize>(reader.GetDataByRowAndName(row, "colliderMin[float]"));
        }

        public override void SetSheetData(ExcelWorksheet sheet, int row)
        {
            base.SetSheetData(sheet, row);
            sheet.Cells[row, 2].Value = type;
            sheet.Cells[row, 3].Value = JsonUtility.ToJson(colliderMax, true);
            sheet.Cells[row, 4].Value = JsonUtility.ToJson(colliderMin, true);
        }
    }

    
	#endregion

	#region xslx相关
	public class ESheetReader
	{
		private string[][] data;
		public ESheetReader(string[][] _data)
		{
			data = _data;
		}
		public int GetRowCount()
		{
			return data.Length;
		}
		public string GetDataByRowAndCol(int _row, int _col)
		{
			if (data == null || data.Length <= 0 || _row >= data.Length) return "";

			if (_col >= data[0].Length) return "";

			return data[_row][_col];
		}

		public string GetDataByRowAndName(int _row, string _name)
		{
			if (data.Length <= 2)
				return "";

			int colindex = -1;
			int colcount = data[2].Length;
			for (int i = 0; i < colcount; i++)
			{
				string key = data[2][i];
				if (key == _name)
				{
					colindex = i;
					break;
				}
			}
			if (colindex != -1)
			{
				return data[_row][colindex];
			}
			return "";
		}
	}

	public class EditorConfigData
	{
		private static EditorConfigData sm_Instance = null;

		public static EditorConfigData Instance
		{
			get
			{
				if (sm_Instance == null)
				{
					sm_Instance = new EditorConfigData();
				}
				return sm_Instance;
			}
		}

		public static string GetExcelFilePath(string name)
		{
			string path = "";
			GamingMapEditorSetting gamingSetting = GamingMapEditorUtility.GetGamingMapEditorSetting();
			if(gamingSetting != null)
			{
				path = string.Format("{0}/{1}", MapEditorUtility.GetFullPath(gamingSetting.m_ConfigPath), name);
			}
			return path;
		}

        public static ESheetReader GetCSVFileSheetReader(string fileName,string sheetName)
        {
            //sheetName = "Csv1";
            string[][] resultdata = null;
            string path = GetExcelFilePath(fileName);
            //FileInfo fileInfo = new FileInfo(path);
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

            try
            {
                using (ExcelPackage excelPkg = new ExcelPackage())
                {
                    ExcelWorkbook workbook = excelPkg.Workbook;
                    ExcelWorksheet worksheet = workbook.Worksheets.Add(sheetName);
                    var range = worksheet.Cells["A1"].LoadFromText(fileData, format);
                    if (workbook.Worksheets.Count > 0)
                    {
                        int columnMin = worksheet.Dimension.Start.Column;
                        int rowMin = worksheet.Dimension.Start.Row;

                        int columnCount = worksheet.Dimension.End.Column; //工作区结束列
                        int rowCount = worksheet.Dimension.End.Row; //工作区结束行号
                        resultdata = new string[rowCount][];

                        for (int i = rowMin; i <= rowCount; i++)
                        {
                            resultdata[i - 1] = new string[columnCount];
                            for (int j = columnMin; j <= columnCount; j++)
                            {
                                ExcelRange data = worksheet.Cells[i, j];
                                if (data != null && data.Value != null)
                                {
                                    resultdata[i - 1][j - 1] = data.Value.ToString();
                                }
                                //默认设置为0,防止强转错误
                                if (string.IsNullOrEmpty(resultdata[i - 1][j - 1]))
                                {
                                    resultdata[i - 1][j - 1] = "0";
                                }
                            }
                        }
                        ESheetReader result = new ESheetReader(resultdata);
                        return result;
                    }

                }

            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.StackTrace);
            }
            return null;
        }

		//读取一个xlsx文件的一个sheet表,并返回数据行列
		public static ESheetReader GetXlsxFileSheetReader(string _filename, string _sheetname)
		{
			string[][] resultdata = null;
			string path = GetExcelFilePath(_filename);
			FileStream fileStream = new FileStream(path, FileMode.Open);
			try
			{
				ExcelPackage excelPkg = new ExcelPackage(fileStream);
				ExcelWorkbook workbook = excelPkg.Workbook;
				if (workbook.Worksheets.Count > 0)
				{
					ExcelWorksheet worksheet = workbook.Worksheets[_sheetname];
					int columnMin = worksheet.Dimension.Start.Column;
					int rowMin = worksheet.Dimension.Start.Row;

					int columnCount = worksheet.Dimension.End.Column; //工作区结束列
					int rowCount = worksheet.Dimension.End.Row; //工作区结束行号
					resultdata = new string[rowCount][];

					for (int i = rowMin; i <= rowCount; i++)
					{
						resultdata[i - 1] = new string[columnCount];
						for (int j = columnMin; j <= columnCount; j++)
						{
							ExcelRange data = worksheet.Cells[i, j];
							if (data != null && data.Value != null)
							{
								resultdata[i - 1][j - 1] = data.Value.ToString();
							}
							//默认设置为0,防止强转错误
							if (string.IsNullOrEmpty(resultdata[i - 1][j - 1]))
							{
								resultdata[i - 1][j - 1] = "0";
							}
						}
					}
					ESheetReader result = new ESheetReader(resultdata);
					fileStream.Close();
					return result;
				}
			}
			catch (System.Exception ex)
			{
				Debug.LogError(ex.StackTrace);
			}
			finally
			{
				fileStream.Close();
			}
			return null;
		}

        private void AddInfo(string name,string typeName)
        {
            List<string> npcListInfo = new List<string>();
            npcListInfo.Add(string.Format("{0}.csv", name));
            npcListInfo.Add(name);
            EditorGamingMapData.m_VoNameDic.Add(typeName, npcListInfo);
        }

		public IEnumerator InitExcelData()
		{
			ConfigVO<NpcVO>.Instance.Clear();
			ConfigVO<TeleportVO>.Instance.Clear();
			ConfigVO<TeleportChanelVO>.Instance.Clear();
			ConfigVO<ModelVO>.Instance.Clear();
            ConfigVO<MissionReleaseVO>.Instance.Clear();

			EditorGamingMapData.m_VoNameDic = new Dictionary<string, List<string>>();
            AddInfo("npc", typeof(NpcVO).Name);
            AddInfo("teleport", typeof(TeleportVO).Name);
            AddInfo("teleport_chanel", typeof(TeleportChanelVO).Name);
            AddInfo("model", typeof(ModelVO).Name);
            AddInfo("npc_trigger", typeof(NpcTriggerVO).Name);
            yield return null;
            AddInfo("icon", typeof(IconVO).Name);
            AddInfo("mission_release", typeof(MissionReleaseVO).Name);

            ConfigVO<NpcVO>.Instance.GetList();
			yield return null;

			ConfigVO<TeleportVO>.Instance.GetList();
			yield return null;

			ConfigVO<TeleportChanelVO>.Instance.GetList();
			yield return null;
			ConfigVO<ModelVO>.Instance.GetList();
			yield return null;
            ConfigVO<NpcTriggerVO>.Instance.GetList();
            yield return null;
            ConfigVO<IconVO>.Instance.GetList();
            yield return null;
            ConfigVO<MissionReleaseVO>.Instance.GetList();
            yield return null;
		}

	}
    #endregion

    #region Json数据

    #region GamingMap
    [Serializable]
    public class EditorGamingMap
    {
        public uint gamingmapId;

        public string gamingmapName;

        public int gamingType;

        public uint mapId;

        public int maxPlayerNum;

        public int removeSecond;

        public int pathType;

        /// <summary>
        /// 所属恒星
        /// </summary>
        public int belongFixedStar;

        /// <summary>
        /// 人形态复活的地图id
        /// </summary>
        public uint spaceGamingMap;
        /// <summary>
        /// 任务发布规则id列表
        /// </summary>
        public int[] sceneMissionReleaseId;
        /// <summary>
        /// 泰坦区域所属GamingMapID
        /// </summary>
        public uint ttGamingMapId;
        /// <summary>
        /// 泰坦区域id
        /// </summary>
        public ulong ttGamingAreaId;

        public EditorArea[] areaList;

        public EditorArea GetAreaByAreaId(ulong areaId)
        {
            if (areaList != null && areaList.Length > 0)
            {
                for (int iArea = 0; iArea < areaList.Length; iArea++)
                {
                    if (areaList[iArea].areaId == areaId)
                    {
                        return areaList[iArea];
                    }
                }
            }
            return null;
        }
    }

    [Serializable]
    public class EditorArea
    {
        public ulong areaId;

        public string areaName;

        /// <summary>
        /// 区域类型 1普通区域 2泰坦区域
        /// </summary>
        public int areaType;

        /// <summary>
        /// 当为副跃迁点时 即leap_type == 2 该字段才生效 返回main_leapid所在的areaId
        /// </summary>
        public ulong fatherArea;

        /// <summary>
        /// 区域内的复活npcid
        /// </summary>
        public ulong relieveCreature;
        /// <summary>
        /// 当为主跃迁点时 即leap_type == 1 此字段生效 返回main_leapid == 自己的leap_id对应的区域列表
        /// </summary>
        public ulong[] childrenAreaList;

        /// <summary>
        /// 任务发布规则id列表
        /// </summary>
        public int[] sceneMissionReleaseId;
        /// <summary>
        /// 存储Area的位置
        /// </summary>
        public EditorPosition position;

        public EditorCreature[] creatureList;

        public EditorTeleport[] teleportList;

        public EditorLocation[] locationList;

        public EditorLeap[] leapList;

        public EditorTrigger[] triggerList;

        public EditorTreasure[] treasureList;

        public EditorMineral[] mineralList;
    }

    /// <summary>
    /// 寻宝信息
    /// </summary>
    [Serializable]
    public class EditorTreasure
    {
        public uint treasureIndex;
        public uint treasureNpcId;
        public uint treasureGroupId;
        public string name;
        public EditorPosition tresurePos;
    }

    /// <summary>
    /// 采矿信息
    /// </summary>
    [Serializable]
    public class EditorMineral
    {
        public uint mineralIndex;
        public uint mineralNpcId;
        public uint mineralGroupId;
        public string name;
        public EditorPosition mineralPos;
    }

    [Serializable]
    public class EditorTrigger
    {
        /// <summary>
        /// 唯一id
        /// </summary>
        public ulong triggerIndex;
        /// <summary>
        /// npc_trigger表中的id
        /// </summary>
        public uint triggerId;
        /// <summary>
        /// NPC id
        /// </summary>
        public uint triggerNpcId;

        public EditorPosition position;

        public EditorRotation rotation;
        /// <summary>
        /// 0 不自动创建  1 自动创建
        /// </summary>
        public int autoCreation;
        /// <summary>
        /// 复活随机半径
        /// </summary>
        public float reviveRange;
    }


    [Serializable]
    public class EditorCreature
    {
        public ulong creatureId;

        public int tplId;

        public EditorPosition position;

        public EditorRotation rotation;

        public int autoCreation;

        public int teleportId;

        /// <summary>
        /// 如果是复活npc 则是个复活半径
        /// </summary>
        public float reviveRange;
    }

    [Serializable]
    public class EditorPosition2D
    {
        public float x;
        public float y;

        public EditorPosition2D(Vector3 pos)
        {
            x = pos.x;
            y = pos.y;
        }

        public EditorPosition2D(Vector2 pos)
        {
            x = pos.x;
            y = pos.y;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x,y,0);
        }

        public Vector2 ToVector2()
        {
            return new Vector2(x, y);
        }
    }

    [Serializable]
    public class EditorPosition
    {
        public float x;

        public float y;

        public float z;

        public EditorPosition(Vector3 pos)
        {
            x = pos.x;
            y = pos.y;
            z = pos.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x,y,z);
        }
    }

    [Serializable]
    public class EditorRotation
    {
        public float x;

        public float y;

        public float z;

        public float w;

        public EditorRotation(Quaternion qua)
        {
            x = qua.x;
            y = qua.y;
            z = qua.z;
            w = qua.w;
        }
    }

    [Serializable]
    public class EditorTeleport
    {
        public int teleportId;

        public ulong creaureId;

        public EditorChanel[] chanelList;
    }

    [Serializable]
    public class EditorChanel
    {
        public int chanelId;

        public ulong eLocation;
    }

    [Serializable]
    public class EditorLocation
    {
        public ulong locationId;

        public string locationName;

        public int locationType;

        public EditorPosition position;

        public EditorRotation rotation;
    }

    [Serializable]
    public class EditorLeap
    {
        public ulong leapId;

        public string leapName;

        public string description;

        //public string iconName;

        public int iconConfId;

        public int leapType;

        public ulong mainLeapId;

        public float range;

        public EditorPosition position;

        public int autoVisible;

        public ulong[] visibleLeapList;
    }
    #endregion

    #region 碰撞数据

    [Serializable]
    public class EditorCollider
    {
        public EditorColliderArea[] areaList;
        public EditorDecorate[] commondecorateList;
        public uint mapID;
    }

    [Serializable]
    public class EditorColliderArea
    {
        public EditorDecorate[] decorateList;
        public ulong areaId;
    }

    [Serializable]
    public class EditorDecorate
    {
        public int id;
        public int type;
        public string path;
        public string scenePath;
        public EditorDecorateSize dirMax;
        public EditorDecorateSize dirMin;
        public EditorRotation dir;
    }

    [Serializable]
    public class EditorDecorateSize
    {
        public float x;
        public float y;
        public float z;

        public EditorDecorateSize(Vector3 size)
        {
            x = size.x;
            y = size.y;
            z = size.z;
        }

        public override string ToString()
        {
           string jsonStr = JsonUtility.ToJson(this, true);
            jsonStr = jsonStr.Replace("\"","\"\"");
            return jsonStr;
        }
    }
    #endregion

    #region 星图数据
    [Serializable]
    public class EditorStarMap
    {
        public EditorFixedStar[] fixedStars;
    }
    
    [Serializable]
    public class EditorFixedStar
    {
        public int fixedStarId;
        public string fixedStarName;
        public string fixedStarRes;
        public int[] relations;
        /// <summary>
        /// 泰坦区域所属GamingMapID
        /// </summary>
        public uint ttGamingMapId;
        /// <summary>
        /// 泰坦区域id
        /// </summary>
        public ulong ttGamingAreaId;

        public EditorPosition2D position;
        public EditorPlanet[] planetList;
    }

    [Serializable]
    public class EditorPlanet
    {
        public uint gamingmapId;
        public string gamingmapName;
        public string gamingmapRes;
        public EditorPosition2D position;
        public EditorPosition2D scale;
        public float minimapSize;

        public string bgmapRes;
        public EditorPosition2D bgmapPos;
        public EditorPosition2D bgmapScale;
        public EditorStarMapArea[] arealist;
    }

    [Serializable]
    public class EditorStarMapArea
    {
        public ulong areaId;
        public string areaName;
        public string area_res;
        public int area_leap_type;
        public int areaType;
        /// <summary>
        /// 当为主跃迁点时 即leap_type == 1 此字段生效 返回main_leapid == 自己的leap_id对应的区域列表
        /// </summary>
        public ulong[] childrenAreaList;

        public EditorPosition2D position;
    }
    #endregion

    #region 战舰数据

    [Serializable]
    public class EditorSpacecraft
    {
        /// <summary>
        /// 编号id
        /// </summary>
        public int spacecraft_id;
        /// <summary>
        /// 模型名称
        /// </summary>
        public string modelName;
        public EditorSpacecraftMode[] spacecraft_modes;
    }

    [Serializable]
    public class EditorSpacecraftMode
    {
        /// <summary>
        /// 速度模式
        /// </summary>
        public int speed_mode;

        /// <summary>
        /// 移动能力
        /// </summary>
        public EditorSpacecraftMoveable movebale;

        /// <summary>
        /// 转向能力
        /// </summary>
        public EditorSpacecraftTurnable turnable;

        /// <summary>
        /// 拟态表现
        /// </summary>
        public EditorSpacecraftMimicry mimicry;

        /// <summary>
        /// 跃迁能力
        /// </summary>
        public EditorSpacecraftTransition transition;

        public void Init()
        {
            speed_mode = 0;
            movebale = new EditorSpacecraftMoveable();
            movebale.Init();
            turnable = new EditorSpacecraftTurnable();
            turnable.Init();
            mimicry = new EditorSpacecraftMimicry();
            transition = new EditorSpacecraftTransition();
        }

        public void Copy(EditorSpacecraftMode target)
        {
            speed_mode = target.speed_mode;
            if(movebale == null)
            {
                movebale = new EditorSpacecraftMoveable();
            }
            movebale.Copy(target.movebale);
            if(turnable == null)
            {
                turnable = new EditorSpacecraftTurnable();
            }
            turnable.Copy(target.turnable);
            if(mimicry == null)
            {
                mimicry = new EditorSpacecraftMimicry();
            }
            mimicry.Copy(target.mimicry);
            if(transition == null)
            {
                transition = new EditorSpacecraftTransition();
            }
            transition.Copy(target.transition);
        }
    }


    /// <summary>
    /// 移动能力
    /// </summary>
    [Serializable]
    public class EditorSpacecraftMoveable
    {
        /// <summary>
        /// 最大水平速度
        /// </summary>
        public Vector4 max_horspeed;
        /// <summary>
        /// 最大升降速度
        /// </summary>
        public Vector2 max_verspeed;

        /// <summary>
        /// 水平前进加速度
        /// </summary>
        public float hor_forspeed;

        /// <summary>
        /// 水平后退加速度
        /// </summary>
        public float hor_backspeed;

        /// <summary>
        /// 左右平移加速度
        /// </summary>
        public Vector2 hor_movespeed;

        /// <summary>
        /// 升降加速度
        /// </summary>
        public Vector2 ver_movespeed;

        /// <summary>
        /// 水平减速度
        /// </summary>
        public Vector2 hor_despeed;
        /// <summary>
        /// 升降减速度
        /// </summary>
        public float ver_despeed;

        public void Init()
        {
            //max_horspeed = new EditorVector4();
            //max_verspeed = new EditorVector2();
            //hor_movespeed = new EditorVector2();
            //ver_movespeed = new EditorVector2();
            //hor_despeed = new EditorVector2();

            max_horspeed = default(Vector4);
            max_verspeed = default(Vector2);
            hor_movespeed = default(Vector2);
            ver_movespeed = default(Vector2);
            hor_despeed = default(Vector2);
        }
        public void Copy(EditorSpacecraftMoveable moveData)
        {
            max_horspeed = moveData.max_horspeed;
            max_verspeed = moveData.max_verspeed;
            hor_forspeed = moveData.hor_forspeed;
            hor_backspeed = moveData.hor_backspeed;
            hor_movespeed = moveData.hor_movespeed;
            ver_movespeed = moveData.ver_movespeed;
            ver_despeed = moveData.ver_despeed;
            hor_despeed = moveData.hor_despeed;
        }
    }

    /// <summary>
    /// 转向能力
    /// </summary>
    [Serializable]
    public class EditorSpacecraftTurnable
    {
        /// <summary>
        /// 最大转向角度
        /// </summary>
        public Vector3 max_turnangle;

        /// <summary>
        /// 转向角加速度
        /// </summary>
        public Vector3 turn_speed;

        /// <summary>
        /// 转向角减速度
        /// </summary>
        public Vector3 turn_despeed;

        public void Init()
        {
            //max_turnangle = new EditorVector3();
            //turn_speed = new EditorVector3();
            //turn_despeed = new EditorVector3();
            max_turnangle = default(Vector3);
            turn_speed = default(Vector3);
            turn_despeed = default(Vector3);
        }

        public void Copy(EditorSpacecraftTurnable target)
        {
            //if (max_turnangle == null)
            //{
            //    max_turnangle = new EditorVector3();
            //}
            //max_turnangle.Copy(target.max_turnangle);
            //if (turn_speed == null)
            //{
            //    turn_speed = new EditorVector3();
            //}
            //turn_speed.Copy(target.turn_speed);
            //if (turn_despeed == null)
            //{
            //    turn_despeed = new EditorVector3();
            //}
            //turn_despeed.Copy(target.turn_despeed);

            max_turnangle = target.max_turnangle;
            turn_speed = target.turn_speed;
            turn_despeed = target.turn_despeed;
        }
    }


    /// <summary>
    /// 拟态表现
    /// </summary>
    [Serializable]
    public class EditorSpacecraftMimicry
    {
        /// <summary>
        /// 转向拟态最大倾角
        /// </summary>
        public float turn_maxangle;

        /// <summary>
        /// 转向拟态角加速度
        /// </summary>
        public float turn_angelespeed;

        /// <summary>
        /// 升降拟态最大倾角
        /// </summary>
        public float ver_maxangle;
        /// <summary>
        /// 升降拟态角加速度
        /// </summary>
        public float ver_anglespeed;

        public void Copy(EditorSpacecraftMimicry target)
        {
            turn_maxangle = target.turn_maxangle;
            turn_angelespeed = target.turn_angelespeed;
            ver_maxangle = target.ver_maxangle;
            ver_anglespeed = target.ver_anglespeed;
        }
    }


    /// <summary>
    /// 跃迁能力
    /// </summary>
    [Serializable]
    public class EditorSpacecraftTransition
    {
        /// <summary>
        /// 跃迁起始速度
        /// </summary>
        public float originspeed;
        /// <summary>
        /// 跃迁加速度
        /// </summary>
        public float speed;
        /// <summary>
        /// 跃迁减速度
        /// </summary>
        public float despeed;

        public void Copy(EditorSpacecraftTransition target)
        {
            originspeed = target.originspeed;
            speed = target.speed;
            despeed = target.despeed;
        }
    }
    [Serializable]
    public class EditorVector3
    {
        public float x;
        public float y;
        public float z;

        public void Copy(EditorVector3 target)
        {
            this.x = target.x;
            this.y = target.y;
            this.z = target.z;
        }
        public Vector3 ToVector3()
        {
            return new Vector3(x,y,z);
        }
    }

    [Serializable]
    public class EditorVector4
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public void Copy(EditorVector4 target)
        {
            this.x = target.x;
            this.y = target.y;
            this.z = target.z;
            this.w = target.w;
        }

        public Vector4 ToVector4()
        {
            return new Vector4(x,y,z,w);
        }
    }

    [Serializable]
    public class EditorVector2
    {
        public float x;
        public float y;
        public void Copy(EditorVector2 target)
        {
            this.x = target.x;
            this.y = target.y;
        }
        public Vector2 ToVector2()
        {
            return new Vector2(x,y);
        }
    }
    #endregion
    #endregion

    public class EditorGamingMapData
    {
        /// <summary>
        /// 对应GamingMapType名称
        /// </summary>
        public static string[] GAMINGMAPTYPE_NAME = { "mapInvalid", "mapMainCity", "mapSpaceStation", "mapNearGround", "mapDeepSpace", "mapPVE", "mapCountryWar", "mapAnnihilatePVE",
        "mapDefencePVE","mapEscapePVE","mapTeam","mutilTeamMap","mapTotal"};

        /// <summary>
        /// 对应LocationType名称
        /// </summary>
        public static string[] LOCATIONTYPE_NAME = { "默认出生点", "传送门内传入点" };

        public static Dictionary<string, List<string>> m_VoNameDic;

        #region GamingMap相关
        /// <summary>
        /// 保存GamingMap
        /// </summary>
        /// <param name="map"></param>
        /// <param name="outData"></param>
        private static void SaveGamingMap(GamingMap map, EditorGamingMap outData,List<ulong> areaIds = null)
        {
            outData.gamingmapId = map.m_Uid;
            outData.gamingmapName = map.m_MapName;
            outData.gamingType = (int)map.m_Type;
            outData.mapId = map.m_MapId;
            outData.pathType = (int)map.m_PathType;
            if (map.m_Type == GamingMapType.mapDeepSpace)
            {
                outData.belongFixedStar = map.m_FixedStarId;
            }
            else
            {
                outData.belongFixedStar = 0;
            }

            outData.maxPlayerNum = map.m_MaxPlayerNum;
            outData.removeSecond = map.m_RemoveSecond;
            outData.spaceGamingMap = map.m_SpaceGamingMapId;
            if (map.m_GamingAreaList != null && map.m_GamingAreaList.Count > 0)
            {
                //if(areaIds == null)
                //{
                //    outData.areaList = new EditorArea[map.m_GamingAreaList.Count];
                //    for (int iGaming = 0; iGaming < map.m_GamingAreaList.Count; iGaming++)
                //    {
                //        EditorArea area = new EditorArea();
                //        outData.areaList[iGaming] = area;
                //        SaveGamingArea(map.m_GamingAreaList[iGaming], area);
                //    }
                //}
                //else
                if(areaIds != null)
                {
                    List<EditorArea> editorAreaList = new List<EditorArea>();
                    if(outData.areaList != null)
                    {
                        for(int iArea =0;iArea<outData.areaList.Length;iArea++)
                        {
                            editorAreaList.Add(outData.areaList[iArea]);
                        }
                    }
                    for (int iGaming = 0; iGaming < map.m_GamingAreaList.Count; iGaming++)
                    {
                        ulong areaId = map.m_GamingAreaList[iGaming].m_AreaId;
                        if (!areaIds.Contains(areaId))
                        {
                            continue;
                        }
                        EditorArea area = editorAreaList.Find(x=>x.areaId == areaId);//去查找配置中是否有该Area
                        if(area == null)
                        {
                            area = new EditorArea();
                            editorAreaList.Add(area);
                        }
                        SaveGamingArea(map.m_GamingAreaList[iGaming], area);
                    }
                    outData.areaList = editorAreaList.ToArray();
                }
            }

            if(map.m_MissionList != null && map.m_MissionList.Count>0)
            {
                outData.sceneMissionReleaseId = map.m_MissionList.ToArray();
            }
        }

        /// <summary>
        /// 在GamingMap中添加泰坦信息
        /// </summary>
        /// <param name="mapData"></param>
        /// <param name="starMap"></param>
        private static bool SetTitanInfo(EditorGamingMap mapData, EditorStarMap starMap)
        {
            if (starMap == null)
            {
                return false;
            }
            EditorFixedStar[] fixedStars = starMap.fixedStars;
            if (fixedStars == null || fixedStars.Length <= 0)
            {
                return false;
            }

            for (int iStar = 0; iStar < fixedStars.Length; iStar++)
            {
                EditorFixedStar fixedStar = fixedStars[iStar];
                if (fixedStar == null)
                {
                    continue;
                }
                if (mapData.belongFixedStar == fixedStar.fixedStarId)
                {
                    mapData.ttGamingMapId = fixedStar.ttGamingMapId;
                    mapData.ttGamingAreaId = fixedStar.ttGamingAreaId;
                }
            }
            return true;
        }

        /// <summary>
        /// 保存GamingArea
        /// </summary>
        /// <param name="area"></param>
        /// <param name="outData"></param>
        private static void SaveGamingArea(GamingMapArea area, EditorArea outData)
        {
            outData.areaId = area.m_AreaId;
            outData.areaName = area.m_AreaName;
            outData.areaType = (int)area.m_AreaType;
            outData.fatherArea = area.m_FatherArea;
            outData.position = new EditorPosition(area.transform.position);
            if (area.m_ChildAreas != null)
            {
                outData.childrenAreaList = area.m_ChildAreas.ToArray();
            }
            outData.relieveCreature = area.m_RelieveCreatue;

            Creature[] creatureArray = area.GetCreature();
            if (creatureArray != null && creatureArray.Length > 0)
            {
                EditorCreature[] editorCreatureArray = new EditorCreature[creatureArray.Length];
                outData.creatureList = editorCreatureArray;
                for (int iCreature = 0; iCreature < creatureArray.Length; iCreature++)
                {
                    EditorCreature creature = new EditorCreature();
                    editorCreatureArray[iCreature] = creature;
                    SaveCreature(creatureArray[iCreature], creature);
                }
            }

            TeleportRoot teleportRoot = area.m_TeleportRoot;
            if (teleportRoot != null)
            {
                SaveTeleport(teleportRoot, outData);
            }


            Location[] locationArray = area.GetLocation();
            if (locationArray != null && locationArray.Length > 0)
            {
                EditorLocation[] editorLocationArray = new EditorLocation[locationArray.Length];
                outData.locationList = editorLocationArray;
                for (int iLocation = 0; iLocation < locationArray.Length; iLocation++)
                {
                    EditorLocation location = new EditorLocation();
                    editorLocationArray[iLocation] = location;
                    SaveLocation(locationArray[iLocation], location);
                }
            }

            LeapRoot leapRoot = area.m_LeapRoot;
            if (leapRoot != null)
            {
                SaveLeap(leapRoot, outData);
            }

            TreasureRoot treasureRoot = area.m_TreasureRoot;
            SaveTreasure(treasureRoot, outData);

            MineralRoot mineralRoot = area.m_MineralRoot;
            SaveMineral(mineralRoot, outData);

            Trigger[] triggerArray = area.GetTrigger();
            if (triggerArray != null && triggerArray.Length > 0)
            {
                EditorTrigger[] editorTriggerArray = new EditorTrigger[triggerArray.Length];
                outData.triggerList = editorTriggerArray;
                for (int iTrigger = 0; iTrigger < triggerArray.Length; iTrigger++)
                {
                    EditorTrigger trigger = new EditorTrigger();
                    editorTriggerArray[iTrigger] = trigger;
                    SaveTrigger(triggerArray[iTrigger], trigger);
                }
            }

            if(area.m_MissionList != null && area.m_MissionList.Count>0)
            {
                outData.sceneMissionReleaseId = area.m_MissionList.ToArray();
            }
        }

        private static void SaveTrigger(Trigger trigger, EditorTrigger editorTrigger)
        {
            editorTrigger.triggerIndex = trigger.m_Index;
            editorTrigger.triggerId = trigger.m_TriggerId;
            editorTrigger.triggerNpcId = trigger.m_NpcId;
            editorTrigger.position = trigger.GetEditorPosition();
            editorTrigger.autoCreation = trigger.GetAutoCreation();
            editorTrigger.reviveRange = trigger.m_Range;
        }

        /// <summary>
        ///保存采矿
        /// </summary>
        /// <param name="root"></param>
        /// <param name="outArea"></param>
        private static void SaveMineral(MineralRoot root,EditorArea outArea)
        {
            List<EditorMineral> mineralList = new List<EditorMineral>();
            if(root != null)
            {
                List<Mineral> minerals = root.m_MineralCache;
                if (minerals != null && minerals.Count > 0)
                {
                    for (int iMineral = 0; iMineral < minerals.Count; iMineral++)
                    {
                        Mineral mineral = minerals[iMineral];
                        if (mineral == null)
                        {
                            continue;
                        }
                        EditorMineral editorMineral = new EditorMineral();
                        editorMineral.mineralIndex = (uint)(mineralList.Count + 1);
                        editorMineral.mineralGroupId = mineral.m_MineralGroupId;
                        editorMineral.mineralNpcId = mineral.m_MineralNpcId;
                        editorMineral.name = mineral.name;
                        editorMineral.mineralPos = new EditorPosition(mineral.transform.position);
                        mineralList.Add(editorMineral);
                    }
                }
            }
            outArea.mineralList = mineralList.ToArray();
        }

        /// <summary>
        /// 保存寻宝
        /// </summary>
        /// <param name="root"></param>
        /// <param name="outArea"></param>
        private static void SaveTreasure(TreasureRoot root, EditorArea outArea)
        {
            List<EditorTreasure> treasureList = new List<EditorTreasure>();
            if(root != null)
            {
                List<Treasure> treasures = root.m_TreasureCache;
                if (treasures != null && treasures.Count > 0)
                {
                    for (int iTreasure = 0; iTreasure < treasures.Count; iTreasure++)
                    {
                        Treasure treasure = treasures[iTreasure];
                        if (treasure == null)
                        {
                            continue;
                        }
                        EditorTreasure editorTreasure = new EditorTreasure();
                        editorTreasure.treasureIndex = (uint)(treasureList.Count + 1);
                        editorTreasure.treasureGroupId = treasure.m_TreasureGroupId;
                        editorTreasure.treasureNpcId = treasure.m_TreasureNpcId;
                        editorTreasure.name = treasure.name;
                        editorTreasure.tresurePos = new EditorPosition(treasure.transform.position);
                        treasureList.Add(editorTreasure);
                    }
                }
            }
            outArea.treasureList = treasureList.ToArray();
        }

        /// <summary>
        /// 保存跃迁
        /// </summary>
        /// <param name="root"></param>
        /// <param name="outData"></param>
        private static void SaveLeap(LeapRoot root, EditorArea outData)
        {
            GamingMapType mapType = root.GetGamingMapType();
            if (mapType == GamingMapType.mapMainCity || mapType == GamingMapType.mapSpaceStation)
            {
                return;
            }
            EditorLeap editorLeap = new EditorLeap();
            outData.leapList = new EditorLeap[1];
            outData.leapList[0] = editorLeap;
            editorLeap.leapId = root.m_LeapId;
            editorLeap.leapName = root.m_LeapName;
            editorLeap.description = root.m_LeapDescription;
            //editorLeap.iconName = root.GetIconName();
            editorLeap.iconConfId = root.m_IconConfId;
            editorLeap.leapType = (int)root.m_LeapType;
            editorLeap.mainLeapId = root.m_MainLeapId;
            editorLeap.range = root.m_Range;
            editorLeap.autoVisible = root.m_AutoVisible ? 1 : 0;
            editorLeap.visibleLeapList = root.m_VisibleLeapList;
            editorLeap.position = new EditorPosition(root.transform.position);
        }
        /// <summary>
        /// 保存Creature
        /// </summary>
        /// <param name="creature"></param>
        /// <param name="outData"></param>
        private static void SaveCreature(Creature creature, EditorCreature outData)
        {
            outData.creatureId = creature.m_Uid;
            outData.tplId = creature.m_NpcId;
            outData.position = creature.GetEditorPosition();
            outData.rotation = creature.GetEditorRotation();
            outData.autoCreation = creature.GetAutoCreation();
            outData.teleportId = creature.GetBindTeleportId();
            outData.reviveRange = creature.m_ReviveRange;
        }

        private static void SaveTeleport(TeleportRoot teleportRoot, EditorArea outData)
        {
            List<TeleportVO> teleportList = teleportRoot.m_TeleportList;
            if (teleportList != null && teleportList.Count > 0)
            {
                EditorTeleport[] editorTeleportArray = new EditorTeleport[teleportList.Count];
                outData.teleportList = editorTeleportArray;
                for (int iTeleport = 0; iTeleport < teleportList.Count; iTeleport++)
                {
                    TeleportVO vo = teleportList[iTeleport];
                    EditorTeleport teleport = new EditorTeleport();
                    editorTeleportArray[iTeleport] = teleport;

                    teleport.teleportId = vo.ID;
                    teleport.creaureId = teleportRoot.m_GamingMapArea.GetBindTeleport(vo.ID);
                    if (vo.ChanelList != null && vo.ChanelList.Length > 0)
                    {
                        EditorChanel[] chanelList = new EditorChanel[vo.ChanelList.Length];
                        teleport.chanelList = chanelList;
                        for (int iChanel = 0; iChanel < vo.ChanelList.Length; iChanel++)
                        {
                            TeleportChanelVO chanelVo = ConfigVO<TeleportChanelVO>.Instance.GetData(vo.ChanelList[iChanel]);
                            if (chanelVo == null)
                            {
                                continue;
                            }
                            EditorChanel editorChanel = new EditorChanel();
                            chanelList[iChanel] = editorChanel;
                            editorChanel.chanelId = vo.ChanelList[iChanel];
                            TeleportRoot.TeleportInfo info = teleportRoot.GetTeleportInfo(editorChanel.chanelId);
                            if (info != null)
                            {
                                EditorLocation[] locations = GamingMap.GetEditorLocations(chanelVo.EndGamingMap, chanelVo.EndGamingMapArea);
                                if (locations != null && locations.Length > 0)
                                {
                                    List<EditorLocation> locationList = new List<EditorLocation>();
                                    for (int iLocation = 0; iLocation < locations.Length; iLocation++)
                                    {
                                        EditorLocation location = locations[iLocation];
                                        if (location.locationType == (int)LocationType.TeleportIn)
                                        {
                                            locationList.Add(location);
                                        }
                                    }
                                    if (locationList != null && info.m_SelectIndex >= 0 && locationList.Count > info.m_SelectIndex)
                                    {
                                        editorChanel.eLocation = locationList[info.m_SelectIndex].locationId;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 保存Location
        /// </summary>
        /// <param name="location"></param>
        /// <param name="outData"></param>
        private static void SaveLocation(Location location, EditorLocation outData)
        {
            outData.locationId = location.m_Uid;
            outData.locationName = location.m_LocationName;
            outData.locationType = (int)location.m_LocationType;
            outData.position = location.GetEditorPosition();
            outData.rotation = location.GetEditorRotation();
        }
        /// <summary>
        /// 保存到json
        /// </summary>
        /// <param name="map"></param>
        public static void SaveGamingMapToJson(GamingMap map,List<ulong> areaIds = null)
        {
            if(areaIds != null)
            {
                areaIds.Sort((x, y) =>
                {
                    return x.CompareTo(y);
                });
            }
            string filePath = "";
            GamingMapEditorSetting gamingSetting = GamingMapEditorUtility.GetGamingMapEditorSetting();
            if (gamingSetting != null)
            {
                filePath = MapEditorUtility.GetFullPath(gamingSetting.m_JsonPath);
            }
            if (!Directory.Exists(filePath))
            {
                EditorUtility.DisplayDialog("提示", "请先设置json保存目录", "确定");
                return;
            }
            filePath = string.Format("{0}/scene_{1}", filePath, map.m_Uid);
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            filePath = string.Format("{0}/map_data.json", filePath);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            EditorGamingMap outdata = new EditorGamingMap();
            //if (areaIds != null)
            //{
            //    outdata = LoadGamingMapFromJson(map.m_Uid,true);
            //}
            //else
            //{
            //    if (File.Exists(filePath))
            //    {
            //        File.Delete(filePath);
            //    }
            //    outdata = new EditorGamingMap();
            //}
            SaveGamingMap(map, outdata, areaIds);
            string jsondata = JsonUtility.ToJson(outdata, true);
            File.WriteAllText(filePath, jsondata, Encoding.UTF8);

            //TODO:设置泰坦mapid 区域id 单独再读取是因为星图和地图的数据都得拿 避免交叉
            EditorStarMap starMap = LoadStarMap();
            if (starMap != null)
            {
                EditorGamingMap gamingMap = LoadGamingMapFromJson(map.m_Uid,true);
                if (gamingMap != null)
                {
                    if (SetTitanInfo(gamingMap, starMap))
                    {
                        jsondata = JsonUtility.ToJson(gamingMap, true);
                        File.WriteAllText(filePath, jsondata, Encoding.UTF8);
                    }
                }

            }
            else
            {
                Debug.LogError("GamingMap中需要泰坦信息，请先导出星图数据");
            }

            MapEditorUtility.OpenFolder(filePath);
            AssetDatabase.Refresh();
        }


        /// <summary>
        /// 存取数据
        /// </summary>
        private static Dictionary<uint, EditorGamingMap> sm_EditorGamingMapData;

        public static void Clear()
        {
            sm_EditorGamingMapData = null;
        }
        /// <summary>
        /// 从json载入数据
        /// </summary>
        /// <param name="mapId"></param>
        /// <returns></returns>
        public static EditorGamingMap LoadGamingMapFromJson(uint mapId,bool force = false)
        {
            if (sm_EditorGamingMapData == null)
            {
                sm_EditorGamingMapData = new Dictionary<uint, EditorGamingMap>();
            }
            if (sm_EditorGamingMapData.ContainsKey(mapId) && !force)
            {
                return sm_EditorGamingMapData[mapId];
            }
            string filePath = "";
            GamingMapEditorSetting gamingSetting = GamingMapEditorUtility.GetGamingMapEditorSetting();
            if (gamingSetting != null)
            {
                filePath = MapEditorUtility.GetFullPath(gamingSetting.m_JsonPath);
            }
            filePath = string.Format("{0}/scene_{1}/map_data.json", filePath, mapId);

            UTF8Encoding utf8 = new UTF8Encoding(false);
            string jsonData = File.ReadAllText(filePath, utf8);
            EditorGamingMap gamingmap = JsonUtility.FromJson<EditorGamingMap>(jsonData);
            sm_EditorGamingMapData[mapId] = gamingmap;
            return gamingmap;
        }

        /// <summary>
        /// 加载所以深空地图数据
        /// </summary>
        /// <returns></returns>
        public static List<EditorGamingMap> LoadAllDeapSpaceMapJson()
        {
            string filePath = "";
            GamingMapEditorSetting gamingSetting = GamingMapEditorUtility.GetGamingMapEditorSetting();
            if (gamingSetting != null)
            {
                filePath = MapEditorUtility.GetFullPath(gamingSetting.m_JsonPath);
                filePath += "/";
            }
            List<EditorGamingMap> editorGamingList = new List<EditorGamingMap>();
            string[] folds = Directory.GetDirectories(filePath);
            if (folds != null && folds.Length > 0)
            {
                for (int iFold = 0; iFold < folds.Length; iFold++)
                {
                    string foldStr = folds[iFold];
                    foldStr += "/";
                    string[] files = Directory.GetFiles(foldStr);

                    if (files != null && files.Length > 0)
                    {
                        UTF8Encoding utf8 = new UTF8Encoding(false);
                        for (int iFile = 0; iFile < files.Length; iFile++)
                        {
                            string jsonFile = files[iFile];
                            if (jsonFile.EndsWith(".json"))
                            {
                                string jsonData = File.ReadAllText(jsonFile, utf8);

                                EditorGamingMap gamingmap = JsonUtility.FromJson<EditorGamingMap>(jsonData);
                                if (gamingmap.gamingType == (int)GamingMapType.mapDeepSpace)
                                {
                                    editorGamingList.Add(gamingmap);
                                }
                            }
                        }
                    }
                }
            }

            return editorGamingList;
        }

        /// <summary>
        /// 加载所有已生成的json文件
        /// </summary>
        /// <returns></returns>
        public static List<EditorGamingMap> LoadAllGamingMapJson()
        {
            string filePath = "";
            GamingMapEditorSetting gamingSetting = GamingMapEditorUtility.GetGamingMapEditorSetting();
            if (gamingSetting != null)
            {
                filePath = MapEditorUtility.GetFullPath(gamingSetting.m_JsonPath);
                filePath += "/";
            }
            List<EditorGamingMap> editorGamingList = new List<EditorGamingMap>();
            string[] folds = Directory.GetDirectories(filePath);
            if (folds != null && folds.Length > 0)
            {
                for (int iFold = 0; iFold < folds.Length; iFold++)
                {
                    string foldStr = folds[iFold];
                    foldStr += "/";
                    string[] files = Directory.GetFiles(foldStr);

                    if (files != null && files.Length > 0)
                    {
                        UTF8Encoding utf8 = new UTF8Encoding(false);
                        for (int iFile = 0; iFile < files.Length; iFile++)
                        {
                            string jsonFile = files[iFile];
                            if (jsonFile.EndsWith(".json"))
                            {
                                string jsonData = File.ReadAllText(jsonFile, utf8);

                                EditorGamingMap gamingmap = JsonUtility.FromJson<EditorGamingMap>(jsonData);
                                editorGamingList.Add(gamingmap);
                            }
                        }
                    }
                }
            }

            return editorGamingList;
        }
        #endregion

        #region 碰撞体相关

        /// <summary>
        /// 载入地图碰撞数据
        /// </summary>
        /// <param name="mapId"></param>
        /// <returns></returns>
        public static EditorCollider LoadMapCollider(uint mapId)
        {
            string filePath = "";
            MapEditorSetting mapSetting = MapEditorUtility.GetMapEditorSetting();
            if (mapSetting != null)
            {
                filePath = MapEditorUtility.GetFullPath(mapSetting.m_ColliderSavePath);
            }

            filePath = string.Format("{0}/map_{1}", filePath, mapId);

            filePath = string.Format("{0}/decorate.json", filePath);

            UTF8Encoding utf8 = new UTF8Encoding(false);
            string jsonData = File.ReadAllText(filePath, utf8);

            return JsonUtility.FromJson<EditorCollider>(jsonData);
        }

        public static void SaveColliderToJson(EditorCollider data)
        {
            string filePath = "";
            MapEditorSetting mapSetting = MapEditorUtility.GetMapEditorSetting();
            if (mapSetting != null)
            {
                filePath = MapEditorUtility.GetFullPath(mapSetting.m_ColliderSavePath);
            }

            if (!Directory.Exists(filePath))
            {
                EditorUtility.DisplayDialog("提示", "请先设置碰撞保存目录", "确定");
                return;
            }
            filePath = string.Format("{0}/map_{1}", filePath, data.mapID);
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            filePath = string.Format("{0}/decorate.json", filePath);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            string jsondata = JsonUtility.ToJson(data, true);
            File.WriteAllText(filePath, jsondata, Encoding.UTF8);
            MapEditorUtility.OpenFolder(filePath);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 返回json数据
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        public static EditorDecorate SaveColliderData(Collider collider, Quaternion rot, bool isUnit = false)
        {
            EditorDecorate editorDecorate = null;
            Vector3 colliderCenter = Vector3.zero;
            if (collider is BoxCollider)
            {
                editorDecorate = new EditorDecorate();
                editorDecorate.type = (int)COLLIDER_TYPE.PX_BOX;
                colliderCenter = (collider as BoxCollider).center;
            }
            else if (collider is SphereCollider)
            {
                editorDecorate = new EditorDecorate();
                editorDecorate.type = (int)COLLIDER_TYPE.PX_SPHERE;
                colliderCenter = (collider as SphereCollider).center;
            }
            else if (collider is CapsuleCollider)
            {
                editorDecorate = new EditorDecorate();
                editorDecorate.type = (int)COLLIDER_TYPE.PX_CAPSULE;
                colliderCenter = (collider as CapsuleCollider).center;
            }
            else
            {
                return editorDecorate;
            }
            editorDecorate.dir = new EditorRotation(rot);
            editorDecorate.dirMin = new EditorDecorateSize(collider.bounds.min + colliderCenter);
            editorDecorate.dirMax = new EditorDecorateSize(collider.bounds.max + colliderCenter);


            editorDecorate.path = GetParentName(collider.gameObject, isUnit);
            editorDecorate.scenePath = GetParentName(collider.gameObject);
            return editorDecorate;
        }

        public static string GetParentName(GameObject obj, bool isUnit = false)
        {
            if (obj == null)
            {
                return "";
            }
            Transform trans = obj.transform;
            StringBuilder sb = new StringBuilder();
            sb.Insert(0, obj.name);
            Transform parent = trans.parent;
            if (isUnit)
            {
                while (parent != null && !UnityEditor.PrefabUtility.IsOutermostPrefabInstanceRoot(parent.gameObject))
                {
                    trans = trans.parent;
                    parent = trans.parent;
                    sb.Insert(0, trans.gameObject.name + "/");
                }

                if (parent != null)
                {
                    GameObject prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(parent.gameObject);
                    string originAssetPath = AssetDatabase.GetAssetPath(prefab);
                    string assetMd5 = StringUtility.CalculateMD5Hash(AssetDatabase.AssetPathToGUID(originAssetPath) 
                        + StringUtility.CalculateMD5Hash(MapEditorUtility.GetPrefabOverrideModification(parent.gameObject)));
                    string addressableKey = string.Format(Constants.UNIT_PREFAB_ADDRESSKEY_FORMAT, assetMd5);

                    sb.Insert(0, string.Format("{0}/", addressableKey));
                }
            }
            else
            {
                while (parent != null)
                {
                    trans = trans.parent;
                    parent = trans.parent;
                    sb.Insert(0, trans.gameObject.name + "/");
                }
            }

            return sb.ToString();
        }

        public enum COLLIDER_TYPE
        {
            PX_BOX = 1,//boxcollider
            PX_SPHERE = 2,//spherecollider
            PX_CAPSULE = 3//capsualcollider
        }

        public enum ModelType
        {
            WarShip = 1,//战舰
            Arms = 2//武器
        }


        /// <summary>
        /// 导出空间寻路
        /// </summary>
        public static IEnumerator Export3DNavMesh(RootScenceRecast.ROOT_SCENCE_CUT_SIZE sceneType, Map map)
        {
            if (map == null)
            {
                yield return null;
            }

            RootScenceRecast[] recasts = UnityEngine.Object.FindObjectsOfType<RootScenceRecast>();
            if (recasts != null && recasts.Length > 0)
            {
                for (int iRecast = 0; iRecast < recasts.Length; iRecast++)
                {
                    GameObject.DestroyImmediate(recasts[iRecast].gameObject);
                }
            }
            GameObject recastObj = new GameObject("SceneRecastRoot");
            RootScenceRecast rootSceneRecast = recastObj.AddComponent<RootScenceRecast>();
            rootSceneRecast.rootScenceCutSize = sceneType;

            BuildRecastCube(recastObj.transform);
            yield return null;

            IEnumerator recastEnumerator =  ExportScene.RecastOctreeAsync();
            while(recastEnumerator.MoveNext())
            {
                yield return null;
            }
            ExportScene.SaveOctreeToOctreeFile(map.Uid);

            //GameObject.DestroyImmediate(recastObj);

            string[] savePath = ExportScene.GetOctreeSavePath(map.Uid);
            if (savePath != null && savePath.Length > 0)
            {
                for (int iSave = 0; iSave < savePath.Length; iSave++)
                {
                    MapEditorUtility.OpenFolder(savePath[iSave]);
                }
            }

        }

        private static void BuildRecastCube(Transform parent)
        {
            RecastRegionInfo[] regionInfos = UnityEngine.Object.FindObjectsOfType<RecastRegionInfo>();
            if (regionInfos == null || regionInfos.Length <= 0)
            {
                return;
            }

            for (int iRegion = 0; iRegion < regionInfos.Length; iRegion++)
            {
                RecastRegionInfo regionInfo = regionInfos[iRegion];
                if (regionInfo == null || regionInfo.enabled == false)
                {
                    continue;
                }
                RecastRegionInfo.CreateRecastCube(regionInfo, parent);
            }
        }

        private static void FindRecastRegionInfos(AreaSpawner areaSpawner, Transform parentTrans)
        {
            Area area = areaSpawner.m_Area;
            if (area == null)
            {
                return;
            }

            RecastRegionInfo[] regionInfos = area.FindRegionInfos();
            if (regionInfos != null && regionInfos.Length > 0)
            {
                for (int iRegion = 0; iRegion < regionInfos.Length; iRegion++)
                {
                    RecastRegionInfo.CreateRecastCube(regionInfos[iRegion], parentTrans);
                }
            }
        }

        public static void CorrectCollider(Collider collider)
        {
            MeshRenderer[] meshRenders = collider.GetComponents<MeshRenderer>();
            if (meshRenders == null || meshRenders.Length <= 0)
            {
                if (collider is BoxCollider)
                {
                    BoxCollider boxCollider = collider as BoxCollider;
                    if(boxCollider.center != Vector3.zero)
                    {
                        Vector3 originPos = collider.transform.position;
                        Vector3 originScale = collider.transform.lossyScale;
                        Quaternion originRotation = collider.transform.rotation;
                        Vector3 colliderBoundCentet = boxCollider.bounds.center;
                        Matrix4x4 matrix = new Matrix4x4();
                        matrix.SetTRS(originPos, originRotation, originScale);
                        originPos = matrix.MultiplyPoint3x4(colliderBoundCentet);
                        originPos = collider.transform.InverseTransformPoint(originPos);
                        boxCollider.center = Vector3.zero;
                        collider.transform.position = originPos;
                    }
                }
                else if (collider is CapsuleCollider)
                {
                    CapsuleCollider capCollider = collider as CapsuleCollider;
                    CorrectCapsule(capCollider);
                }
            }
        }

        /// <summary>
        /// 矫正胶囊碰撞体
        /// </summary>
        /// <param name="capsule"></param>
        private static void CorrectCapsule(CapsuleCollider capsule)
        {
            MeshRenderer[] meshRenders = capsule.GetComponents<MeshRenderer>();
            if (meshRenders == null || meshRenders.Length <= 0)
            {
                Vector3 originScale = capsule.transform.localScale;
                if (capsule.direction != 0)//得把所有的胶囊碰撞体改成方向为x轴的
                {
                    Vector3 angle = capsule.transform.localEulerAngles;
                    if (capsule.direction == 1)
                    {
                        capsule.transform.Rotate(Vector3.forward, 90);
                        capsule.transform.localScale = new Vector3(originScale.y, originScale.x, originScale.z);
                    }
                    else if (capsule.direction == 2)
                    {
                        capsule.transform.Rotate(Vector3.up, 90);
                        capsule.transform.localScale = new Vector3(originScale.z, originScale.y, originScale.x);
                    }
                    capsule.direction = 0;
                }
                if (capsule.center != Vector3.zero)
                {
                    capsule.transform.localPosition += new Vector3(capsule.center.x * originScale.x, capsule.center.y * originScale.y, capsule.center.z * originScale.z);
                    capsule.center = Vector3.zero;
                }
            }
        }
        
        public static void CorrectAreaColliderCenter(Area area)
        {
            if (area == null)
            {
                return;
            }
            List<GameObject> prefabList = new List<GameObject>();
            MapEditorUtility.GetAllPrefab(area.transform, prefabList);
            GameObject[] units = prefabList.ToArray();
            if (units != null && units.Length > 0)
            {
                for (int iUnit = 0; iUnit < units.Length; iUnit++)
                {
                    GameObject unit = units[iUnit];
                    List<Transform> colliderRoots = MapEditorUtility.FindChilds<Transform>(unit.transform,"Collider");
                    if(colliderRoots == null || colliderRoots.Count<=0)
                    {
                        continue;
                    }
                    for(int iRoot =0;iRoot<colliderRoots.Count;iRoot++)
                    {
                        Transform colliderRoot = colliderRoots[iRoot];
                        if (colliderRoot != null)
                        {
                            int childCount = colliderRoot.transform.childCount;
                            if (childCount > 0)
                            {
                                for (int iChild = 0; iChild < childCount; iChild++)
                                {
                                    GameObject colliderObj = colliderRoot.transform.GetChild(iChild).gameObject;
                                    Collider[] collider = colliderObj.GetComponents<Collider>();
                                    if (collider != null && collider.Length == 1)
                                    {
                                        CorrectCollider(collider[0]);
                                    }
                                }
                            }
                        }
                    }
                    
                }
            }
        }

        /// <summary>
		/// 计算船的碰撞导出给服务器
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static CapsuleCollider CalculateCapsuleCollider(GameObject obj)
        {
            CapsuleCollider capsule = obj.GetComponentInChildren<CapsuleCollider>();
            if (capsule != null)
            {
                return capsule;
            }

            Vector3 oldPos = obj.transform.position;
            Quaternion oldRotation = obj.transform.rotation;
            Vector3 oldScale = obj.transform.localScale;

            obj.transform.position = Vector3.zero;
            obj.transform.rotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;

            MeshRenderer[] meshs = obj.GetComponentsInChildren<MeshRenderer>(true);
            Vector3 min = float.MaxValue * Vector3.one;
            Vector3 max = -1 * float.MaxValue * Vector3.one;

            for (int i = 0; i < meshs.Length; ++i)
            {
                if (meshs[i].bounds != null)
                {
                    min.x = Mathf.Min(min.x, meshs[i].bounds.min.x);
                    min.y = Mathf.Min(min.y, meshs[i].bounds.min.y);
                    min.z = Mathf.Min(min.z, meshs[i].bounds.min.z);
                    max.x = Mathf.Max(max.x, meshs[i].bounds.max.x);
                    max.y = Mathf.Max(max.y, meshs[i].bounds.max.y);
                    max.z = Mathf.Max(max.z, meshs[i].bounds.max.z);
                }
            }

            Vector3 center = new Vector3(max.x + min.x, max.y + min.y, max.z + min.z) * 0.5f;
            Vector3 size = new Vector3(max.x - min.x, max.y - min.y, max.z - min.z);

            GameObject colliderObject = new GameObject();
            colliderObject.transform.parent = obj.transform;
            colliderObject.transform.localPosition = Vector3.zero;
            colliderObject.transform.localRotation = Quaternion.identity;
            colliderObject.transform.localScale = Vector3.one;
            capsule = colliderObject.AddComponent<CapsuleCollider>();

            capsule.center = center;
            float len = Mathf.Max(size.x, size.y, size.z);
            capsule.height = len;
            if (len == size.x)
            {
                capsule.direction = 0;
                capsule.radius = Mathf.Max(size.y, size.z) * 0.5f;
            }
            else if (len == size.y)
            {
                capsule.direction = 1;
                capsule.radius = Mathf.Max(size.x, size.z) * 0.5f;
            }
            else if (len == size.z)
            {
                capsule.direction = 2;
                capsule.radius = Mathf.Max(size.x, size.y) * 0.5f;
            }


            obj.transform.position = oldPos;
            obj.transform.rotation = oldRotation;
            obj.transform.localScale = oldScale;

            return capsule;
        }

        [MenuItem("Custom/预览生成的碰撞体")]
        public static void PreviewCollider()
        {
            if (Selection.activeGameObject == null)
            {
                EditorUtility.DisplayDialog("提示", "请先选中带render的物体", "确定");
                return;
            }
            GameObject obj = GameObject.Instantiate(Selection.activeGameObject);
            CapsuleCollider colliderPreview = CalculateCapsuleCollider(obj);
            CorrectCollider(colliderPreview);
            Collider[] colliders = obj.GetComponentsInChildren<Collider>();
            if (colliders != null && colliders.Length > 0)
            {
                for (int iCollider = 0; iCollider < colliders.Length; iCollider++)
                {
                    Collider collider = colliders[iCollider];
                    if (collider != colliderPreview)
                    {
                        collider.enabled = false;
                    }
                }
            }
        }
#endregion

#region 国际化excel
        [MenuItem("Custom/Map/导出language_mapeditor.csv")]
        private static void ExportLanguageMapEditor()
        {
#region 读取excel表中的数据
            if (EditorGamingMapData.m_VoNameDic == null)
            {
                EditorGamingMapData.m_VoNameDic = new Dictionary<string, List<string>>();
            }
            if (!EditorGamingMapData.m_VoNameDic.ContainsKey(typeof(LanguageMapEditorVO).Name))
            {
                List<string> languageList = new List<string>();
                languageList.Add("language_mapeditor.csv");
                languageList.Add("language_mapeditor");
                EditorGamingMapData.m_VoNameDic.Add(typeof(LanguageMapEditorVO).Name, languageList);
            }
            ConfigVO<LanguageMapEditorVO>.Instance.Clear();
            List<LanguageMapEditorVO> languageVoList = ConfigVO<LanguageMapEditorVO>.Instance.GetList();

            List<LanguageMapEditorVO> gamingMapNameList = new List<LanguageMapEditorVO>();//"gamingmap_name_"+gamingmap_id
            List<LanguageMapEditorVO> areaNameList = new List<LanguageMapEditorVO>();//"area_name_"+area_id
            List<LanguageMapEditorVO> leapNameList = new List<LanguageMapEditorVO>();//"leap_name_"+leap_id
            List<LanguageMapEditorVO> leapDescriptionList = new List<LanguageMapEditorVO>();//"description_leap_name_"+leap_id


            if (languageVoList != null && languageVoList.Count > 0)
            {
                for (int iLanguage = 0; iLanguage < languageVoList.Count; iLanguage++)
                {
                    LanguageMapEditorVO languageVo = languageVoList[iLanguage];
                    string key = languageVo.key;
                    if (key.Contains("description_leap_name_"))
                    {
                        leapDescriptionList.Add(languageVo);
                    }
                    else if (key.Contains("leap_name_"))
                    {
                        leapNameList.Add(languageVo);
                    }
                    else if (key.Contains("area_name_"))
                    {
                        areaNameList.Add(languageVo);
                    }
                    else if (key.Contains("gamingmap_name_"))
                    {
                        gamingMapNameList.Add(languageVo);
                    }
                }
            }
#endregion

#region 修改vo数据
            List<EditorGamingMap> gamingMapList = LoadAllGamingMapJson();
            if (gamingMapList != null && gamingMapList.Count > 0)
            {
                for (int iGaming = 0; iGaming < gamingMapList.Count; iGaming++)
                {
                    EditorGamingMap gamingMap = gamingMapList[iGaming];
                    string gamingMapName = string.Format("gamingmap_name_{0}", gamingMap.gamingmapId);
                    LanguageMapEditorVO languageVo = ConfigVO<LanguageMapEditorVO>.Instance.GetData(gamingMapName);
                    if (languageVo != null)
                    {
                        languageVo.chs = gamingMap.gamingmapName;
                        Debug.LogError("ens:" + languageVo.enUs);
                    }
                    else
                    {
                        languageVo = new LanguageMapEditorVO();
                        languageVo.key = gamingMapName;
                        languageVo.strID = gamingMapName;
                        languageVo.chs = gamingMap.gamingmapName;
                        gamingMapNameList.Add(languageVo);
                    }

                    EditorArea[] areaList = gamingMap.areaList;
                    if (areaList != null && areaList.Length > 0)
                    {
                        for (int iArea = 0; iArea < areaList.Length; iArea++)
                        {
                            EditorArea editorArea = areaList[iArea];
                            string areaName = string.Format("area_name_{0}_{1}", gamingMap.gamingmapId, editorArea.areaId);
                            AddLanguageVo(areaName, editorArea.areaName, areaNameList);
                            EditorLeap[] leapList = editorArea.leapList;
                            if (leapList != null && leapList.Length > 0)
                            {
                                for (int iLeap = 0; iLeap < leapList.Length; iLeap++)
                                {
                                    EditorLeap editorLeap = leapList[iLeap];
                                    string leapName = string.Format("leap_name_{0}_{1}", gamingMap.gamingmapId, editorLeap.leapId);
                                    string leapDes = string.Format("description_leap_name_{0}_{1}", gamingMap.gamingmapId, editorLeap.leapId);
                                    AddLanguageVo(leapName, editorLeap.leapName, leapNameList);
                                    AddLanguageVo(leapDes, editorLeap.description, leapDescriptionList);
                                }
                            }
                        }
                    }
                }
            }
#endregion

#region 写入excel
            List<LanguageMapEditorVO> totalList = new List<LanguageMapEditorVO>();
            AddLanguageVo(gamingMapNameList, totalList);
            AddLanguageVo(areaNameList, totalList);
            AddLanguageVo(leapNameList, totalList);
            AddLanguageVo(leapDescriptionList, totalList);

            ConfigVO<LanguageMapEditorVO>.Instance.Set(totalList, true);
            ConfigVO<LanguageMapEditorVO>.Instance.SaveCSV();

            string path = EditorConfigData.GetExcelFilePath("language_mapeditor.csv");
            MapEditorUtility.OpenFolder(path);
#endregion


        }

        private static void AddLanguageVo(string key, string chs, List<LanguageMapEditorVO> voList)
        {
            LanguageMapEditorVO areaLanguageVo = ConfigVO<LanguageMapEditorVO>.Instance.GetData(key);
            if (areaLanguageVo != null)
            {
                areaLanguageVo.chs = chs;
            }
            else
            {
                areaLanguageVo = new LanguageMapEditorVO();
                areaLanguageVo.key = key;
                areaLanguageVo.strID = key;
                areaLanguageVo.chs = chs;
                voList.Add(areaLanguageVo);
            }
        }

        private static void AddLanguageVo(List<LanguageMapEditorVO> voList, List<LanguageMapEditorVO> totalList)
        {
            if (voList.Count > 0)
            {
                for (int iVo = 0; iVo < voList.Count; iVo++)
                {
                    
                    totalList.Add(voList[iVo]);
                }
            }
        }
#endregion

#region 分层碰撞
        private static List<string> sm_IncludeLayers = new List<string>() { "MainPlayer", "SkillProjectile", "HumanNPC", "SpacecraftNPC", "SpacecraftOtherPlayer",
            "SceneOnly","HumanOtherPlayer","UnselectableSpacecraft"
        };

        private static void SetLayerCollisionProperty(object instance, string propertyName, object value)
        {
            if (instance == null || string.IsNullOrEmpty(propertyName))
            {
                return;
            }
            Type m_Type = instance.GetType();
            PropertyInfo propertyInfo = m_Type.GetProperty(propertyName);
            if (propertyInfo != null)
            {
                propertyInfo.SetValue(instance, value, null);
            }
        }

        [MenuItem("Custom/导出分层碰撞信息")]
        private static void Export()
        {
            if (m_VoNameDic == null)
            {
                m_VoNameDic = new Dictionary<string, List<string>>();
            }
            m_VoNameDic.Clear();
            if (!m_VoNameDic.ContainsKey(typeof(LayerCollisionVO).Name))
            {
                List<string> layerInfo = new List<string>();
                layerInfo.Add("layer_collision.csv");
                layerInfo.Add("layer_collision");
                m_VoNameDic.Add(typeof(LayerCollisionVO).Name, layerInfo);
            }

            ConfigVO<LayerCollisionVO>.Instance.Clear();
            ConfigVO<LayerCollisionVO>.Instance.GetList();
            List<LayerCollisionVO> voList = new List<LayerCollisionVO>();
            string[] layers = UnityEditorInternal.InternalEditorUtility.layers;
            if (layers != null && layers.Length > 0)
            {
                for (int iLayer = 0; iLayer < layers.Length; iLayer++)
                {
                    string name1 = layers[iLayer];
                    if (!sm_IncludeLayers.Contains(name1))
                    {
                        continue;
                    }
                    int layer1 = LayerMask.NameToLayer(name1);
                    LayerCollisionVO collisionVo = new LayerCollisionVO();
                    voList.Add(collisionVo);
                    collisionVo.ID = layer1;
                    collisionVo.LayerId = layer1;
                    collisionVo.Name = name1;
                    for (int iLayerTmp = 0; iLayerTmp < layers.Length; iLayerTmp++)
                    {
                        string name2 = layers[iLayerTmp];
                        if (!sm_IncludeLayers.Contains(name2))
                        {
                            continue;
                        }

                        int layer2 = LayerMask.NameToLayer(name2);
                        bool ignore = Physics.GetIgnoreLayerCollision(layer1, layer2);
                        int canCollision = 0;
                        if (!ignore)
                        {
                            canCollision = 1;
                        }
                        SetLayerCollisionProperty(collisionVo, name2, canCollision);
                    }

                }
            }

            ConfigVO<LayerCollisionVO>.Instance.Set(voList);
            ConfigVO<LayerCollisionVO>.Instance.SaveCSV();
        }
#endregion

#region StarMap相关
        /// <summary>
        /// 保存到json
        /// </summary>
        /// <param name="map"></param>
        public static void SaveStarMapToJson(EditorStarMap starMapData)
        {
            string filePath = "";
            GamingMapEditorSetting gamingSetting = GamingMapEditorUtility.GetGamingMapEditorSetting();
            if (gamingSetting != null)
            {
                filePath = MapEditorUtility.GetFullPath(gamingSetting.m_StarMapPath);
            }
            if (!Directory.Exists(filePath))
            {
                EditorUtility.DisplayDialog("提示", "请先设置json保存目录", "确定");
                return;
            }
            filePath = string.Format("{0}/starmap_data.json", filePath);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            string jsondata = JsonUtility.ToJson(starMapData, true);
            File.WriteAllText(filePath, jsondata, Encoding.UTF8);
            MapEditorUtility.OpenFolder(filePath);
            AssetDatabase.Refresh();
        }

        public static EditorStarMap LoadStarMap()
        {
            string filePath = "";
            GamingMapEditorSetting gamingSetting = GamingMapEditorUtility.GetGamingMapEditorSetting();
            if (gamingSetting != null)
            {
                filePath = MapEditorUtility.GetFullPath(gamingSetting.m_StarMapPath);
            }
            if (!Directory.Exists(filePath))
            {
                EditorUtility.DisplayDialog("提示", "请先设置json保存目录", "确定");
                return null;
            }
            filePath = string.Format("{0}/starmap_data.json", filePath);
            if (!File.Exists(filePath))
            {
                Debug.LogError("不存在StarMap数据");
                return null;
            }
            UTF8Encoding utf8 = new UTF8Encoding(false);
            string jsonData = File.ReadAllText(filePath, utf8);
            EditorStarMap starMap = JsonUtility.FromJson<EditorStarMap>(jsonData);
            return starMap;
        }

        public static void LoadStarMapVO()
        {
            if (m_VoNameDic == null)
            {
                m_VoNameDic = new Dictionary<string, List<string>>();
            }
            m_VoNameDic.Clear();
            if (!m_VoNameDic.ContainsKey(typeof(StarMapVO).Name))
            {
                List<string> starInfo = new List<string>();
                starInfo.Add("starmap.csv");
                starInfo.Add("starmap");
                m_VoNameDic.Add(typeof(StarMapVO).Name, starInfo);
            }

            ConfigVO<StarMapVO>.Instance.Clear();
            ConfigVO<StarMapVO>.Instance.GetList();
        }
#endregion

#region 战舰属性相关
        [MenuItem("Custom/Map/导出effect_speed.csv")]
        private static void ExportSpacecraftEffect()
        {
            InitSpacecraftLanguage();
            List<EditorSpacecraft> allSpacecraft = LoadAllSpacecraft();
            if (allSpacecraft != null && allSpacecraft.Count > 0)
            {
                if (m_VoNameDic == null)
                {
                    m_VoNameDic = new Dictionary<string, List<string>>();
                }
                if (!CanExportSpacecraft(allSpacecraft))
                {
                    EditorUtility.DisplayDialog("提示", "导出失败，详情看log", "确定");
                    return;
                }
                if (!EditorGamingMapData.m_VoNameDic.ContainsKey(typeof(EffectSpeedVO).Name))
                {
                    List<string> modelInfo = new List<string>();
                    modelInfo.Add("effect_speed.csv");
                    modelInfo.Add("effect_speed");
                    EditorGamingMapData.m_VoNameDic.Add(typeof(EffectSpeedVO).Name, modelInfo);
                }
                ConfigVO<EffectSpeedVO>.Instance.Clear();
                ConfigVO<EffectSpeedVO>.Instance.GetList();
                List<EffectSpeedVO> speedList = new List<EffectSpeedVO>();
                for (int iSpacecraft = 0; iSpacecraft < allSpacecraft.Count; iSpacecraft++)
                {
                    EditorSpacecraft spaceCraft = allSpacecraft[iSpacecraft];
                    EditorSpacecraftMode[] spacecraftModes = spaceCraft.spacecraft_modes;
                    if (spacecraftModes != null && spacecraftModes.Length > 0)
                    {
                        for (int iSpace = 0; iSpace < spacecraftModes.Length; iSpace++)
                        {
                            EditorSpacecraftMode spacecraftMode = spacecraftModes[iSpace];
                            List<EffectSpeedVO> speedVo = CreateSpeedVO(spaceCraft.spacecraft_id, spaceCraft.modelName, spacecraftMode);
                            if (speedVo != null)
                            {
                                speedList.AddRange(speedVo);
                            }
                        }
                    }
                }
                ConfigVO<EffectSpeedVO>.Instance.SetWithRepeatId(speedList);
                ConfigVO<EffectSpeedVO>.Instance.SaveCSV();
                MapEditorUtility.OpenFolder(EditorConfigData.GetExcelFilePath(""));
            }
        }

        private static bool CanExportSpacecraft(List<EditorSpacecraft> spaceCrafts)
        {

            if (spaceCrafts == null || spaceCrafts.Count <= 0)
            {
                Debug.LogError("未找到json数据");
                return false;
            }
            bool canExport = true;
            for (int iSpace = 0; iSpace < spaceCrafts.Count; iSpace++)
            {
                EditorSpacecraft spaceCraft = spaceCrafts[iSpace];
                if (spaceCraft.spacecraft_modes == null || spaceCraft.spacecraft_modes.Length < 3)
                {
                    Debug.LogError("数据不全，导出失败:" + spaceCraft.spacecraft_id);
                    canExport = false;
                }
            }
            return canExport;
        }

        private static Dictionary<string, Dictionary<string, List<string>>> sm_LanguageSpacecraft = new Dictionary<string, Dictionary<string, List<string>>>();
        private static void InitSpacecraftLanguage()
        {
            if (sm_LanguageSpacecraft != null && sm_LanguageSpacecraft.Count > 0)
            {
                // return;
                sm_LanguageSpacecraft.Clear();
            }

            InitMoveableLanguage();
            InitTurnableLanguage();
            InitMimicryLanguage();
            InitTransitionLanguage();
        }

        private static void InitTransitionLanguage()
        {
            Dictionary<string, List<string>> moveableDir = new Dictionary<string, List<string>>();
            Type varType = typeof(EditorSpacecraftTransition);
            FieldInfo[] fileds = varType.GetFields();
            if (fileds != null && fileds.Length > 0)
            {

                for (int iFiled = 0; iFiled < fileds.Length; iFiled++)
                {
                    List<string> strList = new List<string>();
                    FieldInfo fieldInfo = fileds[iFiled];
                    Type childType = fieldInfo.FieldType;
                    if (childType == typeof(float))
                    {
                        if (fieldInfo.Name.Equals("originspeed"))
                        {
                            strList.Add("跃迁起始速度");
                            moveableDir.Add("originspeed", strList);
                        }
                        else if (fieldInfo.Name.Equals("speed"))
                        {
                            strList.Add("跃迁加速度");
                            moveableDir.Add("speed", strList);
                        }
                        else if (fieldInfo.Name.Equals("despeed"))
                        {
                            strList.Add("跃迁减速度");
                            moveableDir.Add("despeed", strList);
                        }
                    }
                }
            }
            sm_LanguageSpacecraft.Add(varType.Name, moveableDir);
        }

        private static void InitMimicryLanguage()
        {
            Dictionary<string, List<string>> moveableDir = new Dictionary<string, List<string>>();
            Type varType = typeof(EditorSpacecraftMimicry);
            FieldInfo[] fileds = varType.GetFields();
            if (fileds != null && fileds.Length > 0)
            {

                for (int iFiled = 0; iFiled < fileds.Length; iFiled++)
                {
                    List<string> strList = new List<string>();
                    FieldInfo fieldInfo = fileds[iFiled];
                    Type childType = fieldInfo.FieldType;
                    if (childType == typeof(float))
                    {
                        if (fieldInfo.Name.Equals("turn_maxangle"))
                        {
                            strList.Add("拟态X轴最大角度");
                            moveableDir.Add("turn_maxangle", strList);
                        }
                        else if (fieldInfo.Name.Equals("turn_angelespeed"))
                        {
                            strList.Add("拟态X轴最大角加速度");
                            moveableDir.Add("turn_angelespeed", strList);
                        }
                        else if (fieldInfo.Name.Equals("ver_maxangle"))
                        {
                            strList.Add("拟态Z轴最大角度");
                            moveableDir.Add("ver_maxangle", strList);
                        }
                        else if (fieldInfo.Name.Equals("ver_anglespeed"))
                        {
                            strList.Add("拟态Z轴最大角加速度");
                            moveableDir.Add("ver_anglespeed", strList);
                        }
                    }
                }
            }
            sm_LanguageSpacecraft.Add(varType.Name, moveableDir);
        }

        private static void InitTurnableLanguage()
        {
            Dictionary<string, List<string>> moveableDir = new Dictionary<string, List<string>>();
            Type varType = typeof(EditorSpacecraftTurnable);
            FieldInfo[] fileds = varType.GetFields();
            if (fileds != null && fileds.Length > 0)
            {

                for (int iFiled = 0; iFiled < fileds.Length; iFiled++)
                {
                    List<string> strList = new List<string>();
                    FieldInfo fieldInfo = fileds[iFiled];
                    Type childType = fieldInfo.FieldType;
                    if (childType == typeof(Vector3))
                    {
                        if (fieldInfo.Name.Equals("max_turnangle"))
                        {
                            strList.Add("X轴最大角速度");
                            strList.Add("Y轴最大角速度");
                            strList.Add("Z轴最大角速度");
                            moveableDir.Add("max_turnangle", strList);
                        }
                        else if (fieldInfo.Name.Equals("turn_speed"))
                        {
                            strList.Add("X轴角加速度");
                            strList.Add("Y轴角加速度");
                            strList.Add("Z轴角加速度");
                            moveableDir.Add("turn_speed", strList);
                        }
                        else if (fieldInfo.Name.Equals("turn_despeed"))
                        {
                            strList.Add("X轴阻力角减速度");
                            strList.Add("Y轴阻力角减速度");
                            strList.Add("Z轴阻力角减速度");
                            moveableDir.Add("turn_despeed", strList);
                        }
                    }
                }
            }
            sm_LanguageSpacecraft.Add(varType.Name, moveableDir);
        }

        private static void InitMoveableLanguage()
        {
            Dictionary<string, List<string>> moveableDir = new Dictionary<string, List<string>>();
            Type varType = typeof(EditorSpacecraftMoveable);
            FieldInfo[] fileds = varType.GetFields();
            if (fileds != null && fileds.Length > 0)
            {
                for (int iFiled = 0; iFiled < fileds.Length; iFiled++)
                {
                    List<string> strList = new List<string>();
                    FieldInfo fieldInfo = fileds[iFiled];
                    Type childType = fieldInfo.FieldType;
                    if (childType == typeof(Vector2))
                    {
                        if (fieldInfo.Name.Equals("max_verspeed"))
                        {
                            strList.Add("向上最大线速度");
                            strList.Add("向下最大线速度");
                            moveableDir.Add("max_verspeed", strList);
                        }
                        else if (fieldInfo.Name.Equals("hor_movespeed"))
                        {
                            strList.Add("向右线加速度");
                            strList.Add("向左线加速度");
                            moveableDir.Add("hor_movespeed", strList);
                        }
                        else if (fieldInfo.Name.Equals("ver_movespeed"))
                        {
                            strList.Add("向上线加速度");
                            strList.Add("向下线加速度");
                            moveableDir.Add("ver_movespeed", strList);
                        }
                        else if (fieldInfo.Name.Equals("hor_despeed"))
                        {
                            strList.Add("前后阻力减速度");
                            strList.Add("左右阻力减速度");
                            moveableDir.Add("hor_despeed", strList);
                        }
                    }
                    else if (childType == typeof(Vector4))
                    {
                        if (fieldInfo.Name.Equals("max_horspeed"))
                        {
                            strList.Add("向前最大线速度");
                            strList.Add("向后最大线速度");
                            strList.Add("向右最大线速度");
                            strList.Add("向左最大线速度");
                            moveableDir.Add("max_horspeed", strList);
                        }
                    }
                    else if (childType == typeof(float))
                    {
                        if (fieldInfo.Name.Equals("hor_forspeed"))
                        {
                            strList.Add("向前线加速度");
                            moveableDir.Add("hor_forspeed", strList);
                        }
                        else if (fieldInfo.Name.Equals("hor_backspeed"))
                        {
                            strList.Add("向后线加速度");
                            moveableDir.Add("hor_backspeed", strList);
                        }
                        else if (fieldInfo.Name.Equals("ver_despeed"))
                        {
                            strList.Add("上下阻力减速度");
                            moveableDir.Add("ver_despeed", strList);
                        }
                    }
                }
            }
            sm_LanguageSpacecraft.Add(varType.Name, moveableDir);
        }

        private static List<EffectSpeedVO> CreateMoveAble<T>(int spaceCraftId, string modelName, string preStr, T moveable)
        {
            if (moveable == null)
            {
                return null;
            }
            List<EffectSpeedVO> speedList = new List<EffectSpeedVO>();
            FieldInfo[] fileds = typeof(T).GetFields();
            Dictionary<string, List<string>> spacecraftStr = null;
            sm_LanguageSpacecraft.TryGetValue(typeof(T).Name, out spacecraftStr);
            if (fileds != null && fileds.Length > 0)
            {
                FieldInfo[] childInfos = null;
                for (int iFiled = 0; iFiled < fileds.Length; iFiled++)
                {
                    Type childType = fileds[iFiled].FieldType;
                    object value = fileds[iFiled].GetValue(moveable);
                    string attribute = preStr;
                    if (childType == typeof(float))
                    {
                        if (spacecraftStr != null && spacecraftStr.ContainsKey(fileds[iFiled].Name))
                        {
                            attribute += spacecraftStr[fileds[iFiled].Name][0];
                        }
                        speedList.Add(CreateSpeedVO(spaceCraftId, modelName, (float)value, attribute));
                    }
                    else
                    {
                        childInfos = childType.GetFields();

                        if (childInfos != null && childInfos.Length > 0)
                        {
                            int childIndex = 0;
                            for (int iChild = 0; iChild < childInfos.Length; iChild++)
                            {
                                if (!childInfos[iChild].Name.Equals("x") && !childInfos[iChild].Name.Equals("y") && !childInfos[iChild].Name.Equals("z")
                                    && !childInfos[iChild].Name.Equals("w"))
                                {
                                    continue;
                                }
                                attribute = preStr;
                                if (spacecraftStr != null && spacecraftStr.ContainsKey(fileds[iFiled].Name))
                                {
                                    attribute += spacecraftStr[fileds[iFiled].Name][childIndex];
                                    childIndex++;
                                }
                                object childValue = childInfos[iChild].GetValue(value);
                                speedList.Add(CreateSpeedVO(spaceCraftId, modelName, (float)childValue, attribute));
                            }
                        }
                        else
                        {

                            if (spacecraftStr != null && spacecraftStr.ContainsKey(fileds[iFiled].Name))
                            {
                                attribute += spacecraftStr[fileds[iFiled].Name][0];
                            }
                            speedList.Add(CreateSpeedVO(spaceCraftId, modelName, (float)value, attribute));
                        }
                    }

                }
            }
            return speedList;
        }

        private static EffectSpeedVO CreateSpeedVO(int id, string modelName, float value, string attribute)
        {
            EffectSpeedVO speedVo = new EffectSpeedVO();
            speedVo.effectId = id;
            speedVo.ID = id;
            speedVo.model = modelName;
            speedVo.value = (float)value;
            speedVo.pipeLv = 1;
            speedVo.cnattribute = "";
            speedVo.attribute = attribute;
            speedVo.function = "固定值增加";
            speedVo.remarks = "";
            return speedVo;
        }

        private static List<EffectSpeedVO> CreateSpeedVO(int spaceCraftId, string modelName, EditorSpacecraftMode spacecraftMode)
        {
            List<EffectSpeedVO> speedVoList = new List<EffectSpeedVO>();
            string preStr = "";
            switch ((SpacecraftSpeedMode)spacecraftMode.speed_mode)
            {
                case SpacecraftSpeedMode.BattleMode:
                    preStr = "战斗";
                    break;
                case SpacecraftSpeedMode.CruiseMode:
                    break;
                case SpacecraftSpeedMode.OverloadMode:
                    preStr = "过载";
                    break;
            }
            EditorSpacecraftMoveable moveable = spacecraftMode.movebale;
            if (moveable != null)
            {
                List<EffectSpeedVO> moveableEffctList = CreateMoveAble<EditorSpacecraftMoveable>(spaceCraftId, modelName, preStr, moveable);
                if (moveableEffctList != null && moveableEffctList.Count > 0)
                {
                    speedVoList.AddRange(moveableEffctList);
                }
            }
            EditorSpacecraftTurnable turnable = spacecraftMode.turnable;
            if (turnable != null)
            {
                List<EffectSpeedVO> turnableEffctList = CreateMoveAble<EditorSpacecraftTurnable>(spaceCraftId, modelName, preStr, turnable);
                if (turnableEffctList != null && turnableEffctList.Count > 0)
                {
                    speedVoList.AddRange(turnableEffctList);
                }
            }
            EditorSpacecraftMimicry mimicry = spacecraftMode.mimicry;
            if (mimicry != null)
            {
                List<EffectSpeedVO> mimicryableEffctList = CreateMoveAble<EditorSpacecraftMimicry>(spaceCraftId, modelName, preStr, mimicry);
                if (mimicryableEffctList != null && mimicryableEffctList.Count > 0)
                {
                    speedVoList.AddRange(mimicryableEffctList);
                }
            }

            //巡航模式下才有跃迁能力
            if ((SpacecraftSpeedMode)spacecraftMode.speed_mode == SpacecraftSpeedMode.CruiseMode)
            {
                EditorSpacecraftTransition transition = spacecraftMode.transition;
                if (transition != null)
                {
                    List<EffectSpeedVO> transitionEffctList = CreateMoveAble<EditorSpacecraftTransition>(spaceCraftId, modelName, preStr, transition);
                    if (transitionEffctList != null && transitionEffctList.Count > 0)
                    {
                        speedVoList.AddRange(transitionEffctList);
                    }
                }
            }
            return speedVoList;
        }
        public static void SaveSpacecraftToJson(string modelName, EditorSpacecraft spacecraftData)
        {
            string filePath = "";
            GamingMapEditorSetting gamingSetting = GamingMapEditorUtility.GetGamingMapEditorSetting();
            if (gamingSetting != null)
            {
                filePath = MapEditorUtility.GetFullPath(gamingSetting.m_SpacecraftPropertyPath);
            }
            if (!Directory.Exists(filePath))
            {
                EditorUtility.DisplayDialog("提示", "请先设置json保存目录", "确定");
                return;
            }
            filePath = string.Format("{0}/{1}", filePath, modelName);
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            filePath = string.Format("{0}/{1}.json", filePath, spacecraftData.spacecraft_id);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            CheckSpacecraft(spacecraftData);
            string jsondata = JsonUtility.ToJson(spacecraftData, true);
            File.WriteAllText(filePath, jsondata, Encoding.UTF8);
            MapEditorUtility.OpenFolder(filePath);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 矫正
        /// </summary>
        /// <param name="spacecraftData"></param>
        private static void CheckSpacecraft(EditorSpacecraft spacecraftData)
        {
            if (spacecraftData == null)
            {
                return;
            }

            EditorSpacecraftMode[] spacecraftModes = spacecraftData.spacecraft_modes;
            if (spacecraftModes == null || spacecraftModes.Length <= 0)
            {
                return;
            }
            for (int iMode = 0; iMode < spacecraftModes.Length; iMode++)
            {
                EditorSpacecraftMode spacecraftMode = spacecraftModes[iMode];
                if (spacecraftMode.speed_mode == (int)SpacecraftSpeedMode.CruiseMode)
                {
                    continue;
                }
                spacecraftMode.transition = null;
            }
        }

        public static List<EditorSpacecraft> LoadAllSpacecraft()
        {
            string filePath = "";
            GamingMapEditorSetting gamingSetting = GamingMapEditorUtility.GetGamingMapEditorSetting();
            if (gamingSetting != null)
            {
                filePath = MapEditorUtility.GetFullPath(gamingSetting.m_SpacecraftPropertyPath);
            }
            if (!Directory.Exists(filePath))
            {
                EditorUtility.DisplayDialog("提示", "请先设置json保存目录", "确定");
                return null;
            }

            string[] files = Directory.GetFiles(filePath, "*.json", SearchOption.AllDirectories);
            if (files != null && files.Length > 0)
            {
                List<EditorSpacecraft> spacecraftList = new List<EditorSpacecraft>();
                UTF8Encoding utf8 = new UTF8Encoding(false);
                for (int iFile = 0; iFile < files.Length; iFile++)
                {
                    string jsonData = File.ReadAllText(files[iFile], utf8);
                    EditorSpacecraft starMap = JsonUtility.FromJson<EditorSpacecraft>(jsonData);
                    spacecraftList.Add(starMap);
                }
                return spacecraftList;
            }
            return null;
        }

        public static List<EditorSpacecraft> LoadSpacecrafts(string modelName)
        {
            string filePath = "";
            GamingMapEditorSetting gamingSetting = GamingMapEditorUtility.GetGamingMapEditorSetting();
            if (gamingSetting != null)
            {
                filePath = MapEditorUtility.GetFullPath(gamingSetting.m_SpacecraftPropertyPath);
            }
            if (!Directory.Exists(filePath))
            {
                EditorUtility.DisplayDialog("提示", "请先设置json保存目录", "确定");
                return null;
            }
            filePath = string.Format("{0}/{1}", filePath, modelName);
            if (!Directory.Exists(filePath))
            {
                return null;
            }
            string[] files = Directory.GetFiles(filePath, "*.json", SearchOption.AllDirectories);
            if (files != null && files.Length > 0)
            {
                List<EditorSpacecraft> spacecraftList = new List<EditorSpacecraft>();
                UTF8Encoding utf8 = new UTF8Encoding(false);
                for (int iFile = 0; iFile < files.Length; iFile++)
                {
                    string jsonData = File.ReadAllText(files[iFile], utf8);
                    EditorSpacecraft starMap = JsonUtility.FromJson<EditorSpacecraft>(jsonData);
                    spacecraftList.Add(starMap);
                }
                return spacecraftList;
            }
            return null;
        }

        public static EditorSpacecraft LoadSpacecraftFromJson(string modelName, int spacecraftId)
        {
            string filePath = "";
            GamingMapEditorSetting gamingSetting = GamingMapEditorUtility.GetGamingMapEditorSetting();
            if (gamingSetting != null)
            {
                filePath = MapEditorUtility.GetFullPath(gamingSetting.m_SpacecraftPropertyPath);
            }
            if (!Directory.Exists(filePath))
            {
                EditorUtility.DisplayDialog("提示", "请先设置json保存目录", "确定");
                return null;
            }
            filePath = string.Format("{0}/{1}/{2}.json", filePath, modelName, spacecraftId);
            if (!File.Exists(filePath))
            {
                Debug.LogError("不存在Spacecraft数据");
                return null;
            }
            UTF8Encoding utf8 = new UTF8Encoding(false);
            string jsonData = File.ReadAllText(filePath, utf8);
            EditorSpacecraft starMap = JsonUtility.FromJson<EditorSpacecraft>(jsonData);
            return starMap;
        }

        public static void LoadModelVO()
        {
            List<ModelVO> modelList = ConfigVO<ModelVO>.Instance.GetList();
            if (modelList != null && modelList.Count > 0)
            {
                return;
            }
            if (m_VoNameDic == null)
            {
                m_VoNameDic = new Dictionary<string, List<string>>();
            }
            m_VoNameDic.Clear();

            if (!EditorGamingMapData.m_VoNameDic.ContainsKey(typeof(ModelVO).Name))
            {
                List<string> modelInfo = new List<string>();
                modelInfo.Add("model.csv");
                modelInfo.Add("model");
                EditorGamingMapData.m_VoNameDic.Add(typeof(ModelVO).Name, modelInfo);
            }
            ConfigVO<ModelVO>.Instance.Clear();
            ConfigVO<ModelVO>.Instance.GetList();
        }
#endregion

#region 寻宝
        [MenuItem("Custom/Map/导出group_treasure.csv")]
        private static void ExportGroupTreasure()
        {
            if (EditorGamingMapData.m_VoNameDic == null)
            {
                EditorGamingMapData.m_VoNameDic = new Dictionary<string, List<string>>();
            }
            if (!EditorGamingMapData.m_VoNameDic.ContainsKey(typeof(GroupTreasureVO).Name))
            {
                List<string> languageList = new List<string>();
                languageList.Add("group_treasure.csv");
                languageList.Add("group_treasure");
                EditorGamingMapData.m_VoNameDic.Add(typeof(GroupTreasureVO).Name, languageList);
            }

            ConfigVO<GroupTreasureVO>.Instance.Clear();
            ConfigVO<GroupTreasureVO>.Instance.GetList();
            List<GroupTreasureVO> totalList = new List<GroupTreasureVO>();

            MapEditorSetting mapSetting = MapEditorUtility.GetMapEditorSetting();
            string foldPath = "";
            if (mapSetting != null)
            {
                foldPath = MapEditorUtility.GetFullPath(mapSetting.m_TreasurePrefabSavePath);
            }
            if (!Directory.Exists(foldPath))
            {
                EditorUtility.DisplayDialog("提示", "请先设置预设保存目录", "确定");
                return;
            }

            DirectoryInfo direction = new DirectoryInfo(foldPath);
            FileInfo[] files = direction.GetFiles("*.Prefab", SearchOption.AllDirectories);
            int groupIndex = 1;
            if(files != null && files.Length>0)
            {
                for(int iFile = 0;iFile<files.Length;iFile++)
                {
                    FileInfo fileInfo = files[iFile];
                    string fullName = fileInfo.FullName;
                    string absolutePath = System.IO.Directory.GetParent(Application.dataPath).FullName;
                    string relativePath = fullName.Replace(absolutePath+"\\", "");
                    if(!string.IsNullOrEmpty(relativePath))
                    {
                        GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(relativePath);
                        SemaphoreMark semMark = obj.GetComponent<SemaphoreMark>();
                        if (obj != null && semMark != null)
                        {
                            TreasureInfoMark[] treasureInfoMarks = semMark.TreasureInfoMarks;
                            if(treasureInfoMarks != null && treasureInfoMarks.Length>0)
                            {
                                for(int iTreasure = 0;iTreasure<treasureInfoMarks.Length;iTreasure++)
                                {
                                    TreasureInfoMark mark = treasureInfoMarks[iTreasure];
                                    Vector3 markPos = mark.transform.localPosition;
                                    GroupTreasureVO treasureVo = new GroupTreasureVO();
                                    treasureVo.ID = groupIndex++;
                                    treasureVo.groupId = (int)semMark.m_GroupId;
                                    treasureVo.teamMember = (int)mark.m_NpcId;
                                    treasureVo.offestX = markPos.x;
                                    treasureVo.offestY = markPos.y;
                                    treasureVo.offestZ = markPos.z;
                                    totalList.Add(treasureVo);
                                }
                                
                            }
                        }
                    }
                }
            }

            ConfigVO<GroupTreasureVO>.Instance.Set(totalList, false);
            ConfigVO<GroupTreasureVO>.Instance.SaveCSV();
            string path = EditorConfigData.GetExcelFilePath("group_treasure.csv");
            MapEditorUtility.OpenFolder(path);

        }
#endregion

#region 挖矿
        [MenuItem("Custom/Map/导出group_mineral.csv")]
        private static void ExportGroupMineral()
        {
            if (EditorGamingMapData.m_VoNameDic == null)
            {
                EditorGamingMapData.m_VoNameDic = new Dictionary<string, List<string>>();
            }
            if (!EditorGamingMapData.m_VoNameDic.ContainsKey(typeof(GroupMineralVO).Name))
            {
                List<string> languageList = new List<string>();
                languageList.Add("group_mineral.csv");
                languageList.Add("group_mineral");
                EditorGamingMapData.m_VoNameDic.Add(typeof(GroupMineralVO).Name, languageList);
            }

            ConfigVO<GroupMineralVO>.Instance.Clear();

            List<GroupMineralVO> totalList = new List<GroupMineralVO>();

            MapEditorSetting mapSetting = MapEditorUtility.GetMapEditorSetting();
            string foldPath = "";
            if (mapSetting != null)
            {
                foldPath = MapEditorUtility.GetFullPath(mapSetting.m_MineralPrefabSavePath);
            }
            if (!Directory.Exists(foldPath))
            {
                EditorUtility.DisplayDialog("提示", "请先设置预设保存目录", "确定");
                return;
            }

            DirectoryInfo direction = new DirectoryInfo(foldPath);
            FileInfo[] files = direction.GetFiles("*.Prefab", SearchOption.AllDirectories);
            int groupIndex = 1;
            if (files != null && files.Length > 0)
            {
                for (int iFile = 0; iFile < files.Length; iFile++)
                {
                    FileInfo fileInfo = files[iFile];
                    string fullName = fileInfo.FullName;
                    string absolutePath = System.IO.Directory.GetParent(Application.dataPath).FullName;
                    string relativePath = fullName.Replace(absolutePath + "\\", "");
                    if (!string.IsNullOrEmpty(relativePath))
                    {
                        GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(relativePath);
                        SemaphoreMark semMark = obj.GetComponent<SemaphoreMark>();
                        if (obj != null && semMark != null)
                        {
                            TreasureInfoMark[] treasureInfoMarks = semMark.TreasureInfoMarks;
                            if (treasureInfoMarks != null && treasureInfoMarks.Length > 0)
                            {
                                for (int iTreasure = 0; iTreasure < treasureInfoMarks.Length; iTreasure++)
                                {
                                    TreasureInfoMark mark = treasureInfoMarks[iTreasure];
                                    Vector3 markPos = mark.transform.localPosition;
                                    GroupMineralVO mineralVo = new GroupMineralVO();
                                    mineralVo.ID = groupIndex++;
                                    mineralVo.groupId = (int)semMark.m_GroupId;
                                    mineralVo.teamMember = (int)mark.m_NpcId;
                                    mineralVo.offestX = markPos.x;
                                    mineralVo.offestY = markPos.y;
                                    mineralVo.offestZ = markPos.z;
                                    totalList.Add(mineralVo);
                                }

                            }
                        }
                    }
                }
            }

            ConfigVO<GroupMineralVO>.Instance.Set(totalList);
            ConfigVO<GroupMineralVO>.Instance.SaveCSV();
            string path = EditorConfigData.GetExcelFilePath("group_mineral.csv");
            MapEditorUtility.OpenFolder(path);
        }
#endregion
    }
}

#endif

