--[[
-- Author: passion
-- Date: 2019-09-20 12:00:54
-- LastEditors: passion
-- LastEditTime: 2019-09-26 13:38:32
-- Description: 大厅网络连接器，socket长连接
--]]
---@class HallConnector :Singleton
local HallConnector = BaseClass("HallConnector", Singleton)
local LoginHandler = require("Net.Handlers.LoginHandler")
---@return Messenger
local HandlersCallBack = Messenger.New() --HandlersCallBack:AddListener(TestMessengerType1, call)
--HandlersCallBack:Broadcast(TestMessengerType1, tal)
local ConnStatus = {
    Init = 0,
    Connecting = 1,
    WaitLogin = 2,
    Done = 3
}

function HallConnector:AddListener(type, call)
    HandlersCallBack:AddListener(type, call)
end
function HallConnector:RemoveListener(type, call)
    HandlersCallBack:RemoveListener(type, call)
end
function HallConnector:__init()
    self.hallSocket = nil
    self.handlers = {}

    local login = LoginHandler.New()
    login:Init(self.handlers)

    self:LoadPB()
end

function HallConnector:LoadPB()
    local pbFiles = {
        "Net/Proto/protocol_c2s.pb"
    }

    --编辑器下和移动端，加载位置不同
    if (CS.GameUtility.GetPlatform() == "Windows") then
        table.walk(
            pbFiles,
            function(i, path)
                print(path .. " pb load success")
                assert(pb.loadfile("Assets/LuaScripts/" .. path))
            end
        )
    else
        table.walk(
            pbFiles,
            function(i, path)
                SingleGet.ResourcesManager():LoadAsync(
                    "Lua/" .. path .. ".bytes",
                    typeof(CS.UnityEngine.TextAsset),
                    function(data)
                        print(path .. " pb load success")
                        assert(pb.load(data.bytes))
                    end
                )
            end
        )
    end
end

--接受数据 原来的写法
-- function HallConnector:OnReceivePackage(receive_bytes)
--     local receive = CS.ByteBuffer(receive_bytes)
--     Logger.Log("收到消息:" .. #receive_bytes)
--     local msg_id = receive:ReadShort()
--     local msg_bytes = receive:ReadBytes()

--     if (self.handlers[msg_id] == nil) then
--         Logger.LogError("msg_id 未绑定函数" .. msg_id)
--         return
--     end

--     Logger.Log("msg_id:" .. msg_id .. " | msg_bytes.len:" .. #msg_bytes)

--     local msg = nil
--     if (msg_bytes ~= nil) then
--         msg = pb.decode(MsgIDMap[msg_id], msg_bytes)
--     end

--     self.handlers[msg_id](msg_id, msg)
-- end
function HallConnector:OnReceivePackage(receive_bytes)
    local receive = CS.ByteBuffer(receive_bytes)
    Logger.Log("收到消息:" .. #receive_bytes)
    local msg_bytes = receive:ReadBytes()
    local msg = nil
    local msgreally = nil
    if (msg_bytes ~= nil) then
        msg = pb.decode("cs.CSMessage", msg_bytes)
        Logger.Log(msg.TypeId)
        HandlersCallBack:Broadcast(msg.TypeId, msg.Data)
    end
    -- self.handlers[msg_id](msg_id, msg)
end

--连接服务器
function HallConnector:Connect(host_ip, host_port, on_connect, on_close)
    if not self.hallSocket then
        self.hallSocket = CS.Networks.HjTcpNetwork()
        self.hallSocket.ReceivePkgHandle = Bind(self, self.OnReceivePackage)
    end
    self.hallSocket.OnConnect = on_connect
    self.hallSocket.OnClosed = on_close
    self.hallSocket:SetHostPort(host_ip, host_port)
    self.hallSocket:Connect()
    Logger.Log("Connect to " .. host_ip .. ", port : " .. host_port)

    return self.hallSocket
end
----原来的写法
-- function HallConnector:SendMessage(msg_id, msg)
--     local send = CS.ByteBuffer()
--     Logger.Log(msg.UserName)

--     if (msg) then
--         local msg_bytes = pb.encode("cs.CSLoginInfo", msg)
--         Logger.Log("send messge msg_bytes" .. msg_id .. "；byte count：" .. #msg_bytes)
--         local heard = {}
--         heard.TypeId = msg_id
--         heard.Data = msg_bytes
--         local msg_really = pb.encode("cs.CSMessage", heard)
--         send:WriteBytes(msg_really)
--         Logger.Log("send messge msg_really" .. msg_id .. "；byte count：" .. #msg_really)
--     end

--     local bytes = send:ToBytes()
--     Logger.Log("send messge：" .. msg_id .. "；byte count：" .. #bytes)

--     self.hallSocket:SendMessage(bytes)
-- end

function HallConnector:SendMessage(msg_id, msg_str, msg)
    local send = CS.ByteBuffer()
    Logger.Log(msg.UserName)

    if (msg) then
        local msg_bytes = pb.encode(msg_str, msg)
        Logger.Log("send messge msg_bytes" .. msg_id .. "；byte count：" .. #msg_bytes)
        local heard = {}
        heard.TypeId = msg_id
        heard.Data = msg_bytes
        local msg_really = pb.encode("cs.CSMessage", heard)
        send:WriteBytes(msg_really)
        Logger.Log("send messge msg_really" .. msg_id .. "；byte count：" .. #msg_really)
    end

    local bytes = send:ToBytes()
    Logger.Log("send messge：" .. msg_id .. "；byte count：" .. #bytes)

    self.hallSocket:SendMessage(bytes)
end

function HallConnector:Update()
    if self.hallSocket then
        self.hallSocket:UpdateNetwork()
    end
end

function HallConnector:Disconnect()
    if self.hallSocket then
        self.hallSocket:Disconnect()
    end
end

function HallConnector:Dispose()
    if self.hallSocket then
        self.hallSocket:Dispose()
    end
    self.hallSocket = nil
end

return HallConnector
