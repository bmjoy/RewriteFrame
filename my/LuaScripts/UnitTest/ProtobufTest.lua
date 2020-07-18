local pb = require "pb"
local bytes
function InitPB()
    --初始化pb文件，路径采用绝对路径
    assert(pb.loadfile "Assets/LuaScripts/Net/Proto/protocol_c2s.pb")
end
local function Encoder()
    local data = {
        UserName = "lisi",
        Password = "mima123",
        IDList = {2, 3}
    }
    bytes = assert(pb.encode("cs.CSLoginReq", data))
    local data2 = {
        TypeId = 101,
        Data = bytes
    }
    -- local data3 = {
    --     UserName = "dfdfdf",
    --     Password = "666"
    -- }
    -- bytes = assert(pb.encode("cs.CSLoginReq", data3))
    -- ---压缩
    bytes = assert(pb.encode("cs.CSMessage", data2))
    Logger.Log(pb.tohex(bytes))
    return bytes
end

function Decoder(pb_data)
    -- and decode the binary data back into lua table
    local data2 = assert(pb.decode("cs.CSMessage", pb_data))
    local datae = assert(pb.decode("cs.CSLoginReq", data2.Data))
    ---解压
    Logger.Log(data2.TypeId)
    Logger.Log(datae.UserName)
    Logger.Log(#datae.IDList)
end

local function Run()
    Logger.Log("------------Test lua_PB------------")

    InitPB()

    local pb_data = Encoder()
    Decoder(pb_data)
end

return {
    Run = Run
}
