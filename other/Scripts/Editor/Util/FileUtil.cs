using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace LeyoutechEditor.Core.Util
{
    public static class FileUtil
    {
        /// <summary>
        /// 从指定的路径下读取序列化的二进制数据为指定的对象
        /// 
        /// example:
        /// [Serializable]
        /// public class TestData
        /// {
        ///     public string name;
        /// }
        /// 
        /// TestData data = FileUtil.ReadFromBinary<TestData>("D:/testdata.data");
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static T ReadFromBinary<T>(string filePath) where T : class, new()
        {
            T data = null;
            if(File.Exists(filePath))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.OpenRead(filePath);

                try
                {
                    data = (T)bf.Deserialize(file);
                }catch
                {
                    data = null;
                }
                file.Close();
                if(data == null)
                {
                    File.Delete(filePath);
                    data = new T();
                }
            }else
            {
                data = new T();
            }

            return data;
        }

        /// <summary>
        /// 将给定的数据序列化为二进制数据
        /// 
        /// example:
        /// [Serializable]
        /// public class TestData
        /// {
        ///     public string name;
        /// }
        /// 
        /// TestData data = new TestData();
        /// FileUtil.SaveToBinary<TestData>("D:/testdata.data",data);
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="data"></param>
        public static void SaveToBinary<T>(string filePath,T data) where T:class,new()
        {
            var dirPath = Path.GetDirectoryName(filePath).Replace("\\", "/");
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            if (data == null)
            {
                data = new T();
            }

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(filePath);

            bf.Serialize(file, data);
            file.Close();
        }
    }
}
