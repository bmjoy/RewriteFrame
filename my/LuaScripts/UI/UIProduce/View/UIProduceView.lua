--[[
  预设体  UIProducepanel
  ---lua 是按顺序执行的
]]
local UIProduceView = BaseClass("UIProduceView", UIBaseView)
local base = UIBaseView

--各个组件
local title_text_path = "ContentRoot/Bg/Text"
local produce_Btn_path = "ContentRoot/ProduceBtn"
local Content_path = "ContentRoot/Scroll View/Viewport/Content"
local produnum_input_path = "ContentRoot/InputField"
local icon_image_path = "ContentRoot/Icon"
local close_Btn_path = "ContentRoot/CloseBtn"
---预设体路径
local produce_element_path = "UI/Prefabs/Element/ProduceElement"

local function ClickOnProduceBtn(self)
    Logger.Log("开始生产")
end

local function ClickOnCloseBtn(self)
    Logger.Log("Close")
    SingleGet.UIManager():CloseWindow(UIWindowNames.UIProduce)
end

local function OnCreate(self)
    base.OnCreate(self)
    self.title_text = self:AddComponent(UIText, title_text_path)
    self.produce_Btn = self:AddComponent(UIButton, produce_Btn_path)
    -- self.Content = self:AddComponent(UIText, Content_path)
    --Logger.Log(self.gameObject:GetComponent("UILogin").panelname)
    --self.Content = self.transform:Find(Content_path)
    self.Content = self:GetChildTransform(Content_path)
    Logger.Log(self.Content.localPosition)
    Logger.Log(self.Content.name)
    self.produnum_input = self:AddComponent(UIInput, produnum_input_path)
    self.icon_image = self:AddComponent(UIImage, icon_image_path)
    self.close_Btn = self:AddComponent(UIButton, close_Btn_path)

    self.icon_image:SetSpriteNameBySpriteAtlas("UI/SpriteAtlas/Role.spriteatlas", "ui_role_02")
    self.produce_Btn:SetOnClick(self, ClickOnProduceBtn)
    self.close_Btn:SetOnClick(self, ClickOnCloseBtn)

    --ClickOnProduceBtn必须写在上面
    -- self.produce_Btn:SetOnClick(
    --     function()
    --         Logger.LogError("sss")
    --     end
    -- )
end
local function OnEnable(self)
    base.OnEnable(self)
    self:OnRefresh()
    Logger.Log("OnEnable")
    for i = 1, 30 do
        SingleGet.GameObjectPool():GetGameObjectAsync(
            AssetaddressConfig.ProduceElement,
            function(go)
                local trans = go.transform
                trans:SetParent(self.Content)
                trans.localScale = Vector3.New(1, 1, 1)
                trans.localPosition = Vector3.New(0, 0, 0)
                go:SetActive(true)

                if IsNull(go) then
                    return
                end

                --Logger.LogError()
            end
        )
    end

    --self.transform.localPosition=Vector3.New(220,11,5);
end

local function OnRefresh(self)
    Logger.Log("OnRefresh")
    -- 各组件刷新
end

local function OnDestroy(self)
    self.title_text = nil
    -- self.produce_Btn =  nil
    self.Content = nil
    self.produnum_input = nil
    self.produce_Btn = nil
    base.OnDestroy(self)
end

local function OnAddListener(self)
    base.OnAddListener(self)
    -- UI消息注册
    --self:AddUIListener(UIMessageNames.UILOGIN_ON_SELECTED_SVR_CHG, OnRefreshServerInfo)
end

local function OnRemoveListener(self)
    base.OnRemoveListener(self)
    -- UI消息注销
    --self:RemoveUIListener(UIMessageNames.UILOGIN_ON_SELECTED_SVR_CHG, OnRefreshServerInfo)
end

UIProduceView.OnCreate = OnCreate
UIProduceView.OnEnable = OnEnable

UIProduceView.OnRefresh = OnRefresh
UIProduceView.OnAddListener = OnAddListener
UIProduceView.OnRemoveListener = OnRemoveListener
UIProduceView.OnDestroy = OnDestroy

return UIProduceView
