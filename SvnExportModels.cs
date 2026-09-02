namespace SvnExporter;

/// <summary>
/// 控制台消息的显示类别。
/// </summary>
public enum SvnMessageKind
{
    Normal,
    Error,
    Success
}

/// <summary>
/// SVN 服务产生的一条控制台消息。
/// </summary>
public sealed record SvnConsoleMessage(string Text, SvnMessageKind Kind);

/// <summary>
/// 本地路径对应的 SVN 仓库信息。
/// </summary>
public sealed record SvnWorkingCopyInfo(
    string Url,
    string RepositoryRootUrl,
    string RepositoryRelativePath);

/// <summary>
/// 按版本导出快照所需的参数。
/// </summary>
public sealed record VersionExportRequest(
    string LocalRootPath,
    IReadOnlyList<string> Subdirectories,
    IReadOnlyList<string> FilePatterns,
    string Revision,
    string DestinationDirectory,
    bool UseLocalFiles = false);

/// <summary>
/// 按账号导出修改记录所需的参数。
/// </summary>
public sealed record AuthorExportRequest(
    string LocalRootPath,
    IReadOnlyList<string> Authors,
    IReadOnlyList<string> Subdirectories,
    IReadOnlyList<string> FilePatterns,
    string StartRevision,
    string EndRevision,
    string DestinationDirectory);

/// <summary>
/// 一次导出操作的结果。
/// </summary>
public sealed record SvnExportResult(
    bool Success,
    int ExportedFileCount,
    int SkippedFileCount,
    int DeletedFileCount,
    string Message);

/// <summary>
/// SVN 导出的公共接口。回调可能在线程池线程上触发，WinForms 调用方应切换回 UI 线程后更新控件。
/// </summary>
public interface ISvnExportService
{
    Task<SvnWorkingCopyInfo> GetWorkingCopyInfoAsync(
        string localPath,
        Action<SvnConsoleMessage>? output = null,
        CancellationToken cancellationToken = default);

    Task<SvnExportResult> ExportVersionSnapshotAsync(
        VersionExportRequest request,
        Action<SvnConsoleMessage>? output = null,
        CancellationToken cancellationToken = default);

    Task<SvnExportResult> ExportAuthorChangesAsync(
        AuthorExportRequest request,
        Action<SvnConsoleMessage>? output = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 界面输入解析辅助方法。
/// </summary>
public static class SvnInputParser
{
    private static readonly char[] Separators =
        [',', ';', '，', '；', '﹐', '﹔', '、', '､', '\r', '\n', '\t'];

    /// <summary>
    /// 按中英文逗号、分号或顿号拆分输入，并去除空项和重复项。
    /// </summary>
    public static IReadOnlyList<string> SplitMultiValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Array.Empty<string>();
        }

        return value
            .Split(Separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(item => item.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}

/// <summary>
/// 将 SVN 账号转换为同名 diff 文件，并检查 Windows 文件名限制及大小写冲突。
/// </summary>
internal static class AuthorDiffFileName
{
    private static readonly HashSet<string> ReservedWindowsNames = new(
        [
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        ],
        StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyDictionary<string, string> CreateFileNames(IEnumerable<string> authors)
    {
        ArgumentNullException.ThrowIfNull(authors);

        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        var usedFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (string author in authors)
        {
            string normalizedAuthor = author.Trim();
            string fileName = $"{normalizedAuthor}.diff";
            if (normalizedAuthor.Length == 0 ||
                normalizedAuthor.Any(character => character < 32) ||
                normalizedAuthor.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                throw new InvalidOperationException($"账号“{author}”包含 Windows 文件名不支持的字符，无法生成同名 diff 文件。");
            }

            string deviceName = normalizedAuthor.Split('.', 2)[0];
            if (ReservedWindowsNames.Contains(deviceName))
            {
                throw new InvalidOperationException($"账号“{author}”是 Windows 保留文件名，无法生成同名 diff 文件。");
            }

            if (!usedFileNames.Add(fileName))
            {
                throw new InvalidOperationException(
                    $"账号列表中存在只能以大小写区分的名称（例如“{author}”），Windows 无法为它们分别生成同名 diff 文件。");
            }

            result.Add(normalizedAuthor, fileName);
        }

        return result;
    }
}
