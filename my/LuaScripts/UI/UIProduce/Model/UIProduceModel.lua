--[[
-- added by wsh @ 2017-12-01
-- 函数每次在窗口打开时调用，可传递参数用来初始化Model
--]]
---@class UIProduceModel:UIBaseModel
local UIProduceModel = BaseClass("UIProduceModel", UIBaseModel)
---@return UIBaseModel
local base = UIBaseModel

-- 创建
local function OnCreate(self)
    base.OnCreate(self)
    -- 窗口生命周期内保持的成员变量放这
end

-- 打开
local function OnEnable(self)
    base.OnEnable(self)

    self:OnRefresh()
end

-- 刷新全部数据
local function OnRefresh(self)
end

--
local function OnAddListener(self)
    base.OnAddListener(self)
    --self:AddDataListener(DataMessageNames.ON_LOGIN_SERVER_ID_CHG, OnSelectedSvrChg)
end

local function OnRemoveListener(self)
    base.OnRemoveListener(self)
    --self:RemoveDataListener(DataMessageNames.ON_LOGIN_SERVER_ID_CHG, OnSelectedSvrChg)
end

-- 关闭
local function OnDisable(self)
    base.OnDisable(self)
end

-- 销毁
local function OnDistroy(self)
    base.OnDistroy(self)
    -- 清理成员变量
end

UIProduceModel.OnCreate = OnCreate
UIProduceModel.OnEnable = OnEnable
UIProduceModel.OnRefresh = OnRefresh
UIProduceModel.OnAddListener = OnAddListener
UIProduceModel.OnRemoveListener = OnRemoveListener
UIProduceModel.OnDisable = OnDisable
UIProduceModel.OnDistroy = OnDistroy

return UIProduceModel
