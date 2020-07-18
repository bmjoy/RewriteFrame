using Eternity.FlatBuffer;
using FlatBuffers;
using Leyoutech.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

public class MySpacecraftLauncher
{
    public MyLauncher MyLauncher;
}

public class MyLauncher
{
    public List<MyLauncherVO> LauncherVOList;
}

public class MyLauncherVO
{
    public int ID;
    public List<MyLauncherMergeVO> LauncherMergeVOList;
}

public class MyLauncherMergeVO
{
    public int PointType;
    public List<MyPointInfoVO> PointInfoVOList;
}

public class MyPointInfoVO
{
    public Vector3 Position;
    public Vector3 Direction;
}


/// <summary>
/// 导出所有飞船Prefab的技能释放点信息
/// </summary>
public class ExportLauncherInfomationToFlatBuffer
{
    [MenuItem("Loveasy/Data Export/ExportShipLaunchersToFlatBuffer")]
    public static void OutputShipLaunchersToFlatBuffer()
    {
        // 所有飞船Prefab的目录
        List<string> prefabDirPath = new List<string>();
        prefabDirPath.Add(Application.dataPath + "/Artwork/Spacecraft/Ships_Player/Prefabs");
        prefabDirPath.Add(Application.dataPath + "/Artwork/Spacecraft/Ships_Enemy/Prefabs");
        prefabDirPath.Add(Application.dataPath + "/Artwork/Spacecraft/Ships_NPC/Prefabs");
        prefabDirPath.Add(Application.dataPath + "/Artwork/Spacecraft/Ships_device/Prefabs");

        // 找到Prefab那一列
        // 打开Excel文件
        /*ExcelWriter excelWriter = new ExcelWriter();
        excelWriter.OpenExcel(Application.dataPath + "/../../eternity_configurations/share/data/model.xlsx");
        excelWriter.OpenSheet("Sheet1");*/

        string csvPath = Application.dataPath + "/../../eternity_configurations/share/data/csv/model.csv";
        string sheetName = "model";
        CSVReader csvReader = new CSVReader();
        csvReader.OpenCSV(csvPath,sheetName);

        // 遍历所有Prefab, 写入FlatBuffer文件
        MyLauncher myLauncher = new MyLauncher();
        myLauncher.LauncherVOList = new List<MyLauncherVO>();

        List<string> errorList = new List<string>();
        foreach (string dirPath in prefabDirPath)
        {
            Debug.Log(dirPath);
            List<string> prefabPathList = GetAllFilesWithExtension(dirPath, "prefab");
            for (int iPath = 0; iPath < prefabPathList.Count; iPath++)
            {
                // List<MyLauncherVO> launcherList = GetShipLauncher(excelWriter, FileUtility.AbsolutePathToRelativeDataPath(prefabPathList[iPath]), errorList);
                List<MyLauncherVO> launcherList = GetShipLauncher(csvReader, FileUtility.AbsolutePathToRelativeDataPath(prefabPathList[iPath]), errorList);
                if (launcherList != null)
                    myLauncher.LauncherVOList.AddRange(launcherList);
            }
        }

        TransMyDataToFlatBuffer(myLauncher);

        csvReader.CloseCsv();

        // Read
        string fileName = Application.dataPath + "\\..\\..\\product\\server\\resource\\flatbuffers\\spacecraft_launcher.bytes";
        fileName.Replace("/", "\\");
        byte[] fileData = File.ReadAllBytes(fileName);
        ByteBuffer byteBuffer = new ByteBuffer(fileData);
        TestDataRead(byteBuffer);


        // Display Dialog
        StringBuilder errorInfo = new StringBuilder();
        foreach (string error in errorList)
        {
            errorInfo.Append(error);
            errorInfo.Append('\n');
        }

        if (errorList.Count != 0)
            EditorUtility.DisplayDialog("某些飞船的技能释放点信息未导出", errorInfo.ToString(), "朕知道了");

		string Message = string.Format("已经把所有飞船的技能释放点信息导出至 {0}"
			, Application.dataPath + "\\..\\..\\product\\server\\resource\\flatbuffers\\spacecraft_launcher.bytes");
		fileName.Replace("/", "\\");
		EditorUtility.DisplayDialog("导出完成", Message, "朕知道了");

        if(Application.isBatchMode)
        {
            EditorApplication.Exit(0);
        }
    }

