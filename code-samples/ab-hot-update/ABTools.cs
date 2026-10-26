using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor 端 AB 发布工具：
/// 1. 生成 ABCompare.txt（文件名 大小 MD5）
/// 2. 把首包 AB 复制到 StreamingAssets
/// 3. 上传 AB 和清单到 FTP
/// 说明：真正的 AB 打包由 BuildPipeline / AB Browser 完成，这里只负责清单和上传。
/// </summary>
public class ABTools : EditorWindow
{
    private const string FtpUser = "your-user";
    private const string FtpPassword = "your-password";

    private int nowSelIndex = 0;
    private readonly string[] targetString = { "PC", "IOS", "Android" };
    private string serverIP = "ftp://your-ftp-host";

    [MenuItem("AB包工具/打开工具窗口")]
    private static void OpenWindow()
    {
        ABTools window = EditorWindow.GetWindowWithRect(
            typeof(ABTools),
            new Rect(0, 0, 350, 220)
        ) as ABTools;
        window.Show();
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 150, 15), "平台选择");
        nowSelIndex = GUI.Toolbar(
            new Rect(10, 30, 250, 20),
            nowSelIndex,
            targetString
        );

        GUI.Label(new Rect(10, 60, 150, 15), "资源服务器地址");
        serverIP = GUI.TextField(new Rect(10, 80, 250, 20), serverIP);

        if (GUI.Button(new Rect(10, 110, 100, 40), "创建对比文件"))
        {
            CreatABCompareFile();
        }

        if (GUI.Button(new Rect(115, 110, 225, 40), "保存默认资源到StreamingAssets"))
        {
            MoveABToStreamingAssets();
        }

        if (GUI.Button(new Rect(10, 160, 330, 40), "上传AB包和对比文件"))
        {
            UploadAllABFile();
        }
    }

    /// <summary>
    /// 扫描当前平台的 AB 目录，生成版本清单。
    /// </summary>
    public void CreatABCompareFile()
    {
        string abDir = Application.dataPath + "/Arts/AB/" + targetString[nowSelIndex];
        DirectoryInfo dir = Directory.CreateDirectory(abDir);
        FileInfo[] files = dir.GetFiles();

        StringBuilder builder = new StringBuilder();
        foreach (FileInfo file in files)
        {
            // Unity 打出的 AB 包通常没有后缀；.manifest/.txt 跳过。
            if (file.Extension != "")
            {
                continue;
            }

            builder.Append(file.Name)
                .Append(' ')
                .Append(file.Length)
                .Append(' ')
                .Append(GetMDS(file.FullName))
                .Append('|');
        }

        if (builder.Length == 0)
        {
            Debug.LogWarning("当前目录没有可用的 AB 包");
            return;
        }

        builder.Length -= 1; // 去掉最后一个 |
        File.WriteAllText(abDir + "/ABCompare.txt", builder.ToString());
        AssetDatabase.Refresh();
        Debug.Log("AB包对比文件生成成功");
    }

    /// <summary>
    /// 计算文件 MD5，返回 32 位小写十六进制。
    /// </summary>
    public static string GetMDS(string filePath)
    {
        using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        using (MD5 md5 = MD5.Create())
        {
            byte[] hash = md5.ComputeHash(file);
            StringBuilder builder = new StringBuilder(hash.Length * 2);
            for (int i = 0; i < hash.Length; i++)
            {
                builder.Append(hash[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }

    /// <summary>
    /// 把选中的 AB 复制到 StreamingAssets，并生成首包清单。
    /// </summary>
    private void MoveABToStreamingAssets()
    {
        UnityEngine.Object[] selected = Selection.GetFiltered(
            typeof(UnityEngine.Object),
            SelectionMode.DeepAssets
        );
        if (selected.Length == 0)
        {
            return;
        }

        StringBuilder builder = new StringBuilder();
        foreach (UnityEngine.Object asset in selected)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset);
            string fileName = assetPath.Substring(assetPath.LastIndexOf('/'));

            // 只处理没有后缀的 AB 包。
            if (fileName.IndexOf(".", StringComparison.Ordinal) != -1)
            {
                continue;
            }

            AssetDatabase.CopyAsset(assetPath, "Assets/StreamingAssets" + fileName);
            FileInfo info = new FileInfo(Application.streamingAssetsPath + fileName);
            builder.Append(info.Name)
                .Append(' ')
                .Append(info.Length)
                .Append(' ')
                .Append(GetMDS(info.FullName))
                .Append('|');
        }

        if (builder.Length == 0)
        {
            return;
        }

        builder.Length -= 1;
        File.WriteAllText(Application.streamingAssetsPath + "/ABCompareInfo.txt", builder.ToString());
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 上传当前平台目录下所有 AB 和 .txt 清单。
    /// </summary>
    private async void UploadAllABFile()
    {
        string abDir = Application.dataPath + "/Arts/AB/" + targetString[nowSelIndex] + "/";
        DirectoryInfo dir = Directory.CreateDirectory(abDir);
        FileInfo[] files = dir.GetFiles();

        foreach (FileInfo file in files)
        {
            if (file.Extension == "" || file.Extension == ".txt")
            {
                await FtpUploadFile(file.FullName, file.Name);
            }
        }
    }

    /// <summary>
    /// FTP 上传单个文件。
    /// </summary>
    private async Task FtpUploadFile(string filePath, string fileName)
    {
        await Task.Run(() =>
        {
            try
            {
                string url = serverIP + "/AB/" + targetString[nowSelIndex] + "/" + fileName;
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
                request.Credentials = new NetworkCredential(FtpUser, FtpPassword);
                request.Proxy = null;
                request.KeepAlive = false;
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.UseBinary = true;

                using (FileStream file = File.OpenRead(filePath))
                using (Stream uploadStream = request.GetRequestStream())
                {
                    byte[] buffer = new byte[8192];
                    int length = file.Read(buffer, 0, buffer.Length);
                    while (length > 0)
                    {
                        uploadStream.Write(buffer, 0, length);
                        length = file.Read(buffer, 0, buffer.Length);
                    }
                }

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    Debug.Log(fileName + " 上传成功: " + response.StatusDescription);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(fileName + " 上传失败: " + ex.Message);
            }
        });
    }
}
