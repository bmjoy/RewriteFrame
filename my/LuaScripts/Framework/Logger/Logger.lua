--[[
-- added by wsh @ 2017-11-30
-- Logger系统：Lua中所有错误日志输出均使用本脚本接口，以便上报服务器
--]]
---@class Logger
local Logger = BaseClass("Logger")

local function Log(msg)
    if Config.Debug then
        --能定位
        --CS.UnityEngine.Debug.Log(msg)
        --CS.Logger.Log(debug.traceback(msg,2))
        print(debug.traceback(msg, 2))
    else
        print(debug.traceback(msg, 2))
    end
end

local function LogError(msg)
    if Config.Debug then
        --CS.Logger.LogError(debug.traceback(msg,2))
        --luaIdePrintWarn(debug.traceback(msg))
        CS.Logger.LogError(debug.traceback(msg))
        print(debug.traceback(msg, 2))
    else
        --CS.UnityEngine.Debug.LogError(msg)
        CS.Logger.LogError(debug.traceback(msg))
    end
end

-- 重定向event错误处理函数
event_err_handle = function(msg)
    LogError(msg)
end

Logger.Log = Log
Logger.LogError = LogError
Logger.Sign = "<<<<<<<<---Test--->>>>>>>"
return Logger