    private static List<MyLauncherVO> GetShipLauncher(CSVReader excelWriter, string shipPrefabPath, List<string> errorList)
    {
        // AssetPath
        GameObject ship = AssetDatabase.LoadAssetAtPath<GameObject>(shipPrefabPath);
        Debug.Log(ship.name);

		SpacecraftPresentation socketContainer = ship.GetComponent<SpacecraftPresentation>();
        if (socketContainer == null)
        {
            errorList.Add("Prefab: " + shipPrefabPath + " 上没有挂载 SpacecraftPresentation, 无法导出技能释放点信息.");
            Debug.LogWarningFormat("Prefab: {0} 上没有挂载 SpacecraftPresentation, 无法导出技能释放点信息.", shipPrefabPath);
            return null;
        }

        List<MyLauncherMergeVO> launcherMergeList = new List<MyLauncherMergeVO>();

        // 输出所有武器的挂点信息
        launcherMergeList.Add(TransformToLauncherMergeVO(SkillLaunchPoint.WeaponMain, socketContainer.socket_main));
        launcherMergeList.Add(TransformToLauncherMergeVO(SkillLaunchPoint.WeaponSub, socketContainer.socket_secondary));
        launcherMergeList.Add(TransformToLauncherMergeVO(SkillLaunchPoint.WeaponSuper, socketContainer.socket_super));
        launcherMergeList.Add(TransformToLauncherMergeVO(SkillLaunchPoint.WeaponFurnace, socketContainer.socket_furnace));
        // 输出所有技能点的挂点信息
        launcherMergeList.Add(TransformToLauncherMergeVO(SkillLaunchPoint.Center, socketContainer.socket_skill_center));
        launcherMergeList.Add(TransformToLauncherMergeVO(SkillLaunchPoint.QuickFirer, socketContainer.socket_skill_quickFirer));
        launcherMergeList.Add(TransformToLauncherMergeVO(SkillLaunchPoint.Cannon, socketContainer.socket_skill_cannon));
        launcherMergeList.Add(TransformToLauncherMergeVO(SkillLaunchPoint.RailGun, socketContainer.socket_skill_railGun));
        launcherMergeList.Add(TransformToLauncherMergeVO(SkillLaunchPoint.Rocket, socketContainer.socket_skill_rocket));
        launcherMergeList.Add(TransformToLauncherMergeVO(SkillLaunchPoint.Missile, socketContainer.socket_skill_missile));
        launcherMergeList.Add(TransformToLauncherMergeVO(SkillLaunchPoint.Torpedo, socketContainer.socket_skill_torpedo));
        launcherMergeList.Add(TransformToLauncherMergeVO(SkillLaunchPoint.Laser, socketContainer.socket_skill_laser));
        launcherMergeList.Add(TransformToLauncherMergeVO(SkillLaunchPoint.Electromagnetism, socketContainer.socket_skill_electromagnetism));
        launcherMergeList.Add(TransformToLauncherMergeVO(SkillLaunchPoint.Particle, socketContainer.socket_skill_particle));
        launcherMergeList.Add(TransformToLauncherMergeVO(SkillLaunchPoint.SkillA, socketContainer.socket_skill_skillA));
        launcherMergeList.Add(TransformToLauncherMergeVO(SkillLaunchPoint.SkillB, socketContainer.socket_skill_skillB));
        launcherMergeList.Add(TransformToLauncherMergeVO(SkillLaunchPoint.SkillC, socketContainer.socket_skill_skillC));
        launcherMergeList.Add(TransformToLauncherMergeVO(SkillLaunchPoint.SkillD, socketContainer.socket_skill_skillD));


        // 找到所有使用这个Prefab的Module, 记录他们的ModuleID
        List<int> moduleIDList = new List<int>();
        int rowCount = excelWriter.GetRowCount();
        for (int iRow = 1; iRow <= rowCount; iRow++)
        {
            if (excelWriter.GetCellText(iRow, 3) != "1")
                continue;

            if (excelWriter.GetCellText(iRow, 4) == FileUtility.ExtractFileNameFromAbsolutePath(shipPrefabPath))
            {
                string strIndex = excelWriter.GetCellText(iRow, 1);
                int index = Int32.Parse(strIndex);

                moduleIDList.Add(index);
            }
        }

        // 每个Module弄一个 MyLauncherVO
        List<MyLauncherVO> launcherVOList = new List<MyLauncherVO>();
        for (int iModule = 0; iModule < moduleIDList.Count; iModule++)
        {
            MyLauncherVO launcherVO = new MyLauncherVO();
            launcherVO.ID = moduleIDList[iModule];
            launcherVO.LauncherMergeVOList = launcherMergeList;
            launcherVOList.Add(launcherVO);
        }

        return launcherVOList;
    }


