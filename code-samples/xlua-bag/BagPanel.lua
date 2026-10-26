--一个面板对应一个表
BasePanel:subClass("BagPanel")

--存当前显示的格子
BagPanel.items={}

BagPanel.nowTye=-1
function BagPanel:Init(name)
    self.base.Init(self,name)
    if self.isInitEvent==false then
        --找到没有挂在ui脚本的控件
       self.content=self:GetControl("sVBag","ScrollRect").transform:Find("ViewPort"):Find("Content")
       self.atlas=ABMgr:LoadRes("ui","Icon",typeof(SpriteAtlas))

       self:GetControl("btnClose","Button").onClick:AddListener(
            function()
                self:HideMe()
            end
        )
        
        --切页签toggle
        self:GetControl("togZB","Toggle").onValueChanged:AddListener(
            function(isOn)
                if isOn then
                    --切页签
                    self:ChangeType(1)
                end
            end
        )

        self:GetControl("togDJ","Toggle").onValueChanged:AddListener(
            function(isOn)
                if isOn then
                    --切页签
                    self:ChangeType(2)
                end
            end
        )

        self:GetControl("togBS","Toggle").onValueChanged:AddListener(
            function(isOn)
                if isOn then
                    --切页签
                    self:ChangeType(3)
                end
            end
        )
        self.isInitEvent=true
    end

    self.panelObj:SetActive(false) -- 加载完后立刻隐藏
end



function BagPanel:ShowMe(name)
self.base.ShowMe(self,name)
    if self.nowTye==-1 then
        self:ChangeType(1)
    end
end


--tog逻辑处理函数
--1.装备 2。道具 3.宝石
function BagPanel:ChangeType(a)
    --1.先创建格子预设体
    if self.nowTye==a then
        return
    end
    local itemList=nil
    --切页 要根据玩家信息来进行格子的创建

    --更新之前 应该要删除或者把当前的格子存起来
    for i = 1, #self.items do
        self.items[i]:Destroy()
    end
    self.items={}
    --再根据当前的页签创建不同的格子
    --根据传入的int来创建格子
    if a==1 then
    itemList=PlayerData.equipts
    elseif a==2 then
        itemList=PlayerData.items
    else
        itemList=PlayerData.gems   
    end

    --遍历玩家数据创建格子
    for i = 1, #itemList do
        --根据数据 创建格子对象
        local grid=ItemGrid:new()
        --实例化对象 设置位置
        grid:Init(self.content,(i-1)%4*170,math.floor(((i-1)/4))*-170)
        --初始化格子的信息 数量和图标
        grid:InitData(itemList[i])

        table.insert(self.items,grid)
    end
    self.nowType = a
end
