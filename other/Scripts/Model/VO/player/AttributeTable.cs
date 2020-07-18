using Assets.Scripts.Define;
using System.Collections.Generic;

public class AttributeTable
{
    private IDictionary<AttributeName, double> attributes = new Dictionary<AttributeName, double>();

    public int Count
    {
        get
        {
            return attributes.Count;
        }
    }

    public ICollection<AttributeName> Keys
    {
        get
        {
            return attributes.Keys;
        }
    }

    public bool ContainsKey(AttributeName key)
    {
        return attributes.Keys.Contains(key);
    }

    public double this[AttributeName key]
    {
        get
        {
            if (ContainsKey(key))
            {
                return attributes[key];
            }
            return 0;
        }
        set
        {
            attributes[key] = value;
            // TODO.
            //AttributeVO attributeVO = m_CfgAttributeCfgProxy.GetAttributeVOByKey((int)key);
            //if (attributeVO.ByteBuffer != null)
            //{
            //    attributes[key] = value / attributeVO.Multiple;
            //}
        }
    }
}
