namespace Leyoutech.Core.Generic
{
    /// <summary>
    /// 唯一ID计数器
    /// </summary>
    public class UniqueIDCreator
    {
        private long m_ID = 0;

        public long Next()
        {
            if (m_ID == long.MaxValue)
                m_ID = 0;
            return ++m_ID;
        }
    }
}

