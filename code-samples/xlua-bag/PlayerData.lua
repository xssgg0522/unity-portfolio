PlayerData={}
--目前只做背包功能 只需要道具信息即可
PlayerData.equipts={}
PlayerData.items={}
PlayerData.gems={}

function PlayerData:Init()
    --道具信息 不管本地还是服务器 都不会把所有信息存进去
    --只会存道具ID和数量

    --这里就是写死了玩家的物品 方便测试，以后的就在服务器上读取
    table.insert(self.equipts,{id=1,num=1})
    table.insert(self.equipts,{id=2,num=1})

    table.insert(self.items,{id=3,num=50})
    table.insert(self.items,{id=4,num=20})

    table.insert(self.gems,{id=5,num=99})
    table.insert(self.gems,{id=6,num=88})
end
PlayerData:Init()
print(PlayerData.equipts[1].id)