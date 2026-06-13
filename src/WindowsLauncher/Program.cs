using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace LingJiClipScribe
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length == 2 && args[0].Equals("--screenshot", StringComparison.OrdinalIgnoreCase))
            {
                using (var form = new MainForm())
                {
                    form.Size = new Size(1040, 740);
                    form.ShowInTaskbar = false;
                    form.StartPosition = FormStartPosition.Manual;
                    form.Location = new Point(-2000, -2000);
                    form.Show();
                    Application.DoEvents();

                    using (var bitmap = new Bitmap(form.Width, form.Height))
                    {
                        form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.Size));
                        bitmap.Save(args[1], System.Drawing.Imaging.ImageFormat.Png);
                    }
                }
                return;
            }

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

            Text = "领记 ClipScribe";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(920, 680);
            Size = new Size(1040, 740);
            Font = new Font("Microsoft YaHei UI", 10F);
            BackColor = Color.FromArgb(248, 250, 252);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                BackColor = Color.FromArgb(248, 250, 252)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 132));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            var header = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Padding = new Padding(32, 22, 32, 18),
                BackColor = Color.FromArgb(30, 41, 59)
            };
            header.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            header.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.Controls.Add(header, 0, 0);

            var title = new Label
            {
                Text = "领记 ClipScribe",
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei UI", 22F, FontStyle.Bold),
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 44
            };
            header.Controls.Add(title, 0, 0);

            var subtitle = new Label
            {
                Text = "多平台公开视频转写与故事卡工具。下载、抽音频、转文字、生成创作素材，一步步带你完成。",
                ForeColor = Color.FromArgb(203, 213, 225),
                Font = new Font("Microsoft YaHei UI", 10.5F),
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            header.Controls.Add(subtitle, 0, 1);

            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(22),
                BackColor = Color.FromArgb(248, 250, 252)
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.Controls.Add(content, 0, 1);

            var left = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(0, 0, 18, 0)
            };
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 344));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 158));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            content.Controls.Add(left, 0, 0);

            var actionPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = false,
                Padding = new Padding(0)
            };
            left.Controls.Add(actionPanel, 0, 0);

            setupButton = CreateActionButton("1  安装或修复工具", "首次使用先点这里，自动下载 yt-dlp、FFmpeg、whisper.cpp 和模型。");
            editButton = CreateActionButton("2  编辑视频链接", "打开 urls.txt，一行粘贴一个公开视频链接。");
            runButton = CreateActionButton("3  开始处理素材", "下载、转写并生成故事卡。");
            checkButton = CreateActionButton("检查运行环境", "确认本地工具和模型是否齐全。");

            actionPanel.Controls.Add(setupButton);
            actionPanel.Controls.Add(editButton);
            actionPanel.Controls.Add(runButton);
            actionPanel.Controls.Add(checkButton);

            var utilityPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0, 10, 0, 0)
            };
            left.Controls.Add(utilityPanel, 0, 1);

            var openRawButton = CreateSmallButton("打开下载视频");
            var openCardsButton = CreateSmallButton("打开故事卡");
            var readmeButton = CreateSmallButton("打开说明文档");
            utilityPanel.Controls.Add(openRawButton);
            utilityPanel.Controls.Add(openCardsButton);
            utilityPanel.Controls.Add(readmeButton);

            statusLabel = new Label
            {
                Text = "就绪。建议第一次先安装或修复工具。",
                ForeColor = Color.FromArgb(71, 85, 105),
                AutoSize = false,
                Dock = DockStyle.Fill,
                Padding = new Padding(2, 12, 4, 0),
                TextAlign = ContentAlignment.TopLeft
            };
            left.Controls.Add(statusLabel, 0, 2);

            var right = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(18)
            };
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            content.Controls.Add(right, 1, 0);

            var logTitle = new Label
            {
                Text = "运行日志",
                Font = new Font("Microsoft YaHei UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            right.Controls.Add(logTitle, 0, 0);

            logBox = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.FromArgb(226, 232, 240),
                Font = new Font("Consolas", 10F),
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 10, 0, 0)
            };
            right.Controls.Add(logBox, 0, 1);

            setupButton.Click += delegate { RunPowerShell("setup-tools.ps1", "正在安装或修复工具..."); };
            editButton.Click += delegate { EditUrls(); };
            runButton.Click += delegate { RunPowerShell("run.ps1", "正在处理素材..."); };
            checkButton.Click += delegate { RunPowerShell("check-tools.ps1", "正在检查运行环境..."); };
            openRawButton.Click += delegate { OpenFolder(Path.Combine(appRoot, "data", "raw")); };
            openCardsButton.Click += delegate { OpenFolder(Path.Combine(appRoot, "data", "cards")); };
            readmeButton.Click += delegate { OpenFile(Path.Combine(appRoot, "README.md")); };

            AppendLog("欢迎使用领记 ClipScribe。支持范围取决于 yt-dlp，例如抖音、TikTok、X/Twitter、小红书、B站、YouTube 等公开链接。");
        }

        private Button CreateActionButton(string title, string description)
        {
            var button = new Button
            {
                Text = title + Environment.NewLine + description,
                TextAlign = ContentAlignment.MiddleLeft,
                Size = new Size(292, 72),
                Margin = new Padding(0, 0, 0, 12),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(15, 23, 42),
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold)
            };
            button.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            return button;
        }

        private Button CreateSmallButton(string text)
        {
            var button = new Button
            {
                Text = text,
                Size = new Size(292, 38),
                Margin = new Padding(0, 0, 0, 10),
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
