# 基于 xLua 的 UI 框架与背包系统

> 用 Lua 元表做面向对象基础，用 BasePanel 统一面板生命周期，用命名规范做控件自动绑定，再把背包做成数据驱动的格子视图。

## 1. 背景

如果每个 UI 面板都自己加载预制体、自己 `Find` 控件、自己处理显示隐藏，代码会越来越散。这个项目把公共部分抽成两层：

- `Object`：Lua 层面的类、继承、实例化
- `BasePanel`：面板加载、显示、隐藏、控件缓存

`MainPanel` 和 `BagPanel` 都继承 `BasePanel`，只写自己的业务逻辑。

![xLua UI 框架结构](../assets/xlua-architecture.svg)

## 2. 框架结构

```text
Object
  └─ BasePanel
       ├─ MainPanel
       └─ BagPanel
            └─ ItemGrid

资源：
  ABMgr → 面板预制体 / ItemGrid 预制体 / Icon 图集

数据：
  ItemData（JSON → id 字典）
  PlayerData（装备 / 道具 / 宝石列表）
```

## 3. Lua 元表 OOP

`Object.lua` 用元表实现两个核心方法：

- `new()`：创建实例，把 `__index` 指向类表
- `subClass(className)`：创建子类表，设置 `base` 指向父类，并设置元表

```lua
Object = {}

function Object:new()
    local obj = {}
    self.__index = self
    setmetatable(obj, self)
    return obj
end

function Object:subClass(className)
    _G[className] = {}
    local obj = _G[className]
    obj.base = self
    self.__index = self
    setmetatable(obj, self)
end
```

`BasePanel:subClass("BagPanel")` 之后，`BagPanel` 就拥有了 `BasePanel` 的方法；调用父类方法用 `self.base.Init(self, name)`。

## 4. BasePanel 生命周期

`BasePanel` 负责：

- `Init(name)`：通过 `ABMgr:LoadRes("ui", name, typeof(GameObject))` 加载面板预制体
- 把面板挂到 `Canvas` 下
- 用 `GetComponentsInChildren(typeof(UIBehaviour))` 一次性拿到所有 UI 控件
- 按名称前缀过滤：`txt / img / sV / tog / btn`
- 用 `GetType().Name` 拿到控件类型，按 `名称 → 类型 → 控件` 存进字典
- `GetControl(name, typeName)`：按名称和类型取控件
- `ShowMe(name)` / `HideMe()`：显示、隐藏

命名规范的作用：新增面板时，只要控件按规范命名，就能被自动扫描到，不需要每个控件写一次 `Find`。

## 5. 背包数据流

`BagPanel` 有三个 Toggle 页签：

- 装备：`PlayerData.equipts`
- 道具：`PlayerData.items`
- 宝石：`PlayerData.gems`

切换页签时：

1. 销毁上一页的 `ItemGrid` 对象
2. 取当前分类的数据列表
3. 按 4 列布局创建格子：`(i - 1) % 4 * 170`、`floor((i - 1) / 4) * -170`
4. 每个格子 `Init` 加载 prefab 和 `Icon` 图集
5. `InitData` 按 `id` 从 `ItemData` 取配置，设置图标和数量

`ItemData` 的加载：

```lua
local txt = ABMgr:LoadRes("json", "ItemData", typeof(TextAsset))
local itemList = Json.decode(txt.text)

ItemData = {}
for _, value in pairs(itemList) do
    ItemData[value.id] = value
end
```

数组转字典后，`ItemGrid` 只依赖 `id` 取数据，配置和表现分离。

## 6. 图标加载

`ItemGrid:InitData`：

```lua
local data = ItemData[data.id]
local strs = string.split(data.icon, "_")
self.dj.sprite = self.atlas:GetSprite(strs[2])
self.count.text = num
```

- 配置里的 `icon` 例如 `icon_1001`
- 按 `_` 拆分，取第二段作为 Sprite 名
- `SpriteAtlas:GetSprite` 从图集取图，减少 DrawCall

## 7. 编辑器工具

`LuaCopyEditor` 做两件事：

1. 把 `Assets/Lua/*.lua` 复制到 `Assets/LuaTxt/*.lua.txt`
2. 给复制后的文件设置 `AssetBundleName = "lua"`

这样 Lua 脚本可以走 AB 包更新，不需要手动拷贝和手动设置标签。

## 8. 设计上的取舍

**做得好的地方**

- `Object → BasePanel` 两层继承，公共逻辑只写一次
- 控件自动绑定，新增面板接入成本低
- 命名规范 + 类型字典，解决同名控件和同物体多脚本的问题
- 背包数据驱动，`ItemGrid` 可复用
- 配置 / 表现分离，JSON 改数据不需要改 UI 代码

**可以继续改进**

- `BagPanel` 里 `nowTye` / `nowType` 拼写不一致，建议统一
- 每个 `ItemGrid` 都加载一次图集，可以在 `BagPanel` 加载后传给格子
- 格子对象可以池化，避免频繁 `Instantiate / Destroy`
- 控件绑定可以加缓存失效和缺失提示
- Lua 脚本热更可以进一步接入版本校验

## 9. 面试一句话

> 这个项目用 Lua 元表实现 Object / BasePanel 两级继承，把面板加载、显示、隐藏和控件绑定收口；背包侧用 JSON 配置转 id 字典，BagPanel 按装备 / 道具 / 宝石三个页签动态重建 ItemGrid，图标从 SpriteAtlas 取，整体是数据驱动的 UI 结构。
