--用到之前的知识 Object
--生成一个table 击沉gObject 主要目的是要他实现里面 继承的方法
Object:subClass("ItemGrid")
--成员变量
ItemGrid.obj=nil
ItemGrid.dj=nil
ItemGrid.count=nil

--成员函数
--实例化格子对象
function ItemGrid:Init(father,posX,posY)
    --加载格子预制体和图集
    self.obj=ABMgr:LoadRes("ui","ItemGrid",typeof(GameObject))
    self.atlas=ABMgr:LoadRes("ui","Icon",typeof(SpriteAtlas))
    --设置预制体父对象 把父对象最为参数传进来
    self.obj.transform:SetParent(father,false)
    --把x和y的位置也作为参数传进来
    self.obj.transform.localPosition=Vector3(posX,posY,0)
    --根据玩家数据设置图片和数量
    --找控件
    self.dj=self.obj.transform:Find("dj"):GetComponent(typeof(Image))
    self.count=self.obj.transform:Find("count"):GetComponent(typeof(Text))
end

--初始化格子信息
--data就是外部得到的包含id的玩家数据
function ItemGrid:InitData(data)

    --通过id找到 道具配置表 在得到图标信息
    local num=data.num 
    local data=ItemData[data.id]
    local strs=string.split(data.icon,"_")
    self.dj.sprite=self.atlas:GetSprite(strs[2])

    --设置数量
    self.count.text=num

end


--加自己的逻辑
function ItemGrid:Destroy()
    GameObject.Destroy(self.obj)
    self.obj=nil
end