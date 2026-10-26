using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

/// <summary>
/// 客户端 AB 资源更新管理器。
/// 负责：下载版本清单、读取本地清单、MD5 对比、下载差异包、回写清单、清理旧包。
/// 说明：这是资源热更新，不包含 AB 加载逻辑。
/// </summary>
public class ABUpdateManager : MonoBehaviour
{
    // 公开仓库中不保留真实 FTP 地址与账号，运行时请从配置读取。
    private const string FtpHost = "ftp://your-ftp-host";
    private const string FtpUser = "your-user";
    private const string FtpPassword = "your-password";

    private static ABUpdateManager instance;

    public static ABUpdateManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("ABUpdateManager");
                instance = obj.AddComponent<ABUpdateManager>();
            }
            return instance;
        }
    }

    // key = AB 包名
    private readonly Dictionary<string, ABInfo> remoteABInfo = new Dictionary<string, ABInfo>();
    private readonly Dictionary<string, ABInfo> localABInfo = new Dictionary<string, ABInfo>();
    private readonly List<string> downLoadList = new List<string>();

    /// <summary>
    /// 下载远端版本清单，最多重试 5 次。
    /// </summary>
    public async void DownLoadABCompare(UnityAction<bool> overCallBack)
    {
        bool isOver = false;
        int retry = 5;
        string remoteName = "ABCompare.txt";
        string localTempPath = Application.persistentDataPath + "/ABCompare_TMP.txt";

        while (!isOver && retry > 0)
        {
            await Task.Run(() =>
            {
                isOver = DownLoadFile(remoteName, localTempPath);
            });
            retry--;
        }

        overCallBack?.Invoke(isOver);
    }

    /// <summary>
    /// 更新总流程。
    /// </summary>
    public void CheckUpdate(UnityAction<bool> overCallBack, UnityAction<string> updateInfoCellBack)
    {
        remoteABInfo.Clear();
        localABInfo.Clear();
        downLoadList.Clear();

        DownLoadABCompare((isOver) =>
        {
            updateInfoCellBack("开始更新资源");
            if (!isOver)
            {
                updateInfoCellBack("版本清单下载失败");
                overCallBack(false);
                return;
            }

            updateInfoCellBack("对比文件下载结束");
            string remoteInfo = File.ReadAllText(Application.persistentDataPath + "/ABCompare_TMP.txt");
            updateInfoCellBack("解析远端对比文件");
            GetRemoteABCompareFileInfo(remoteInfo, remoteABInfo);
            updateInfoCellBack("解析远端对比文件完成");

            GetLocalABCopmpareFileInfo((localOk) =>
            {
                if (!localOk)
                {
                    overCallBack(false);
                    return;
                }

                updateInfoCellBack("解析本地对比文件完成");
                updateInfoCellBack("开始对比远端和本地的AB包信息");

                // 新增 / 变更：远端有，本地没有或 MD5 不同。
                foreach (string abName in remoteABInfo.Keys)
                {
                    if (!localABInfo.ContainsKey(abName))
                    {
                        downLoadList.Add(abName);
                    }
                    else
                    {
                        if (localABInfo[abName].md5 != remoteABInfo[abName].md5)
                        {
                            downLoadList.Add(abName);
                        }

                        // 匹配过的本地项移除，最后剩下的就是远端已下线的旧包。
                        localABInfo.Remove(abName);
                    }
                }

                updateInfoCellBack("对比完成");

                // 删除远端已下线的旧 AB。
                foreach (string abName in localABInfo.Keys)
                {
                    string oldPath = Application.persistentDataPath + "/" + abName;
                    if (File.Exists(oldPath))
                    {
                        File.Delete(oldPath);
                    }
                }

                updateInfoCellBack("下载和更新AB包文件");
                DownLoadABFile((downloadOk) =>
                {
                    if (downloadOk)
                    {
                        updateInfoCellBack("更新本地AB包对比文件为最新");
                        File.WriteAllText(
                            Application.persistentDataPath + "/ABCompareInfo.txt",
                            remoteInfo
                        );
                    }

                    overCallBack(downloadOk);
                }, updateInfoCellBack);
            });
        });
    }

    /// <summary>
    /// 解析清单：name size md5|name size md5|...
    /// </summary>
    public void GetRemoteABCompareFileInfo(string info, Dictionary<string, ABInfo> target)
    {
        if (string.IsNullOrWhiteSpace(info))
        {
            return;
        }

        string[] records = info.Split('|');
        for (int i = 0; i < records.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(records[i]))
            {
                continue;
            }

            string[] fields = records[i].Split(' ');
            if (fields.Length < 3)
            {
                continue;
            }

            target[fields[0]] = new ABInfo(fields[0], fields[1], fields[2]);
        }
    }

    /// <summary>
    /// 下载待更新 AB，最多 5 轮重试。
    /// </summary>
    public async void DownLoadABFile(UnityAction<bool> overCallback, UnityAction<string> updatePro)
    {
        string localDir = Application.persistentDataPath + "/";
        int retry = 5;
        int downloaded = 0;
        int total = downLoadList.Count;
        List<string> successList = new List<string>();

        while (downLoadList.Count > 0 && retry > 0)
        {
            for (int i = 0; i < downLoadList.Count; i++)
            {
                string abName = downLoadList[i];
                bool isOver = false;

                await Task.Run(() =>
                {
                    isOver = DownLoadFile(abName, localDir + abName);
                });

                if (isOver)
                {
                    downloaded++;
                    updatePro($"{downloaded}/{total}");
                    successList.Add(abName);
                }
            }

            // 成功的从待下载列表移除，失败的留到下一轮。
            for (int i = 0; i < successList.Count; i++)
            {
                downLoadList.Remove(successList[i]);
            }
            successList.Clear();
            retry--;
        }

        overCallback(downLoadList.Count == 0);
    }

    /// <summary>
    /// 用 FTP 下载单个文件。阻塞 IO，调用方应放在 Task.Run 中。
    /// </summary>
    private bool DownLoadFile(string fileName, string localPath)
    {
        try
        {
            string platform =
#if UNITY_IOS
                "IOS";
#elif UNITY_ANDROID
                "Android";
#else
                "PC";
#endif

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(
                FtpHost + "/AB/" + platform + "/" + fileName
            );
            request.Credentials = new NetworkCredential(FtpUser, FtpPassword);
            request.Proxy = null;
            request.KeepAlive = false;
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.UseBinary = true;

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            using (Stream networkStream = response.GetResponseStream())
            using (FileStream fileStream = File.Create(localPath))
            {
                byte[] buffer = new byte[2048];
                int length = networkStream.Read(buffer, 0, buffer.Length);

                while (length > 0)
                {
                    fileStream.Write(buffer, 0, length);
                    length = networkStream.Read(buffer, 0, buffer.Length);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning(fileName + " 下载失败: " + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// 读取本地清单：优先 persistentDataPath，首次启动回退 StreamingAssets。
    /// </summary>
    public void GetLocalABCopmpareFileInfo(UnityAction<bool> overCallBack)
    {
        if (File.Exists(Application.persistentDataPath + "/ABCompareInfo.txt"))
        {
            StartCoroutine(GetLocalABCompareFIleInfo(
                "file:///" + Application.persistentDataPath + "/ABCompareInfo.txt",
                overCallBack
            ));
            return;
        }

#if UNITY_ANDROID
        // Android 的 StreamingAssets 在 APK 内，不能直接用 File.Exists，交给 UnityWebRequest 读取。
        string streamingPath = Application.streamingAssetsPath;
        StartCoroutine(GetLocalABCompareFIleInfo(
            streamingPath + "/ABCompareInfo.txt",
            overCallBack
        ));
#else
        if (File.Exists(Application.streamingAssetsPath + "/ABCompareInfo.txt"))
        {
            StartCoroutine(GetLocalABCompareFIleInfo(
                "file:///" + Application.streamingAssetsPath + "/ABCompareInfo.txt",
                overCallBack
            ));
            return;
        }

        // 第一次安装且没有首包清单：按空本地清单处理，远端资源全部下载。
        overCallBack(true);
#endif
    }

    private IEnumerator GetLocalABCompareFIleInfo(string filePath, UnityAction<bool> overCallBack)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(filePath))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                GetRemoteABCompareFileInfo(request.downloadHandler.text, localABInfo);
                overCallBack(true);
            }
            else
            {
                overCallBack(false);
            }
        }
    }

    private void OnDestroy()
    {
        instance = null;
    }

    /// <summary>
    /// AB 包元数据。
    /// </summary>
    public class ABInfo
    {
        public string name;
        public long size;
        public string md5;

        public ABInfo(string name, string size, string md5)
        {
            this.name = name;
            this.size = long.Parse(size);
            this.md5 = md5;
        }
    }
}
