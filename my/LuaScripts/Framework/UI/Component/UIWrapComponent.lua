--[[
-- added by wsh @ 2017-12-11
-- Lua侧带有可复用功能的UI组件，UIWrapGroup需要使用到
-- 注意：
-- 1、这里只是作为一个接口文件来使用，具体功能需要自行继承并实现
--]]
---@class UIWrapComponent :UIBaseContainer
local UIWrapComponent = BaseClass("UIWrapComponent", UIBaseContainer)
---@return UIBaseContainer
local base = UIBaseContainer

-- 组件被复用时回调该函数，执行组件的刷新
function UIWrapComponent:OnRefresh(real_index, check)
end

-- 组件添加了按钮组，则按钮被点击时回调该函数
function UIWrapComponent:OnClick(toggle_btn, real_index, check)
end

return UIWrapComponent
