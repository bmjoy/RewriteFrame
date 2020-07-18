--[[
-- added by wsh @ 2017-12-01
-- UILogin模型层
-- 注意：
-- 1、成员变量预先在OnCreate、OnEnable函数声明，提高代码可读性
-- 2、OnCreate内放窗口生命周期内保持的成员变量，窗口销毁时才会清理
-- 3、OnEnable内放窗口打开时才需要的成员变量，窗口关闭后及时清理
-- 4、OnEnable函数每次在窗口打开时调用，可传递参数用来初始化Model
--]]
---@class UILoginModel:UIBaseModel
local UILoginModel = BaseClass("UILoginModel", UIBaseModel)
---@return UIBaseModel
local base = UIBaseModel

-- 创建
function UILoginModel:OnCreate()
    base.OnCreate(self)
    -- 窗口生命周期内保持的成员变量放这
end

-- 打开
function UILoginModel:OnEnable()
    base.OnEnable(self)
    -- 窗口关闭时可以清理的成员变量放这
    -- 账号
    self.account = nil
    -- 密码
    self.password = nil
    -- 客户端app版本号
    self.client_app_ver = nil
    -- 客户端资源版本号
    self.client_res_ver = nil
    -- 区域名
    self.area_name = nil
    -- 服务器名
    self.server_name = nil

    self:OnRefresh()
end

function UILoginModel:SetServerInfo(select_svr_id)
    local server_data = SingleGet.ServerData()
    local select_svr = server_data.servers[select_svr_id]
    if select_svr ~= nil then
        self.area_name = LangUtil.GetServerAreaName(select_svr.area_id)
        self.server_name = LangUtil.GetServerName(select_svr_id)
    end
end

-- 刷新全部数据
function UILoginModel:OnRefresh()
    local client_data = SingleGet.ClientData()
    self.account = client_data.account
    self.password = client_data.password
    self.client_app_ver = client_data.app_version
    self.client_res_ver = client_data.res_version
    self.SetServerInfo(self, client_data.login_server_id)
end

function UILoginModel:OnSelectedSvrChg(id)
    self.SetServerInfo(self, id)
    self:UIBroadcast(UIMessageNames.UILOGIN_ON_SELECTED_SVR_CHG)
end

-- 监听选服变动
function UILoginModel:OnAddListener()
    base.OnAddListener(self)
    self:AddDataListener(DataMessageNames.ON_LOGIN_SERVER_ID_CHG, self.OnSelectedSvrChg)
end

function UILoginModel:OnRemoveListener()
    base.OnRemoveListener(self)
    self:RemoveDataListener(DataMessageNames.ON_LOGIN_SERVER_ID_CHG, self.OnSelectedSvrChg)
end

-- 关闭
function UILoginModel:OnDisable()
    base.OnDisable(self)
    -- 清理成员变量
    self.account = nil
    self.password = nil
    self.client_app_ver = nil
    self.client_res_ver = nil
    self.area_name = nil
    self.server_name = nil
end

-- 销毁
function UILoginModel:OnDistroy()
    base.OnDistroy(self)
    -- 清理成员变量
end

return UILoginModel
