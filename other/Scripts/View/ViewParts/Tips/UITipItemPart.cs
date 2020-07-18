using Eternity.FlatBuffer;
using Eternity.Runtime.Item;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITipItemPart : UITipBasePart
{
    private const string TIP_PREFAB = Leyoutech.Core.Loader.Config.AssetAddressKey.PRELOADUI_TIPSITEMPANEL;

    /// <summary>
    /// TIP1的Prefab
    /// </summary>
    private GameObject m_TipPrefab1;
    /// <summary>
    /// TIP2的Prefab
    /// </summary>
    private GameObject m_TipPrefab2;
    /// <summary>
    /// Tip1是否在加载中
    /// </summary>
    private bool m_TipPrefabLoading1;
    /// <summary>
    /// Tip1
    /// </summary>
    private GameObject m_TipInstance1;
    /// <summary>
    /// Tip2是否在加载中
    /// </summary>
    private bool m_TipPrefabLoading2;
    /// <summary>
    /// Tip2
    /// </summary>
    private GameObject m_TipInstance2;

    /// <summary>
    /// 配置
    /// </summary>
    private CfgEternityProxy m_Config;
    /// <summary>
    /// 背包
    /// </summary>
    private PackageProxy m_Package;


    public override void OnShow(object msg)
    {
        base.OnShow(msg);

        m_TipPrefab1 = null;
        m_TipPrefab2 = null;
        m_TipPrefabLoading1 = false;
        m_TipPrefabLoading2 = false;
    }

    public override void OnHide()
    {
        m_TipPrefab1 = null;
        m_TipPrefab2 = null;
        m_TipPrefabLoading1 = false;
        m_TipPrefabLoading2 = false;

        base.OnHide();
    }

    /// <summary>
    /// 获取配置文件
    /// </summary>
    /// <returns>配置数据</returns>
    private CfgEternityProxy GetConfig()
    {
        if (m_Config == null)
            m_Config = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        return m_Config;
    }

    /// <summary>
    /// 获取背包Proxy
    /// </summary>
    /// <returns>背包</returns>
    private PackageProxy GetPackage()
    {
        if (m_Package == null)
            m_Package = GameFacade.Instance.RetrieveProxy(ProxyName.PackageProxy) as PackageProxy;
        return m_Package;
    }

    #region 主Tip

    /// <summary>
    /// 清除视图
    /// </summary>
    protected override void CloseTipView()
    {
        if (m_TipInstance1)
        {
            m_TipInstance1.Recycle();
            m_TipInstance1 = null;
        }

        if (m_TipInstance2)
        {
            m_TipInstance2.Recycle();
            m_TipInstance2 = null;
        }

        base.CloseTipView();
    }

    /// <summary>
    /// 更新Tip视图
    /// </summary>
    /// <param name="data">数据</param>
    protected override void OpenTipView()
    {
        if (TipData is ItemBase)
            OpenTip(TipData as ItemBase);
        else
            base.OpenTipView();

    }

    /// <summary>
    /// 打开Tip
    /// </summary>
    private void OpenTip(ItemBase data)
    {
        if (m_TipPrefab1)
        {
            if (!m_TipInstance1)
            {
                m_TipInstance1 = m_TipPrefab1.Spawn(TipBoxLeft);
                m_TipInstance1.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
            LayoutItemTip(m_TipInstance1, data, null);
        }
        else if (!m_TipPrefabLoading1)
        {
            m_TipPrefabLoading1 = true;

            LoadPrefabFromPool(TIP_PREFAB, (prefab) =>
            {
                if (Opened)
                {
                    m_TipPrefab1 = prefab;

                    OpenTipView();
                }
            });
        }
    }

    #endregion


    #region 比较Tip

    /// <summary>
    /// 查找可以比较的数据
    /// </summary>
    /// <param name="data">当前数据</param>
    /// <param name="compareableDatas">可参与比较的数据列表</param>
    protected override void FindCompareableData(object data, List<object> compareableDatas)
    {
        base.FindCompareableData(data, compareableDatas);

        if (data == null)
            return;
        if (!(data is ItemBase))
            return;

        ItemBase item = data as ItemBase;
        if (item.MainType != Category.Blueprint && item.MainType != Category.Warship && item.MainType != Category.Weapon && item.MainType != Category.Reformer && item.MainType != Category.Equipment)
            return;

        //当前角色的包
        ItemContainer container = GetPackage().GetHeroItem();
        if (container == null || container.Items == null || container.Items.Count == 0)
            return;

        //当前出战的飞船
        ItemWarShipVO ship = null;
        foreach (ItemBase heroItem in container.Items.Values)
        {
            if (heroItem is ItemWarShipVO)
            {
                ship = heroItem as ItemWarShipVO;
                break;
            }
        }

        //当前是飞船
        if (item.MainType == Category.Warship)
        {
            if (item != ship)
                compareableDatas.Add(ship);
        }
        //当前是飞船的蓝图
        else if (item.MainType == Category.Blueprint && (item as ItemDrawingVO).DrawingType == BlueprintL1.Warship)
        {
            compareableDatas.Add(ship);
        }
        else
        {
            IShip iship = (GameFacade.Instance.RetrieveProxy(ProxyName.ShipProxy) as ShipProxy).GetAppointWarShip();
            Category mainType = item.MainType;
            Enum secondaryType = null;

            if (item is ItemDrawingVO)
            {
                ItemDrawingVO blueprint = item as ItemDrawingVO;
                switch (blueprint.DrawingType)
                {
                    case BlueprintL1.Weapon:
                        mainType = Category.Weapon;
                        break;
                    case BlueprintL1.Reformer:
                        mainType = Category.Reformer;
                        break;
                    case BlueprintL1.Equipment:
                        FoundryProxy foundryProxy = GameFacade.Instance.RetrieveProxy(ProxyName.FoundryProxy) as FoundryProxy;
                        Item product = foundryProxy.GetItemByProduceKey((int)blueprint.TID);
                        mainType = Category.Equipment;
                        secondaryType = (Enum)ItemTypeUtil.GetItemType(product.Type).EnumList[2];
                        break;
                }
            }
            else if (item is ItemEquipmentVO)
            {
                ItemEquipmentVO equip = item as ItemEquipmentVO;
                mainType = equip.MainType;
                secondaryType = equip.EquipmentType;
            }

            //武器
            if (mainType == Category.Weapon)
            {
                ItemBase[] list = new ItemBase[iship.GetWeaponContainer().GetCurrentSizeMax()];
                foreach (IWeapon weapon in iship.GetWeaponContainer().GetWeapons())
                    list[weapon.GetPos()] = GetPackage().GetItem<ItemWeaponVO>(weapon.GetUID());
                compareableDatas.AddRange(list);
            }
            //转化炉
            else if (mainType == Category.Reformer)
            {
                IReformer reformer = iship.GetReformerContainer().GetReformer();
                if (reformer != null)
                    compareableDatas.Add(GetPackage().GetItem<ItemReformerVO>(reformer.GetUID()));
                else
                    compareableDatas.Add(null);
            }
            //装备
            else if (mainType == Category.Equipment)
            {
                foreach (IEquipment equip in iship.GetEquipmentContainer().GetEquipments())
                {
                    ItemEquipmentVO equipVO = GetPackage().GetItem<ItemEquipmentVO>(equip.GetUID());
                    Enum type = equipVO.EquipmentType;
                    if (Enum.Equals(type, secondaryType))
                        compareableDatas.Add(equipVO);
                }
            }
        }
    }

    /// <summary>
    /// 关闭比较视图
    /// </summary>
    protected override void CloseCompareView()
    {
        if (m_TipInstance2)
        {
            m_TipInstance2.Recycle();
            m_TipInstance2 = null;
        }

        base.CloseCompareView();
    }

    /// <summary>
    /// 打开比较视图
    /// </summary>
    protected override void OpenCompareView()
    {
        if (TipData is ItemBase && CompareTipData is ItemBase)
            OpenCompareTip(CompareTipData as ItemBase, TipData as ItemBase);
        else
            base.OpenCompareView();
    }


    /// <summary>
    /// 打开Tip
    /// </summary>
    private void OpenCompareTip(ItemBase data, ItemBase compareData)
    {
        if (m_TipPrefab2)
        {
            if (!m_TipInstance2)
            {
                m_TipInstance2 = m_TipPrefab2.Spawn(TipBoxRight);
                m_TipInstance2.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
            LayoutItemTip(m_TipInstance2, data, compareData);
        }
        else if (!m_TipPrefabLoading2)
        {
            m_TipPrefabLoading2 = true;

            LoadPrefabFromPool(TIP_PREFAB, (prefab) =>
            {
                if (Opened)
                {
                    m_TipPrefab2 = prefab;

                    OpenCompareView();
                }
            });
        }
    }

    #endregion


    /// <summary>
    /// 布局Tip
    /// </summary>
    /// <param name="view">视图</param>
    /// <param name="data">数据</param>
    protected void LayoutItemTip(GameObject view, ItemBase data, ItemBase compareData)
    {
        RectTransform root = FindComponent<RectTransform>(view.transform, "TipsScrollView/Viewport/Content/Content");
        //品质部分
        Transform qualityNode = FindComponent<Transform>(view.transform, "TipsScrollView/Viewport/Content/Quality");

        //名称部分
        TMP_Text nameField = FindComponent<TMP_Text>(root, "NameType/Label_Name");

        TMP_Text tnField = FindComponent<TMP_Text>(root, "NameType/Label_T");

        Transform typeNode = FindComponent<Transform>(root, "NameType/Type");
        TMP_Text typeField = FindComponent<TMP_Text>(typeNode, "Label_Type");
        Image typeIcon = FindComponent<Image>(typeNode, "Image_IconType");

        Transform levelNode = FindComponent<Transform>(root, "NameType/Level");
        TMP_Text levelField = FindComponent<TMP_Text>(levelNode, "Label_Score_Num");

        //限制部分
        Transform limitNode = FindComponent<Transform>(root, "Limit");
        TMP_Text limitText = FindComponent<TMP_Text>(limitNode, "Label");

        //材料部分
        Transform materialNode = FindComponent<Transform>(root, "Materials");

        //描述部分
        Transform descriptionNode = FindComponent<Transform>(root, "Describe");
        TMP_Text descriptionField = FindComponent<TMP_Text>(descriptionNode, "Label_Describe");

        //属性部分
        Transform attributeNode = FindComponent<Transform>(root, "Attribute");
        //属性部分A
        Transform attributeANode = FindComponent<Transform>(root, "Attribute/Top");
        //属性部分B
        Transform attributeBNode = FindComponent<Transform>(root, "Attribute/Temp");
        //属性部分C
        Transform attributeCNode = FindComponent<Transform>(root, "Attribute/DFD");
        //属性部分D
        Transform attributeDNode = FindComponent<Transform>(root, "Attribute/Repair");

        //属性部分Nature 容器
        Transform attributeNature = FindComponent<Transform>(root, "Attribute/nature");
        //属性部分Extra容器
        Transform attributeExtra = FindComponent<Transform>(root, "Attribute/Extra");

        //属性部分Nature 单体
        Transform attributeNatureSingle = FindComponent<Transform>(root, "Attribute/nature/content1");
        //属性部分Extra 单体
        Transform attributeExtraSingle = FindComponent<Transform>(root, "Attribute/Extra/content1");


        //技能部分
        Transform skillNode = FindComponent<Transform>(root, "Attribute/Skill");

        //随机属性部分
        Transform randomNode = FindComponent<Transform>(root, "Random");

        //模组部分
        Transform modNode = FindComponent<Transform>(root, "MOD");

        //关掉所有
        tnField.text = string.Empty;
        typeNode.gameObject.SetActive(false);
        levelNode.gameObject.SetActive(false);
        limitNode.gameObject.SetActive(false);
        materialNode.gameObject.SetActive(false);
        attributeNode.gameObject.SetActive(false);
        attributeANode.gameObject.SetActive(false);
        attributeBNode.gameObject.SetActive(false);
        attributeCNode.gameObject.SetActive(false);
        attributeDNode.gameObject.SetActive(false);
        skillNode.gameObject.SetActive(false);
        randomNode.gameObject.SetActive(false);
        modNode.gameObject.SetActive(false);

        attributeNature.gameObject.SetActive(false);
        attributeExtra.gameObject.SetActive(false);
        //名称
        nameField.text = TableUtil.GetItemName(data.TID);
        //描述
        descriptionField.text = TableUtil.GetItemDescribe(data.TID);
        //品质
        int quality = data.ItemConfig.Quality;
        //data.ItemConfig
        for (int i = 0; i < qualityNode.childCount; i++)
        {
            qualityNode.GetChild(i).gameObject.SetActive((i + 1) == quality);
        }

        //蓝图
        if (data.MainType == Category.Blueprint)
        {
            ItemDrawingVO blueprint = data as ItemDrawingVO;

            //类型
            typeNode.gameObject.SetActive(true);
            typeField.text = TableUtil.GetLanguageString(blueprint.DrawingType);
            switch (blueprint.DrawingType)
            {

                case BlueprintL1.Warship: UIUtil.SetIconImage(typeIcon, 31005); break;
                case BlueprintL1.Weapon: UIUtil.SetIconImage(typeIcon, 31006); break;
                case BlueprintL1.Reformer: UIUtil.SetIconImage(typeIcon, 31007); break;
                case BlueprintL1.EquipmentMod: UIUtil.SetIconImage(typeIcon, 31008); break;
                case BlueprintL1.Equipment: UIUtil.SetIconImage(typeIcon, 31009); break;
                case BlueprintL1.Material: UIUtil.SetIconImage(typeIcon, 31010); break;
                default: typeIcon.sprite = null; break;
            }

            //材料
            materialNode.gameObject.SetActive(true);
            LayoutBlueprintMaterial(blueprint.TID, materialNode);

            //飞船蓝图
            if (blueprint.DrawingType == BlueprintL1.Warship)
            {
                attributeNode.gameObject.SetActive(true);
                attributeANode.gameObject.SetActive(true);
                attributeBNode.gameObject.SetActive(true);
                attributeCNode.gameObject.SetActive(true);
                attributeDNode.gameObject.SetActive(true);
                skillNode.gameObject.SetActive(true);

                modNode.gameObject.SetActive(true);
                LayoutBlueprintModList(blueprint.TID, modNode);
            }
            //武器蓝图
            else if (blueprint.DrawingType == BlueprintL1.Weapon)
            {
                attributeNode.gameObject.SetActive(true);
                attributeANode.gameObject.SetActive(true);
                attributeBNode.gameObject.SetActive(true);
                randomNode.gameObject.SetActive(true);

                modNode.gameObject.SetActive(true);
                LayoutBlueprintModList(blueprint.TID, modNode);
            }
            //转化炉蓝图
            else if (blueprint.DrawingType == BlueprintL1.Reformer)
            {
                attributeNode.gameObject.SetActive(true);
                attributeANode.gameObject.SetActive(true);
                attributeBNode.gameObject.SetActive(true);
                randomNode.gameObject.SetActive(true);
            }
            //装备蓝图
            else if (blueprint.DrawingType == BlueprintL1.Equipment)
            {
                attributeNode.gameObject.SetActive(true);
                attributeANode.gameObject.SetActive(true);
                attributeBNode.gameObject.SetActive(true);
                randomNode.gameObject.SetActive(true);
            }
            //装备模组蓝图
            else if (blueprint.DrawingType == BlueprintL1.EquipmentMod)
            {
                attributeNode.gameObject.SetActive(true);
                randomNode.gameObject.SetActive(true);
            }
            //材料蓝图
            else if (blueprint.DrawingType == BlueprintL1.Material)
            {
            }
        }
        //飞船
        else if (data.MainType == Category.Warship)
        {
            ItemWarShipVO ship = data as ItemWarShipVO;

            typeNode.gameObject.SetActive(true);
            typeField.text = TableUtil.GetLanguageString(ship.WarshipL1);
            UIUtil.SetIconImage(typeIcon, 31005);

            attributeNode.gameObject.SetActive(true);
            attributeANode.gameObject.SetActive(true);
            attributeBNode.gameObject.SetActive(true);
            attributeCNode.gameObject.SetActive(true);
            attributeDNode.gameObject.SetActive(true);
            skillNode.gameObject.SetActive(true);

            modNode.gameObject.SetActive(true);
            LayoutShipOrWeaponModList(ship, EquipmentModL1.WarshipMod, modNode);
        }
        //武器
        else if (data.MainType == Category.Weapon)
        {
            ItemWeaponVO weapon = data as ItemWeaponVO;

            typeNode.gameObject.SetActive(true);
            typeField.text = TableUtil.GetLanguageString(weapon.WeaponType2);
            UIUtil.SetIconImage(typeIcon, 31006);

            tnField.text = "T" + weapon.ItemConfig.Grade;

            attributeNode.gameObject.SetActive(true);
            attributeANode.gameObject.SetActive(true);
            attributeBNode.gameObject.SetActive(true);
            randomNode.gameObject.SetActive(true);

            modNode.gameObject.SetActive(true);
            LayoutShipOrWeaponModList(weapon, EquipmentModL1.WeaponMod, modNode);
        }
        //转化炉
        else if (data.MainType == Category.Reformer)
        {
            ItemReformerVO reformer = data as ItemReformerVO;

            typeNode.gameObject.SetActive(true);
            typeField.text = TableUtil.GetLanguageString(Category.Reformer);
            UIUtil.SetIconImage(typeIcon, 31007);

            tnField.text = "T" + reformer.ItemConfig.Grade;

            attributeNode.gameObject.SetActive(true);
            attributeANode.gameObject.SetActive(true);
            attributeBNode.gameObject.SetActive(true);
            randomNode.gameObject.SetActive(true);
        }
        //装备
        else if (data.MainType == Category.Equipment)
        {
            ItemEquipmentVO equip = data as ItemEquipmentVO;

            typeNode.gameObject.SetActive(true);

            typeField.text = TableUtil.GetLanguageString(equip.EquipmentType);
            UIUtil.SetIconImage(typeIcon, 31009);

            tnField.text = "T" + equip.ItemConfig.Grade;

            attributeNode.gameObject.SetActive(true);
            attributeANode.gameObject.SetActive(true);
            attributeBNode.gameObject.SetActive(true);
            randomNode.gameObject.SetActive(true);
        }
        //MOD
        else if (data.MainType == Category.EquipmentMod)
        {
            ItemModVO mod = data as ItemModVO;

            typeNode.gameObject.SetActive(true);
            typeField.text = TableUtil.GetLanguageString(mod.ModType1);
            UIUtil.SetIconImage(typeIcon, 31008);

            attributeNode.gameObject.SetActive(true);
            randomNode.gameObject.SetActive(true);
        }
        //材料
        else if (data.MainType == Category.Material)
        {
            ItemMaterialVO itemMaterial = data as ItemMaterialVO;

            typeNode.gameObject.SetActive(true);
            typeField.text = TableUtil.GetLanguageString(itemMaterial.MaterialType);
            UIUtil.SetIconImage(typeIcon, 31010);
        }
        //消耗品
        else if (data.MainType == Category.Expendable)
        {
            ItemExpendableVO itemExpendable = data as ItemExpendableVO;

            typeNode.gameObject.SetActive(true);
            typeField.text = TableUtil.GetLanguageString(itemExpendable.ExpendableType);
            UIUtil.SetIconImage(typeIcon, 51027);
        }
    }


    #region Tip的材料部分

    /// <summary>
    /// 布局蓝图的材料部分
    /// </summary>
    /// <param name="tid">蓝图的TID</param>
    /// <param name="materialNode">材料节点</param>

    private void LayoutBlueprintMaterial(uint tid, Transform materialNode)
    {
        CfgEternityProxy cfg = GetConfig();
        PackageProxy pack = GetPackage();

        Produce produce = cfg.GetProduceByKey(tid);

        Transform materialList = FindComponent<Transform>(materialNode, "Resources");
        TMP_Text materialTimeField = FindComponent<TMP_Text>(materialNode, "Label_Time");
        materialTimeField.text = TableUtil.GetLanguageString("production_text_1024") + TimeUtil.GetTimeStr(produce.Time);

        int index = 0;
        int currencyIndex = 0;
        List<EffectElement?> sortedElements = new List<EffectElement?>();
        if (produce.Cost > 0)
        {
            EffectElement?[] elements = cfg.GetEffectElementsByKey((uint)produce.Cost);
            if (elements.Length > 0)
            {
                for (int i = 0; i < elements.Length; i++)
                {
                    EffectElement? element = elements[i];
                    Item item = cfg.GetItemByKey((uint)element.Value.ItemId);

                    if (ItemTypeUtil.GetItemType(item.Type).MainType == Category.Currency)
                    {
                        sortedElements.Insert(currencyIndex, element);
                        currencyIndex++;
                    }
                    else
                    {
                        sortedElements.Add(element);
                    }
                }
            }

            for (; index < sortedElements.Count; index++)
            {
                EffectElement? element = sortedElements[index];
                Item item = cfg.GetItemByKey((uint)element.Value.ItemId);

                Transform node = index < materialList.childCount ? materialList.GetChild(index) : UnityEngine.Object.Instantiate(materialList.GetChild(0), materialList);
                Image icon = FindComponent<Image>(node, "Icon/Icon");
                Image quality = FindComponent<Image>(node, "Icon/Quality");
                TMP_Text name = FindComponent<TMP_Text>(node, "Label_Name");
                TMP_Text count = FindComponent<TMP_Text>(node, "Label_Num");

                node.gameObject.SetActive(true);
                UIUtil.SetIconImageSquare(icon, item.Icon);

                long haveCount = pack.GetItemCountByTID((uint)element.Value.ItemId);
                long needCount = (long)element.Value.Value;

                quality.color = ColorUtil.GetColorByItemQuality(item.Quality);
                name.text = TableUtil.GetItemName(element.Value.ItemId);
                if (haveCount < needCount)
                    count.text = string.Format("<color=#ff0000>{0}</color>/{1}", haveCount, needCount);
                else
                    count.text = string.Format("<color=#00ff00>{0}</color>/{1}", haveCount, needCount);
            }
        }
        for (; index < materialList.childCount; index++)
        {
            materialList.GetChild(index).gameObject.SetActive(false);
        }
    }

    #endregion

    #region Tip的技能部分



    #endregion

    #region Tip的MOD部分

    /// <summary>
    /// 布局蓝图的MOD部分
    /// </summary>
    /// <param name="tid">蓝图的TID</param>
    /// <param name="materialNode">MOD节点</param>
    private void LayoutBlueprintModList(uint tid, Transform modNode)
    {
        CfgEternityProxy cfg = GetConfig();

        Produce produce = cfg.GetProduceByKey(tid);

        Item item = cfg.GetItemByKey((uint)produce.ProductId);
        Category itemType = ItemTypeUtil.GetItemType(item.Type).MainType;

        int modCount = 0;

        if (itemType == Category.Warship)
        {
            Warship ship = cfg.GetWarshipByKey(item.Id);
            ModPosition modPosCfg = cfg.GetModPosition((uint)ship.ModPosition);
            modCount = modPosCfg.PositionsLength;
        }
        else if (itemType == Category.Weapon)
        {
            Weapon weapon = cfg.GetWeapon(item.Id);
            ModPosition modPosCfg = cfg.GetModPosition((uint)weapon.ModPosition);
            modCount = modPosCfg.PositionsLength;
        }

        LayoutModList(new int[modCount], modNode);
    }

    /// <summary>
    /// 布局飞船或武器的MOD部分
    /// </summary>
    /// <param name="itemContainer">武器数据</param>
    /// <param name="modNode">MOD节点</param>
    private void LayoutShipOrWeaponModList(ItemContainer itemContainer, EquipmentModL1 type, Transform modNode)
    {
        CfgEternityProxy cfg = GetConfig();

        uint containerSizeA = 0;
        uint containerSizeB = 0;

        ItemContainer containerA = null;
        ItemContainer containerB = null;

        if (itemContainer != null && itemContainer.Items != null)
        {
            string containerKeyA = Category.EquipmentMod.ToString() + type.ToString() + EquipmentModL2.General.ToString();
            string containerKeyB = Category.EquipmentMod.ToString() + type.ToString() + EquipmentModL2.Exclusively.ToString();

            foreach (ItemBase item in itemContainer.Items.Values)
            {
                if (item is ItemContainer)
                {
                    ItemContainer container = item as ItemContainer;

                    if (container.ContainerType.Equals(containerKeyA))
                    {
                        containerA = container;
                        containerSizeA = container.CurrentSizeMax;
                    }
                    else if (container.ContainerType.Equals(containerKeyB))
                    {
                        containerB = container;
                        containerSizeB = container.CurrentSizeMax;
                    }
                }
            }
        }

        int[] qualitys = new int[containerSizeA + containerSizeB];
        if (containerA != null && containerA.Items != null)
        {
            foreach (ItemBase mod in containerA.Items.Values)
            {
                qualitys[mod.Position] = cfg.GetItemByKey(mod.TID).Quality;
            }
        }
        if (containerB != null && containerB.Items != null)
        {
            foreach (ItemBase mod in containerB.Items.Values)
            {
                qualitys[containerSizeA + mod.Position] = cfg.GetItemByKey(mod.TID).Quality;
            }
        }

        LayoutModList(qualitys, modNode);
    }

    /// <summary>
    /// 布局MOD列表
    /// </summary>
    /// <param name="qualitys">品质列表</param>
    /// <param name="modNode">MOD节点</param>
    private void LayoutModList(int[] qualitys, Transform modNode)
    {
        int index = 0;
        Transform modList = FindComponent<Transform>(modNode, "Content");
        for (; index < qualitys.Length; index++)
        {
            Transform mod = index < modList.childCount ? modList.GetChild(index) : UnityEngine.Object.Instantiate(modList.GetChild(0), modList);
            Image modIcon = FindComponent<Image>(mod, "Image_Mod");
            mod.gameObject.SetActive(true);
            if (qualitys[index] != 0)
            {
                modIcon.gameObject.SetActive(true);
                modIcon.color = ColorUtil.GetColorByItemQuality(qualitys[index]);
            }
            else
            {
                modIcon.gameObject.SetActive(false);
            }
        }
        for (; index < modList.childCount; index++)
        {
            modList.GetChild(index).gameObject.SetActive(true);
        }
    }

    #endregion
}
