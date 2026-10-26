
--首先把Json表从AB包中加载出来
--加载的Json文件 TxtAsset对象
local txt=ABMgr:LoadRes("json","ItemData",typeof(TextAsset))
--获取他的文本信息 进行jso 解析
local itemList=Json.decode(txt.text)
--这个加载出来的是一张像数组结构的数据
--不方便我们通过id去查找物品信息
--所以我们要把它转换成一个字典结构
--而且要在任何地方都能够访问到 所以声明成全局变量

--一张用键值对的形式存储道具信息的表
--键存储道具的id 值存储道具的所有信息
ItemData={}
for _, value in pairs(itemList) do
    ItemData[value.id]=value
end

for k, v in pairs(ItemData) do
    print(k,v.name)
end