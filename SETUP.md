# 发布到 GitHub Pages

## 1. 创建仓库

在 GitHub 新建一个 **Public** 仓库：

```text
unity-portfolio
```

创建页面这样选：

- **Repository name**：`unity-portfolio`
- **Description**：可填 `Unity 客户端开发作品集：AssetBundle 热更新、xLua UI 框架、UGUI 与玩法 Demo`
- **Visibility**：Public
- **Add README**：**关掉**
- **Add .gitignore**：**None / 没有 .gitignore**
- **Add license**：**None / 无执照**

README、.gitignore 本地都已经有了。如果 GitHub 再自动生成一份，push 时会出现 unrelated histories，需要额外 merge，没必要。

## 2. 检查用户名

仓库里的 GitHub 链接已经配置为：

```text
xssgg0522
```

涉及文件：

- `README.md`
- `index.html`

如果以后改 GitHub 用户名，全局替换 `xssgg0522` 即可。

## 3. 推送到 GitHub

在 `UnityPortfolio` 目录打开终端：

```bash
git init
git add .

# 建议用 GitHub 的 noreply 邮箱，避免 QQ 邮箱进入公开提交历史
# 在 GitHub：Settings → Emails → Keep my email addresses private
# 复制类似 12345678+xssgg0522@users.noreply.github.com 的地址
git config user.name "张浩"
git config user.email "你的noreply邮箱"

git commit -m "init: Unity portfolio"
git branch -M main
git remote add origin https://github.com/xssgg0522/unity-portfolio.git
git push -u origin main
```

### 常见问题

**push 时让你输密码？**

GitHub 现在不接受账号密码。Windows 上一般会弹出 Git Credential Manager，选 “Sign in with your browser”；或者用 Personal Access Token 当密码。

**push 被拒绝，提示 non-fast-forward / unrelated histories？**

说明创建仓库时勾了 README。可以：

```bash
git pull --rebase origin main
git push -u origin main
```

下次创建仓库时把 “Add README” 关掉就不会有这个问题。

**remote origin already exists？**

```bash
git remote set-url origin https://github.com/xssgg0522/unity-portfolio.git
```

## 4. 开启 Pages

仓库页面：

```text
Settings → Pages → Build and deployment
Source: Deploy from a branch
Branch: main
Folder: / (root)
Save
```

等 1–2 分钟，访问：

```text
https://xssgg0522.github.io/unity-portfolio/
```

## 5. 后续补截图/视频

有录屏或截图后，放到：

```text
assets/screenshots/
```

然后在 `README.md` 和 `index.html` 对应项目里加上图片即可。

建议每个项目 1 张主图 + 1 个 GIF，不要放太多。
