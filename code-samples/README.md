# 核心代码节选

这里放的是两个框架项目的核心脚本，用于展示实现思路，不是完整 Unity 工程。

完整工程没有直接上传，原因：

- 包含课程提供的场景、美术资源和第三方插件
- 工程体积较大（Library / Temp / 构建产物）
- 代码里原本有 FTP 地址和账号，已在本节选中移除

## 已做的整理

- `ABUpdateManager.cs`
  - 移除运行时脚本里的 `using UnityEditor`
  - FTP 地址、账号、密码改成占位常量
  - 补 `downLoadList.Clear()`，避免重复调用残留
  - 远端清单失败时补 `overCallBack(false)`
  - 修正 `Unity_ANDROID` → `UNITY_ANDROID`
  - 下载改为 `using` 释放响应流 / 文件流
- `ABTools.cs`
  - FTP 地址、账号、密码改成占位常量
  - 上传失败不再在 `async void` 里 `throw`
  - 补充注释，方便阅读
- `xlua-bag/*.lua`
  - 保留课程项目中的核心逻辑
  - 补充少量注释

## 目录

```text
ab-hot-update/
  ABUpdateManager.cs   客户端更新流程
  ABTools.cs           Editor 发布工具
  Main.cs              入口与回调示例

xlua-bag/
  Object.lua           Lua 元表 OOP
  BasePanel.lua        面板生命周期与控件绑定
  MainPanel.lua        主面板，打开背包
  BagPanel.lua         三个页签、格子创建
  ItemGrid.lua         单个格子的图标与数量
  ItemData.lua         JSON 配置转 id 字典
  PlayerData.lua       玩家装备 / 道具 / 宝石数据
```

## 阅读顺序

1. `ab-hot-update/ABUpdateManager.cs`：先看 `CheckUpdate`，再看 `DownLoadABCompare`、`DownLoadABFile`、`DownLoadFile`
2. `ab-hot-update/ABTools.cs`：看 `CreatABCompareFile`、`MoveABToStreamingAssets`、`UploadAllABFile`
3. `xlua-bag/Object.lua` → `BasePanel.lua` → `BagPanel.lua` → `ItemGrid.lua`

> 说明：这些代码来自课程项目复现，仓库中的文档、结构整理和部分修正由我完成。仅用于学习交流。
