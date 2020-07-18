-- 全局模块
require "Global.Global"

local AssetBundleManager = CS.AssetBundles.AssetBundleManager.Instance
-- package.cpath =
--     package.cpath ..
--     ";c:/Users/Administrator.XSOOY-20190615H/.vscode/extensions/tangzx.emmylua-0.3.37/debugger/emmy/windows/x64/?.dll"
-- local dbg = require("emmy_core")
-- dbg.tcpListen("localhost", 9966)
-- 定义为全局模块，整个lua程序的入口类
---@class GameMain
GameMain = {}

-- 全局初始化
local function Initilize()
    local loadingAssetbundlePath = "UI/Prefabs/View/UILoading.prefab"

    -- SingleGet.ResourcesManager():CoLoadAssetBundleAsync(loadingAssetbundlePath)
    --  AssetBundleManager:LoadAssetAsync(loadingAssetbundlePath, typeof(GameObject), loadCall)

    SingleGet.ResourcesManager():NewLoadAsync(loadingAssetbundlePath, typeof(GameObject), loadCall)
end
timeCallBack = function(canshu)
    -- Logger.LogError("sfdsfs .... ")
    -- Logger.LogError(canshu)
end

loadCall = function(obj)
    -- Logger.LogError("带参数11 .... ")
    -- Logger.LogError(obj.name)
    -- Logger.LogError(SingleGet.ConfigCfgManager():Hero()[1].name)
    SingleGet.TimerManager():GetTimer(5, timeCallBack, "canshu")
end
callBack = function()
    Logger.LogError("没有参数11.... ")
end
-- 进入游戏
local function EnterGame()
    -- luaide 调试
    -- local breakInfoFun,xpcallFun = require("LuaDebug")("localhost", 7003)
    -- luaide 调试

    -- TODO：服务器信息应该从服务器上拉取，这里读取测试数据
    local ServerData = require "DataCenter.ServerData.ServerData"
    local TestServerData = require "GameTest.DataTest.TestServerData"
    local ClientData = require "DataCenter.ClientData.ClientData"
    SingleGet.ServerData():ParseServerList(TestServerData)
    local selected = SingleGet.ClientData().login_server_id
    if selected == nil or SingleGet.ServerData().servers[selected] == nil then
        SingleGet.ClientData():SetLoginServerID(10001)
    end

    SingleGet.SceneManager():SwitchScene(SceneConfig.LoginScene)
end

--主入口函数。从这里开始lua逻辑
local function Start()
    -- Logger.LogError("GameMain start...")
    Logger.Log("dddddd")
    Logger.Log(CS.TestCS.index .. "===CS>TestCS")
    local loadingAssetbundlePath = "UI/Prefabs/View/UILoading.prefab"
    AssetBundleManager:LoadAssetAsync(loadingAssetbundlePath, typeof(GameObject), loadCall)
    -- AssetBundleManager:LoadAssetAsync3(callBack)
    -- AssetBundleManager:LoadAssetAsync5(loadCall)
    -- AssetBundleManager:LoadAssetAsyncmk()
    -- 模块启动
    SingleGet.UpdateManager():Startup()
    SingleGet.TimerManager():Startup()
    SingleGet.LogicUpdater():Startup()
    SingleGet.UIManager():Startup()
    if Config.Debug then
        -- 单元测试()
        local UnitTest = require "UnitTest.UnitTestMain"
        UnitTest.Run()
    end

    coroutine.start(
        function()
            Initilize()
            EnterGame()
        end
    )
end

-- 场景切换通知
local function OnLevelWasLoaded(level)
    collectgarbage("collect")
    Time.timeSinceLevelLoad = 0
end

local function OnApplicationQuit()
    -- 模块注销
    SingleGet.UpdateManager():Dispose()
    SingleGet.TimerManager():Dispose()
    SingleGet.LogicUpdater():Dispose()
    -- SingleGet.HallConnector():Dispose()
end

-- GameMain公共接口，其它的一律为私有接口，只能在本模块访问
GameMain.Start = Start
GameMain.OnLevelWasLoaded = OnLevelWasLoaded
GameMain.OnApplicationQuit = OnApplicationQuit

return GameMain