    /// <summary>
    /// 把自定义的数据格式转换成Flatbuffer
    /// </summary>
    /// <param name="launcherVOList"></param>
    public static void TransMyDataToFlatBuffer(MyLauncher myLauncher)
    {
		
        FlatBufferBuilder builder = new FlatBufferBuilder(1);

        List<MyLauncherVO> launcherVOList = myLauncher.LauncherVOList;
        bool haveLauncherVO = launcherVOList != null && launcherVOList.Count > 0;

        // LauncherVO
        Offset<LauncherVO>[] launcherVOs = haveLauncherVO ? new Offset<LauncherVO>[launcherVOList.Count] : null;
        if (haveLauncherVO)
        {
            // 这里使用反序的原因是, 服务器读出来的顺序与我写入的顺序是相反的
            // FlatBuffer的内部实现没看明白. 先直接写成反序了
            for (int iLauncherVO = launcherVOList.Count - 1; iLauncherVO >= 0; iLauncherVO--)
            {
                List<MyLauncherMergeVO> mergeList = launcherVOList[iLauncherVO].LauncherMergeVOList;
                Offset<LauncherMergeVO>[] mergeVOs = new Offset<LauncherMergeVO>[mergeList.Count];
                for (int iMerge = mergeList.Count - 1; iMerge >= 0; iMerge--)
                {
                    List<MyPointInfoVO> pointList = mergeList[iMerge].PointInfoVOList;
                    // pointInfoVo
                    LauncherMergeVO.StartPointInfoArrayVector(builder, pointList.Count);
                    for (int iPoint = pointList.Count - 1; iPoint >= 0; iPoint--)
                    {
                        Vector3 pos = pointList[iPoint].Position;
                        Vector3 dir = pointList[iPoint].Direction;
                        pointInfoVo.CreatepointInfoVo(builder, pos.x, pos.z, pos.y, dir.z, dir.y, dir.x);
                    }
                    VectorOffset pointVector = builder.EndVector();

                    // MergeVO
                    LauncherMergeVO.StartLauncherMergeVO(builder);
                    LauncherMergeVO.AddPointType(builder, mergeList[iMerge].PointType);
                    LauncherMergeVO.AddPointInfoArray(builder, pointVector);
                    mergeVOs[iMerge] = LauncherMergeVO.EndLauncherMergeVO(builder);
                }
                VectorOffset mergeVOVector = LauncherMergeVO.CreateSortedVectorOfLauncherMergeVO(builder, mergeVOs);

                // LauncherVO
                LauncherVO.StartLauncherVO(builder);
                LauncherVO.AddId(builder, launcherVOList[iLauncherVO].ID);
                LauncherVO.AddMergeArray(builder, mergeVOVector);
                launcherVOs[iLauncherVO] = LauncherVO.EndLauncherVO(builder);
            }
        }

        // Launcher
        VectorOffset launcherVOVector = haveLauncherVO ? LauncherVO.CreateSortedVectorOfLauncherVO(builder, launcherVOs) : new VectorOffset();
        Launcher.StartLauncher(builder);
        if (haveLauncherVO)
        {
            Launcher.AddData(builder, launcherVOVector);
        }
        Offset<Launcher> launcher = Launcher.EndLauncher(builder);

        // SpacecraftLauncher
        SpacecraftLauncher.StartSpacecraftLauncher(builder);
        SpacecraftLauncher.AddLauncher(builder, launcher);
        Offset<SpacecraftLauncher> spacecraft = SpacecraftLauncher.EndSpacecraftLauncher(builder);

        SpacecraftLauncher.FinishSpacecraftLauncherBuffer(builder, spacecraft);

        // Write To File
        using (MemoryStream stream = new MemoryStream(builder.DataBuffer.ToFullArray(), builder.DataBuffer.Position, builder.Offset))
        {
            byte[] data = stream.ToArray();
            string fileName = Application.dataPath + "\\..\\..\\product\\server\\resource\\flatbuffers\\spacecraft_launcher.bytes";
            fileName.Replace("/", "\\");
            File.WriteAllBytes(fileName, data);
        }

        /*using (MemoryStream stream = new MemoryStream(builder.DataBuffer.ToFullArray(), builder.DataBuffer.Position, builder.Offset))
        {
            byte[] data = stream.ToArray();
            string fileName = Application.dataPath + "\\..\\..\\data\\output\\flatBuffers\\server\\data\\spacecraft_launcher.bytes";
            fileName.Replace("/", "\\");
            File.WriteAllBytes(fileName, data);
        }
		*/
    }

