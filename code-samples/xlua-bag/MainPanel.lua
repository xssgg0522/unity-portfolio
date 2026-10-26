--只要是一个新的对象（面板） 就新建一张表
BasePanel:subClass("MainPanel")
--可以在实例化之前先声明一个成员变量
--但其实写不写都一样 因为lua里面可以直接写出来用就行

--接下来要做的就是实例化面板对象
--然后为这个面板 处理对应的逻辑

--初始化面板 实例化对象 控件事件判断
function MainPanel:Init(name)
    self.base.Init(self,name)
    --让事件只添加一次
    if self.isInitEvent==false then
        print(self:GetControl("btnBag","Button"))
        self:GetControl("btnBag","Button").onClick:AddListener(
        function()
            self:OnClickBag()
        end)
        self.isInitEvent=true
    end
    
   
end

function MainPanel:OnClickBag()
    BagPanel:ShowMe("BagPanel")
end