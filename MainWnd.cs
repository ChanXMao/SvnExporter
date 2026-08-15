using System.Drawing;
using System.Globalization;

namespace SvnExporter;

public partial class MainWnd : Form
{
    private const string ModifyRecordDirectoryName = "modify_record";

    private readonly Color _versionExportColor;
    private readonly Color _authorExportColor;
    private readonly ISvnExportService _svnExportService = new SvnExportService();
    private CancellationTokenSource? _exportCancellation;
    private bool _closeAfterExportCancellation;
    private bool _allowClose;
    private int _authorListsLeft;
    private int _authorListsRightMargin;
    private int _authorListToTransferButtonGap;
    private int _authorTransferButtonToListGap;
    private int _authorTransferButtonWidth;

    public MainWnd()
    {
        InitializeComponent();
        Icon = AppIcon.Create() ?? Icon;

        _versionExportColor = groupBox1.BackColor;
        _authorExportColor = groupBox2.BackColor;

        CaptureAuthorListLayoutMetrics();
        BindEvents();
        UpdateAuthorListLayout();
        LoadConfig();
        UpdateModuleColors();
    }

    private void BindEvents()
    {
        btn_Browse.Click += BrowseButton_Click;
        btn_Browse2.Click += ExportPathBrowseButton_Click;
        btn_SaveConfig.Click += SaveConfigButton_Click;
        btn_AddAuthor.Click += AddAuthorButton_Click;
        btn_RemoveAuthor.Click += RemoveAuthorButton_Click;
        btn_AddToExport.Click += AddToExportButton_Click;
        btn_RemoveForExport.Click += RemoveFromExportButton_Click;
        btn_Export.Click += ExportButton_Click;
        cb_ExportVersion.CheckedChanged += ExportSwitch_CheckedChanged;
        cb_ExportModify.CheckedChanged += ExportSwitch_CheckedChanged;
        groupBox2.Resize += AuthorExportGroupBox_Resize;
        FormClosing += MainWnd_FormClosing;
    }

    private void CaptureAuthorListLayoutMetrics()
    {
        _authorListsLeft = lb_ExportAuthor.Left;
        _authorListsRightMargin = groupBox2.ClientSize.Width - lb_AllAuthor.Right;
        _authorListToTransferButtonGap = btn_AddToExport.Left - lb_ExportAuthor.Right;
        _authorTransferButtonToListGap = lb_AllAuthor.Left - btn_AddToExport.Right;
        _authorTransferButtonWidth = btn_AddToExport.Width;
    }

    private void AuthorExportGroupBox_Resize(object? sender, EventArgs e)
    {
        UpdateAuthorListLayout();
    }

    private void UpdateAuthorListLayout()
    {
        groupBox2.SuspendLayout();
        try
        {
            int listsRight = groupBox2.ClientSize.Width - _authorListsRightMargin;
            int availableListWidth = Math.Max(
                2,
                listsRight - _authorListsLeft -
                _authorListToTransferButtonGap -
                _authorTransferButtonWidth -
                _authorTransferButtonToListGap);

            int exportAuthorWidth = availableListWidth / 2;
            int allAuthorWidth = availableListWidth - exportAuthorWidth;

            lb_ExportAuthor.Width = exportAuthorWidth;

            int transferButtonsLeft = lb_ExportAuthor.Right + _authorListToTransferButtonGap;
            btn_AddToExport.Left = transferButtonsLeft;
            btn_RemoveForExport.Left = transferButtonsLeft;

            int allAuthorsLeft = transferButtonsLeft +
                _authorTransferButtonWidth +
                _authorTransferButtonToListGap;
            label6.Left = allAuthorsLeft;
            lb_AllAuthor.Left = allAuthorsLeft;
            lb_AllAuthor.Width = allAuthorWidth;
        }
        finally
        {
            groupBox2.ResumeLayout(performLayout: false);
        }
    }

