using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ShortVideoStoryPipeline
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    internal sealed class MainForm : Form
    {
        private readonly string appRoot;
        private readonly TextBox logBox;
        private readonly Label statusLabel;
        private readonly Button setupButton;
        private readonly Button editButton;
        private readonly Button runButton;
        private readonly Button checkButton;

        public MainForm()
        {
            appRoot = AppDomain.CurrentDomain.BaseDirectory;

            Text = "多平台故事素材流水线";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(860, 620);
            Size = new Size(980, 700);
            Font = new Font("Microsoft YaHei UI", 10F);
            BackColor = Color.FromArgb(248, 250, 252);

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 132,
                BackColor = Color.FromArgb(30, 41, 59)
            };
            Controls.Add(header);

            var title = new Label
            {
                Text = "多平台故事素材流水线",
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei UI", 22F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(28, 24)
            };
            header.Controls.Add(title);

            var subtitle = new Label
            {
                Text = "下载公开短视频，抽取音频，本地转写，并生成可用于分析和原创改编的故事卡。",
                ForeColor = Color.FromArgb(203, 213, 225),
                Font = new Font("Microsoft YaHei UI", 10.5F),
                AutoSize = true,
                Location = new Point(32, 78)
            };
            header.Controls.Add(subtitle);

            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(22),
                BackColor = Color.FromArgb(248, 250, 252)
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            Controls.Add(content);

            var left = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            content.Controls.Add(left, 0, 0);

            setupButton = CreateActionButton("1  安装或修复工具", "首次使用先点这里，自动下载 yt-dlp、FFmpeg、whisper.cpp 和模型。", 0);
            editButton = CreateActionButton("2  编辑视频链接", "打开 urls.txt，一行粘贴一个公开视频链接。", 92);
            runButton = CreateActionButton("3  开始处理素材", "下载、转写并生成故事卡。", 184);
            checkButton = CreateActionButton("检查运行环境", "确认本地工具和模型是否齐全。", 276);

            left.Controls.Add(setupButton);
            left.Controls.Add(editButton);
            left.Controls.Add(runButton);
            left.Controls.Add(checkButton);

            var openRawButton = CreateSmallButton("打开下载视频", 370);
            var openCardsButton = CreateSmallButton("打开故事卡", 420);
            var readmeButton = CreateSmallButton("打开说明文档", 470);
            left.Controls.Add(openRawButton);
            left.Controls.Add(openCardsButton);
            left.Controls.Add(readmeButton);

            statusLabel = new Label
            {
                Text = "就绪。建议第一次先安装或修复工具。",
                ForeColor = Color.FromArgb(71, 85, 105),
                AutoSize = false,
                Location = new Point(0, 536),
                Size = new Size(280, 52)
            };
            left.Controls.Add(statusLabel);

            var right = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            content.Controls.Add(right, 1, 0);

            var logTitle = new Label
            {
                Text = "运行日志",
                Font = new Font("Microsoft YaHei UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                AutoSize = true,
                Location = new Point(18, 16)
            };
            right.Controls.Add(logTitle);

            logBox = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.FromArgb(226, 232, 240),
                Font = new Font("Consolas", 10F),
                Location = new Point(18, 54),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Size = new Size(600, 548)
            };
            right.Controls.Add(logBox);
            right.Resize += delegate
            {
                logBox.Size = new Size(right.ClientSize.Width - 36, right.ClientSize.Height - 72);
            };

            setupButton.Click += delegate { RunPowerShell("setup-tools.ps1", "正在安装或修复工具..."); };
            editButton.Click += delegate { EditUrls(); };
            runButton.Click += delegate { RunPowerShell("run.ps1", "正在处理素材..."); };
            checkButton.Click += delegate { RunPowerShell("check-tools.ps1", "正在检查运行环境..."); };
            openRawButton.Click += delegate { OpenFolder(Path.Combine(appRoot, "data", "raw")); };
            openCardsButton.Click += delegate { OpenFolder(Path.Combine(appRoot, "data", "cards")); };
            readmeButton.Click += delegate { OpenFile(Path.Combine(appRoot, "README.md")); };

            AppendLog("欢迎使用。支持范围取决于 yt-dlp，例如抖音、TikTok、X/Twitter、小红书、B站、YouTube 等公开链接。");
        }

        private Button CreateActionButton(string title, string description, int top)
        {
            var button = new Button
            {
                Text = title + Environment.NewLine + description,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(0, top),
                Size = new Size(278, 76),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(15, 23, 42),
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold)
            };
            button.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            return button;
        }

        private Button CreateSmallButton(string text, int top)
        {
            var button = new Button
            {
                Text = text,
                Location = new Point(0, top),
                Size = new Size(278, 38),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(30, 41, 59)
            };
            button.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            return button;
        }

        private void EditUrls()
        {
            var target = Path.Combine(appRoot, "urls.txt");
            var example = Path.Combine(appRoot, "urls.example.txt");
            if (!File.Exists(target) && File.Exists(example))
            {
                File.Copy(example, target);
            }
            OpenFile(target);
            AppendLog("已打开 urls.txt。每行放一个公开视频链接，保存后回到这里点击“开始处理素材”。");
        }

        private void OpenFolder(string path)
        {
            Directory.CreateDirectory(path);
            Process.Start("explorer.exe", "\"" + path + "\"");
        }

        private void OpenFile(string path)
        {
            if (!File.Exists(path))
            {
                AppendLog("文件不存在：" + path);
                return;
            }
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }

        private void RunPowerShell(string scriptName, string status)
        {
            var scriptPath = Path.Combine(appRoot, scriptName);
            if (!File.Exists(scriptPath))
            {
                AppendLog("找不到脚本：" + scriptName);
                return;
            }

            SetBusy(true, status);
            AppendLog("");
            AppendLog("开始运行：" + scriptName);

            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-NoProfile -ExecutionPolicy Bypass -File \"" + scriptPath + "\"",
                WorkingDirectory = appRoot,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                CreateNoWindow = true
            };

            var process = new Process { StartInfo = psi, EnableRaisingEvents = true };
            process.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e)
            {
                if (e.Data != null) AppendLog(e.Data);
            };
            process.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs e)
            {
                if (e.Data != null) AppendLog(e.Data);
            };
            process.Exited += delegate
            {
                var code = process.ExitCode;
                BeginInvoke((Action)(() =>
                {
                    AppendLog("运行结束，退出码：" + code);
                    SetBusy(false, code == 0 ? "完成。" : "运行失败，请查看日志。");
                    process.Dispose();
                }));
            };

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                AppendLog("启动失败：" + ex.Message);
                SetBusy(false, "启动失败。");
            }
        }

        private void SetBusy(bool busy, string status)
        {
            setupButton.Enabled = !busy;
            editButton.Enabled = !busy;
            runButton.Enabled = !busy;
            checkButton.Enabled = !busy;
            statusLabel.Text = status;
        }

        private void AppendLog(string message)
        {
            if (logBox.InvokeRequired)
            {
                logBox.BeginInvoke((Action)(() => AppendLog(message)));
                return;
            }
            logBox.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message + Environment.NewLine);
        }
    }
}
