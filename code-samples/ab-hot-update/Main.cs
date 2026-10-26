using UnityEngine;

/// <summary>
/// 游戏启动入口：触发资源更新，并把进度/结果转给 UI。
/// 实际项目中这里会配合进度条和“开始游戏”按钮。
/// </summary>
public class Main : MonoBehaviour
{
    private void Start()
    {
        ABUpdateManager.Instance.CheckUpdate(
            isOver =>
            {
                if (isOver)
                {
                    Debug.Log("检测更新结束，可以隐藏进度条并进入游戏");
                }
                else
                {
                    Debug.Log("更新失败，提示玩家检查网络或重试");
                }
            },
            info =>
            {
                Debug.Log(info);
            }
        );
    }
}