    private async void ExportButton_Click(object? sender, EventArgs e)
    {
        if (!TryCreateExportRequests(out VersionExportRequest? versionRequest, out AuthorExportRequest? authorRequest))
        {
            return;
        }

        string outputPath = tb_ExportPath.Text.Trim();
        if (outputPath.Length == 0)
        {
            ShowValidationError("请选择输出目录。");
            return;
        }

        string outputDirectory;
        string projectDirectory;
        try
        {
            outputDirectory = Path.GetFullPath(outputPath);
            projectDirectory = Path.GetFullPath(tb_ProjectPath.Text.Trim());
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            ShowValidationError($"输出目录无效：{ex.Message}");
            return;
        }

        if (PathsOverlap(outputDirectory, projectDirectory))
        {
            ShowValidationError("导出目录不能与主目录重合，也不能是主目录的父目录或子目录，请选择其他位置。");
            return;
        }

        versionRequest = versionRequest is null
            ? null
            : versionRequest with { DestinationDirectory = outputDirectory };
        authorRequest = authorRequest is null
            ? null
            : authorRequest with
            {
                DestinationDirectory = Path.Combine(outputDirectory, ModifyRecordDirectoryName)
            };

        if (btn_DebugMode.Checked)
        {
            OutputDebugCommands(versionRequest, authorRequest);
            return;
        }

        try
        {
            if (Directory.Exists(outputDirectory) && Directory.EnumerateFileSystemEntries(outputDirectory).Any())
            {
                DialogResult confirmation = MessageBox.Show(
                    this,
                    "选择的导出目录不是空目录，已有同名文件可能会被覆盖。是否继续？",
                    "确认导出",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

                if (confirmation != DialogResult.Yes)
                {
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            ShowValidationError($"无法检查导出目录：{ex.Message}");
            return;
        }

        _exportCancellation = new CancellationTokenSource();
        SetExportingState(true);
        AppendConsole("========== 开始导出 ==========", Color.White);

        try
        {
            bool succeeded = true;
            if (versionRequest is not null)
            {
                SvnExportResult result = await _svnExportService.ExportVersionSnapshotAsync(
                    versionRequest,
                    HandleSvnMessage,
                    _exportCancellation.Token);
                succeeded &= result.Success;
            }

            if (authorRequest is not null)
            {
                SvnExportResult result = await _svnExportService.ExportAuthorChangesAsync(
                    authorRequest,
                    HandleSvnMessage,
                    _exportCancellation.Token);
                succeeded &= result.Success;
            }

            if (succeeded)
            {
                AppendConsole($"全部导出任务执行成功。输出目录：{outputDirectory}", Color.LawnGreen);
            }
            else
            {
                AppendConsole("导出任务已执行完毕，但有部分内容未能成功导出，请查看上方红色信息。", Color.Red);
            }
        }
        catch (OperationCanceledException)
        {
            AppendConsole("导出操作已取消。", Color.Red);
        }
        catch (Exception ex)
        {
            string message = $"导出失败：{ex.Message}";
            AppendConsole(message, Color.Red);
            if (!_closeAfterExportCancellation)
            {
                MessageBox.Show(this, message, "导出失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        finally
        {
            _exportCancellation.Dispose();
            _exportCancellation = null;
            if (!_closeAfterExportCancellation && !IsDisposed)
            {
                SetExportingState(false);
            }

            if (_closeAfterExportCancellation && !IsDisposed)
            {
                _allowClose = true;
                Close();
            }
        }
    }

    private bool TryCreateExportRequests(
        out VersionExportRequest? versionRequest,
        out AuthorExportRequest? authorRequest)
    {
        versionRequest = null;
        authorRequest = null;

        if (!cb_ExportVersion.Checked && !cb_ExportModify.Checked)
        {
            ShowValidationError("请至少选中一个“是否导出”开关。");
            return false;
        }

        string projectPath = tb_ProjectPath.Text.Trim();
        if (projectPath.Length == 0 || !Directory.Exists(projectPath))
        {
            ShowValidationError("请选择有效的主目录。");
            return false;
        }

        if (!TryNormalizeRevision(tb_VersionNum.Text, "工程导出版本号", out string endRevision))
        {
            return false;
        }

        if (cb_ExportVersion.Checked)
        {
            IReadOnlyList<string> enteredSubdirectories = SvnInputParser.SplitMultiValue(tb_ExportSubPath.Text);
            if (enteredSubdirectories.Count == 0)
            {
                ShowValidationError("版本导出已开启，请至少填写一个导出子目录。");
                return false;
            }

            var subdirectories = new List<string>();
            foreach (string subdirectory in enteredSubdirectories)
            {
                if (!TryNormalizeSubdirectory(projectPath, subdirectory, out string normalizedSubdirectory))
                {
                    ShowValidationError($"导出子目录“{subdirectory}”无效，请填写主目录下的相对路径。");
                    return false;
                }

                if (!subdirectories.Contains(normalizedSubdirectory, StringComparer.OrdinalIgnoreCase))
                {
                    subdirectories.Add(normalizedSubdirectory);
                }
            }

            string normalizedPatterns = tb_ExportFileType.Text
                .Replace('＊', '*')
                .Replace('？', '?');
            IReadOnlyList<string> filePatterns = SvnInputParser.SplitMultiValue(normalizedPatterns);
            if (filePatterns.Count == 0)
            {
                ShowValidationError("版本导出已开启，请至少填写一种导出文件类型。");
                return false;
            }

            if (filePatterns.Any(pattern => pattern.IndexOfAny(['/', '\\']) >= 0))
            {
                ShowValidationError("导出文件类型中不能包含目录路径，请使用类似 *.cs、*.shader 的格式。");
                return false;
            }

            versionRequest = new VersionExportRequest(
                projectPath,
                subdirectories,
                filePatterns,
                endRevision,
                string.Empty);
        }

        if (cb_ExportModify.Checked)
        {
            IReadOnlyList<string> authors = GetAllItems(lb_ExportAuthor);
            if (authors.Count == 0)
            {
                ShowValidationError("账号修改导出已开启，请至少选择一个导出账号。");
                return false;
            }

            if (!TryNormalizeRevision(tb_BeginVersionNum.Text, "起始版本号", out string startRevision, allowEmpty: true))
            {
                return false;
            }

            if (startRevision.Length > 0 && endRevision != "HEAD" &&
                long.Parse(startRevision) > long.Parse(endRevision))
            {
                ShowValidationError("起始版本号不能大于工程导出版本号。");
                return false;
            }

            authorRequest = new AuthorExportRequest(
                projectPath,
                authors,
                startRevision,
                endRevision,
                string.Empty);
        }

        return true;
    }

    private bool TryNormalizeRevision(
        string input,
        string fieldName,
        out string revision,
        bool allowEmpty = false)
    {
        revision = input.Trim();
        if (revision.Length == 0)
        {
            revision = allowEmpty ? string.Empty : "HEAD";
            return true;
        }

        if (!long.TryParse(revision, out long number) || number < 0)
        {
            ShowValidationError($"{fieldName}必须是大于或等于 0 的整数，也可以留空。");
            return false;
        }

        revision = number.ToString();
        return true;
    }

    private void HandleSvnMessage(SvnConsoleMessage message)
    {
        Color color = message.Kind switch
        {
            SvnMessageKind.Error => Color.Red,
            SvnMessageKind.Success => Color.LawnGreen,
            _ => Color.White
        };

        AppendConsole(message.Text, color);
    }

    private void OutputDebugCommands(
        VersionExportRequest? versionRequest,
        AuthorExportRequest? authorRequest)
    {
        AppendConsole("========== 调试模式：仅输出命令，不执行 ==========", Color.White);

        if (versionRequest is not null)
        {
            AppendConsole("【版本导出】", Color.White);
            AppendDebugCommand(["info", "--xml", "--non-interactive", versionRequest.LocalRootPath]);

            foreach (string subdirectory in versionRequest.Subdirectories)
            {
                string localSubdirectory = Path.Combine(
                    versionRequest.LocalRootPath,
                    subdirectory.Replace('/', Path.DirectorySeparatorChar));
                AppendDebugCommand(
                [
                    "list", "--xml", "--recursive", "--revision", versionRequest.Revision,
                    "--non-interactive", AppendPegRevision(localSubdirectory, versionRequest.Revision)
                ]);
            }

            AppendConsole(
                $"说明：实际执行时会解析上述 list 结果，筛选文件类型“{string.Join(", ", versionRequest.FilePatterns)}”，再为每个匹配文件生成以下命令。调试模式未执行 list，因此这里只能输出动态模板。",
                Color.White);
            AppendDebugCommand(
            [
                "export", "--force", "--quiet", "--revision", versionRequest.Revision,
                "--non-interactive", $"<匹配文件的 SVN URL>@{versionRequest.Revision}",
                Path.Combine(versionRequest.DestinationDirectory, "<原相对路径>")
            ],
            "[动态模板] ");
        }

        if (authorRequest is not null)
        {
            AppendConsole("【账号修改导出】", Color.White);
            AppendDebugCommand(["info", "--xml", "--non-interactive", authorRequest.LocalRootPath]);

            string logStartRevision = authorRequest.StartRevision.Length == 0
                ? CreateTodayRevisionSpec()
                : authorRequest.StartRevision;
            string range = $"{logStartRevision}:{authorRequest.EndRevision}";
            AppendDebugCommand(
            [
                "log", "--xml", "--verbose", "--quiet", "--revision", range,
                "--non-interactive", AppendPegRevision(authorRequest.LocalRootPath, authorRequest.EndRevision)
            ]);

            AppendConsole(
                $"说明：实际执行时会解析上述 log 结果，筛选账号“{string.Join(", ", authorRequest.Authors)}”，再为每个修改文件生成以下命令。调试模式未执行 log，因此这里只能输出动态模板。",
                Color.White);
            AppendDebugCommand(
            [
                "export", "--force", "--quiet", "--revision", authorRequest.EndRevision,
                "--non-interactive", $"<修改文件的 SVN URL>@{authorRequest.EndRevision}",
                Path.Combine(authorRequest.DestinationDirectory, "<原相对路径>")
            ],
            "[动态模板] ");
        }

        AppendConsole("调试模式命令生成完毕，未执行任何 SVN 命令，也未写入任何导出文件。", Color.LawnGreen);
    }

    private void AppendDebugCommand(IReadOnlyList<string> arguments, string prefix = "")
    {
        string command = prefix + "> svn.exe " + string.Join(' ', arguments.Select(QuoteForDisplay));
        AppendConsole(command, Color.White);
    }

    private static string AppendPegRevision(string target, string revision)
    {
        return $"{target}@{revision}";
    }

    private static string CreateTodayRevisionSpec()
    {
        DateTime localMidnight = DateTime.Today;
        var todayStart = new DateTimeOffset(localMidnight, TimeZoneInfo.Local.GetUtcOffset(localMidnight));
        return "{" + todayStart.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture) + "}";
    }

    private static string QuoteForDisplay(string argument)
    {
        if (argument.Length > 0 && !argument.Any(character => char.IsWhiteSpace(character) || character == '"'))
        {
            return argument;
        }

        return "\"" + argument.Replace("\"", "\\\"") + "\"";
    }

    private void SetExportingState(bool exporting)
    {
        btn_Export.Enabled = !exporting;
        btn_SaveConfig.Enabled = !exporting;
        btn_Browse.Enabled = !exporting;
        btn_Browse2.Enabled = !exporting;
        btn_AddAuthor.Enabled = !exporting;
        btn_RemoveAuthor.Enabled = !exporting;
        btn_AddToExport.Enabled = !exporting;
        btn_RemoveForExport.Enabled = !exporting;
        cb_ExportVersion.Enabled = !exporting;
        cb_ExportModify.Enabled = !exporting;
        btn_DebugMode.Enabled = !exporting;
        groupBox1.Enabled = !exporting;
        groupBox2.Enabled = !exporting;
        groupBox3.Enabled = !exporting;
        UseWaitCursor = exporting;
        btn_Export.Text = exporting ? "导出中" : "导出";
    }

    private void ShowValidationError(string message)
    {
        AppendConsole(message, Color.Red);
        MessageBox.Show(this, message, "输入有误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private void MainWnd_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_exportCancellation is null || _allowClose)
        {
            return;
        }

        e.Cancel = true;
        _closeAfterExportCancellation = true;
        AppendConsole("正在取消导出，请稍候关闭窗口……", Color.White);
        _exportCancellation.Cancel();
    }

    private static bool TryNormalizeSubdirectory(string projectPath, string path, out string normalizedPath)
    {
        normalizedPath = string.Empty;
        string enteredPath = path.Trim();
        if (enteredPath.Length == 0 ||
            enteredPath[0] is '/' or '\\' ||
            Path.IsPathRooted(enteredPath) ||
            enteredPath.Contains(':'))
        {
            return false;
        }

        string slashPath = enteredPath.Replace('\\', '/').TrimEnd('/');
        string[] segments = slashPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0 || segments.Any(segment => segment is "." or ".."))
        {
            return false;
        }

        string projectFullPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(projectPath));
        string candidateFullPath = Path.GetFullPath(Path.Combine(projectFullPath, Path.Combine(segments)));
        if (!candidateFullPath.StartsWith(EnsureTrailingDirectorySeparator(projectFullPath), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        normalizedPath = string.Join('/', segments);
        return true;
    }

    private static bool PathsOverlap(string firstPath, string secondPath)
    {
        return IsSameOrChildPath(firstPath, secondPath) || IsSameOrChildPath(secondPath, firstPath);
    }

    private static bool IsSameOrChildPath(string candidatePath, string parentPath)
    {
        string candidate = Path.TrimEndingDirectorySeparator(Path.GetFullPath(candidatePath));
        string parent = Path.TrimEndingDirectorySeparator(Path.GetFullPath(parentPath));
        if (string.Equals(candidate, parent, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return candidate.StartsWith(EnsureTrailingDirectorySeparator(parent), StringComparison.OrdinalIgnoreCase);
    }

    private static string EnsureTrailingDirectorySeparator(string path)
    {
        return Path.EndsInDirectorySeparator(path) ? path : path + Path.DirectorySeparatorChar;
    }

    private void BrowseButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "请选择 SVN 工作副本的主目录",
            ShowNewFolderButton = false,
            UseDescriptionForTitle = true
        };

        if (Directory.Exists(tb_ProjectPath.Text))
        {
            dialog.SelectedPath = tb_ProjectPath.Text;
        }

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            tb_ProjectPath.Text = dialog.SelectedPath;
        }
    }

    private void ExportPathBrowseButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "请选择导出内容的输出目录",
            ShowNewFolderButton = true,
            UseDescriptionForTitle = true
        };

        if (Directory.Exists(tb_ExportPath.Text))
        {
            dialog.SelectedPath = tb_ExportPath.Text;
        }

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            tb_ExportPath.Text = dialog.SelectedPath;
        }
    }

    private void SaveConfigButton_Click(object? sender, EventArgs e)
    {
        try
        {
            ConfigService.Save(CreateConfigFromControls());
            AppendConsole($"配置已保存：{ConfigService.ConfigPath}", Color.LawnGreen);
            MessageBox.Show(this, "配置保存成功。", "保存配置", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            string message = $"保存配置失败：{ex.Message}";
            AppendConsole(message, Color.Red);
            MessageBox.Show(this, message, "保存配置", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AddAuthorButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new AuthorInputDialog();
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        foreach (string author in dialog.Authors)
        {
            AddUniqueItem(lb_AllAuthor, author);
        }
    }

    private void RemoveAuthorButton_Click(object? sender, EventArgs e)
    {
        List<string> selectedAuthors = GetSelectedItems(lb_AllAuthor);
        if (selectedAuthors.Count == 0)
        {
            MessageBox.Show(this, "请先在“所有账号”中选择要移除的账号。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        foreach (string author in selectedAuthors)
        {
            lb_AllAuthor.Items.Remove(author);
            RemoveItem(lb_ExportAuthor, author);
        }
    }

    private void AddToExportButton_Click(object? sender, EventArgs e)
    {
        List<string> selectedAuthors = GetSelectedItems(lb_AllAuthor);
        if (selectedAuthors.Count == 0)
        {
            MessageBox.Show(this, "请先在“所有账号”中选择要导出的账号。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        foreach (string author in selectedAuthors)
        {
            AddUniqueItem(lb_ExportAuthor, author);
        }
    }

    private void RemoveFromExportButton_Click(object? sender, EventArgs e)
    {
        List<string> selectedAuthors = GetSelectedItems(lb_ExportAuthor);
        if (selectedAuthors.Count == 0)
        {
            MessageBox.Show(this, "请先在“导出账号”中选择要移除的账号。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        foreach (string author in selectedAuthors)
        {
            lb_ExportAuthor.Items.Remove(author);
        }
    }

    private void ExportSwitch_CheckedChanged(object? sender, EventArgs e)
    {
        UpdateModuleColors();
    }

    private void UpdateModuleColors()
    {
        groupBox1.BackColor = cb_ExportVersion.Checked ? _versionExportColor : Color.Gray;
        groupBox2.BackColor = cb_ExportModify.Checked ? _authorExportColor : Color.Gray;
    }

    private void LoadConfig()
    {
        try
        {
            AppConfig config = ConfigService.Load();
            tb_ProjectPath.Text = config.ProjectPath;
            tb_ExportSubPath.Text = config.ExportSubPath;
            tb_ExportFileType.Text = config.ExportFileType;
            tb_VersionNum.Text = config.ProjectVersionNumber;
            tb_BeginVersionNum.Text = config.BeginVersionNumber;
            tb_ExportPath.Text = config.OutputPath;
            cb_ExportVersion.Checked = config.ExportVersionEnabled;
            cb_ExportModify.Checked = config.ExportAuthorChangesEnabled;

            lb_AllAuthor.Items.Clear();
            lb_ExportAuthor.Items.Clear();

            foreach (string author in NormalizeAuthors(config.AllAuthors.Concat(config.ExportAuthors)))
            {
                lb_AllAuthor.Items.Add(author);
            }

            foreach (string author in NormalizeAuthors(config.ExportAuthors))
            {
                lb_ExportAuthor.Items.Add(author);
            }
        }
        catch (Exception ex)
        {
            AppendConsole($"读取配置失败，将使用界面默认设置：{ex.Message}", Color.Red);
        }
    }

    private AppConfig CreateConfigFromControls()
    {
        return new AppConfig
        {
            ProjectPath = tb_ProjectPath.Text.Trim(),
            ExportSubPath = tb_ExportSubPath.Text.Trim(),
            ExportFileType = tb_ExportFileType.Text.Trim(),
            ProjectVersionNumber = tb_VersionNum.Text.Trim(),
            ExportVersionEnabled = cb_ExportVersion.Checked,
            ExportAuthors = GetAllItems(lb_ExportAuthor),
            AllAuthors = GetAllItems(lb_AllAuthor),
            BeginVersionNumber = tb_BeginVersionNum.Text.Trim(),
            OutputPath = tb_ExportPath.Text.Trim(),
            ExportAuthorChangesEnabled = cb_ExportModify.Checked
        };
    }

    private void AppendConsole(string message, Color color)
    {
        if (rtb_Console.IsDisposed)
        {
            return;
        }

        if (rtb_Console.InvokeRequired)
        {
            rtb_Console.BeginInvoke(() => AppendConsole(message, color));
            return;
        }

        rtb_Console.SelectionStart = rtb_Console.TextLength;
        rtb_Console.SelectionLength = 0;
        rtb_Console.SelectionColor = color;
        rtb_Console.AppendText(message.TrimEnd('\r', '\n') + Environment.NewLine);
        rtb_Console.SelectionColor = rtb_Console.ForeColor;
        rtb_Console.ScrollToCaret();
    }

    private static void AddUniqueItem(ListBox listBox, string value)
    {
        string author = value.Trim();
        if (author.Length == 0 || GetAllItems(listBox).Contains(author, StringComparer.Ordinal))
        {
            return;
        }

        listBox.Items.Add(author);
    }

    private static void RemoveItem(ListBox listBox, string value)
    {
        object? item = listBox.Items
            .Cast<object>()
            .FirstOrDefault(candidate => string.Equals(candidate.ToString(), value, StringComparison.Ordinal));

        if (item is not null)
        {
            listBox.Items.Remove(item);
        }
    }

    private static List<string> GetSelectedItems(ListBox listBox)
    {
        return listBox.SelectedItems.Cast<object>()
            .Select(item => item.ToString() ?? string.Empty)
            .Where(item => item.Length > 0)
            .ToList();
    }

    private static List<string> GetAllItems(ListBox listBox)
    {
        return listBox.Items.Cast<object>()
            .Select(item => item.ToString()?.Trim() ?? string.Empty)
            .Where(item => item.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private static IEnumerable<string> NormalizeAuthors(IEnumerable<string?> authors)
    {
        return authors
            .Select(author => author?.Trim() ?? string.Empty)
            .Where(author => author.Length > 0)
            .Distinct(StringComparer.Ordinal);
    }
}