    /// <summary>
    /// 查找 dirPath 指定的文件夹下, 所有扩展名为 extension 的文件
    /// </summary>
    /// <param name="dirPath"></param>
    /// <param name="extension"></param>
    /// <returns></returns>
    private static List<string> GetAllFilesWithExtension(string dirPath, string extension)
    {
        List<string> fileList = FileUtility.FindAllFiles(dirPath, true);

        List<int> removeList = new List<int>();
        for (int iFile = 0; iFile < fileList.Count; ++iFile)
        {
            if (FileUtility.GetExtension(fileList[iFile]) != extension)
                removeList.Add(iFile);
        }

        for (int index = removeList.Count - 1; index >= 0; --index)
        {
            fileList.RemoveAt(removeList[index]);
        }

        return fileList;
    }

    private static MyLauncherMergeVO TransformToLauncherMergeVO(SkillLaunchPoint pointType, List<Transform> transformList)
    {
        MyLauncherMergeVO mergeVO = new MyLauncherMergeVO();
        mergeVO.PointType = (int)pointType;
        mergeVO.PointInfoVOList = new List<MyPointInfoVO>();
        for (int iTrans = 0; iTrans < transformList.Count; iTrans++)
        {
            MyPointInfoVO pointInfo = new MyPointInfoVO();
            pointInfo.Position = transformList[iTrans].position;
            pointInfo.Direction = transformList[iTrans].up;
            mergeVO.PointInfoVOList.Add(pointInfo);
        }

        return mergeVO;
    }


    private static void TestDataRead(ByteBuffer byteBuffer)
    {
		/*
        StringBuilder sb = new StringBuilder();

        SpacecraftLauncher spacecraftLauncher = SpacecraftLauncher.GetRootAsSpacecraftLauncher(byteBuffer);
        Launcher? launcher = spacecraftLauncher.Launcher;
        if (launcher != null)
        {
            for (int iLauncherVO = 0; iLauncherVO < launcher.Value.DataLength; iLauncherVO++)
            {
                LauncherVO? launcherVO = launcher.Value.Data(iLauncherVO);
                if (launcherVO != null)
                {
                    sb.Append("ID: ");
                    sb.Append(launcherVO.Value.Id + "\n");
                    for (int iMergeVO = 0; iMergeVO < launcherVO.Value.MergeArrayLength; iMergeVO++)
                    {
                        LauncherMergeVO? mergeVO = launcherVO.Value.MergeArray(iMergeVO);
                        if (mergeVO != null)
                        {
                            sb.Append("Type: ");
                            sb.Append(mergeVO.Value.PointType + ": ");
                            for (int iPointVO = 0; iPointVO < mergeVO.Value.PointInfoArrayLength; iPointVO++)
                            {
                                pointInfoVo? pointVO = mergeVO.Value.PointInfoArray(iPointVO);
                                if (pointVO != null)
                                {
                                    sb.Append(pointVO.Value.Positionx);
                                    sb.Append(", ");
                                    sb.Append(pointVO.Value.Positiony);
                                    sb.Append(", ");
                                    sb.Append(pointVO.Value.Positionz);
                                    sb.Append("; ");
                                    sb.Append(pointVO.Value.Dirx);
                                    sb.Append(", ");
                                    sb.Append(pointVO.Value.Diry);
                                    sb.Append(", ");
                                    sb.Append(pointVO.Value.Dirz);
                                    sb.Append("| ");
                                }
                            }
                            sb.Append("\n");
                        }
                    }
                    sb.Append("---LauncherVO End----------------------------------------");
                }
            }
        }

        sb.Append("-All End----------------------------------------");

        Debug.Log(sb.ToString());
		*/
    }
}

#endif