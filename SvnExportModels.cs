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
    string DestinationDirectory);

/// <summary>
/// 按账号导出修改文件所需的参数。
/// </summary>
public sealed record AuthorExportRequest(
    string LocalRootPath,
    IReadOnlyList<string> Authors,
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
