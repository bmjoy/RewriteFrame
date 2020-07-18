--  please create lue file
---@class ConfigCfgManager:Singleton
local ConfigCfgManager = BaseClass("ConfigManager", Singleton)
---Auto Generated Start---

---@return Hero
function ConfigCfgManager:Hero()
	return reload('Config.Data.Hero')
end

---@return Language
function ConfigCfgManager:Language()
	return reload('Config.Data.Language')
end

---@return Me
function ConfigCfgManager:Me()
	return reload('Config.Data.Me')
end

---@return Server
function ConfigCfgManager:Server()
	return reload('Config.Data.Server')
end

---@return ServerAreaLang
function ConfigCfgManager:ServerAreaLang()
	return reload('Config.Data.ServerAreaLang')
end

---@return ServerLang
function ConfigCfgManager:ServerLang()
	return reload('Config.Data.ServerLang')
end

---@return TestName
function ConfigCfgManager:TestName()
	return reload('Config.Data.TestName')
end
---Auto Generated End---



return ConfigCfgManager
