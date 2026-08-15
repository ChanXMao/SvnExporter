using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO.Enumeration;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace SvnExporter;

public sealed class SvnExportService : ISvnExportService
{
    private readonly string _svnExecutablePath;

    public SvnExportService(string svnExecutablePath = "svn.exe")
    {
        if (string.IsNullOrWhiteSpace(svnExecutablePath))
        {
            throw new ArgumentException("SVN 命令行程序路径不能为空。", nameof(svnExecutablePath));
        }

        _svnExecutablePath = svnExecutablePath.Trim();
    }

    public async Task<SvnWorkingCopyInfo> GetWorkingCopyInfoAsync(
        string localPath,
        Action<SvnConsoleMessage>? output = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(localPath) || !Directory.Exists(localPath))
        {
            throw new InvalidOperationException("主目录不存在，请重新选择有效目录。");
        }

        string fullPath = Path.GetFullPath(localPath);
        CommandResult command = await RunCommandAsync(
            ["info", "--xml", "--non-interactive", fullPath],
            output,
            cancellationToken);

        if (!command.Succeeded)
        {
            throw new InvalidOperationException("无法读取主目录的 SVN 信息，请确认该目录是 SVN 工作副本并检查上方错误。");
        }

        try
        {
            XDocument document = XDocument.Parse(command.StandardOutput, LoadOptions.None);
            XElement entry = FindElement(document, "entry")
                ?? throw new InvalidOperationException("SVN 返回的信息中缺少工作副本节点。");
            string url = GetRequiredElementValue(entry, "url", "SVN 地址").TrimEnd('/');
            XElement repository = FindElement(entry, "repository")
                ?? throw new InvalidOperationException("SVN 返回的信息中缺少仓库节点。");
            string repositoryRoot = GetRequiredElementValue(repository, "root", "仓库根地址").TrimEnd('/');
            string relativeUrl = GetRequiredElementValue(entry, "relative-url", "仓库相对路径");
            string repositoryRelativePath = NormalizeRepositoryPath(
                relativeUrl.StartsWith('^') ? relativeUrl[1..] : relativeUrl);

            return new SvnWorkingCopyInfo(url, repositoryRoot, repositoryRelativePath);
        }
        catch (XmlException ex)
        {
            throw new InvalidOperationException("无法解析 SVN 工作副本信息。", ex);
        }
    }

    public async Task<SvnExportResult> ExportVersionSnapshotAsync(
        VersionExportRequest request,
        Action<SvnConsoleMessage>? output = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        string localRoot = ValidateLocalRoot(request.LocalRootPath);
        string destinationRoot = ValidateDestination(request.DestinationDirectory);
        string revision = NormalizeRevision(request.Revision, "工程导出版本号", emptyMeansHead: true);
        IReadOnlyList<string> subdirectories = NormalizeSubdirectories(request.Subdirectories, localRoot);
        IReadOnlyList<string> filePatterns = NormalizeFilePatterns(request.FilePatterns);

        Directory.CreateDirectory(destinationRoot);
        SvnWorkingCopyInfo workingCopy = await GetWorkingCopyInfoAsync(localRoot, output, cancellationToken);

        var candidates = new Dictionary<string, ExportCandidate>(StringComparer.OrdinalIgnoreCase);
        int skippedCount = 0;
        bool allCommandsSucceeded = true;

        foreach (string subdirectory in subdirectories)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string directoryUrl = CombineSvnUrl(workingCopy.Url, subdirectory);
            CommandResult listCommand = await RunCommandAsync(
                [
                    "list", "--xml", "--recursive", "--revision", revision,
                    "--non-interactive", AppendPegRevision(directoryUrl, revision)
                ],
                output,
                cancellationToken);

            if (!listCommand.Succeeded)
            {
                allCommandsSucceeded = false;
                Report(output, $"无法读取子目录“{subdirectory}”在版本 {revision} 的内容。", SvnMessageKind.Error);
                continue;
            }

            foreach (string listedFile in ParseListedFiles(listCommand.StandardOutput))
            {
                string relativePath = CombineRelativePath(subdirectory, listedFile);
                string fileName = GetFileName(relativePath);
                if (!filePatterns.Any(pattern => FileSystemName.MatchesSimpleExpression(pattern, fileName, ignoreCase: true)))
                {
                    skippedCount++;
                    continue;
                }

                if (!candidates.ContainsKey(relativePath))
                {
                    candidates.Add(
                        relativePath,
                        new ExportCandidate(relativePath, CombineSvnUrl(workingCopy.Url, relativePath)));
                }
            }
        }

        int exportedCount = 0;
        foreach (ExportCandidate candidate in candidates.Values.OrderBy(item => item.RelativePath, StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();
            string targetPath;
            try
            {
                targetPath = GetSafeDestinationPath(destinationRoot, candidate.RelativePath);
                string? parentDirectory = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrEmpty(parentDirectory))
                {
                    Directory.CreateDirectory(parentDirectory);
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
            {
                skippedCount++;
                allCommandsSucceeded = false;
                Report(output, $"无法创建“{candidate.RelativePath}”的导出目录：{ex.Message}", SvnMessageKind.Error);
                continue;
            }

            CommandResult exportCommand = await RunCommandAsync(
                [
                    "export", "--force", "--quiet", "--revision", revision,
                    "--non-interactive", AppendPegRevision(candidate.SourceUrl, revision), targetPath
                ],
                output,
                cancellationToken);

            if (exportCommand.Succeeded)
            {
                exportedCount++;
            }
            else
            {
                skippedCount++;
                allCommandsSucceeded = false;
            }
        }

        string summary = $"版本导出完成：成功导出 {exportedCount} 个文件，跳过 {skippedCount} 个文件。";
        Report(output, summary, allCommandsSucceeded ? SvnMessageKind.Success : SvnMessageKind.Error);
        return new SvnExportResult(allCommandsSucceeded, exportedCount, skippedCount, 0, summary);
    }

    public async Task<SvnExportResult> ExportAuthorChangesAsync(
        AuthorExportRequest request,
        Action<SvnConsoleMessage>? output = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        string localRoot = ValidateLocalRoot(request.LocalRootPath);
        string destinationRoot = ValidateDestination(request.DestinationDirectory);
        string endRevision = NormalizeRevision(request.EndRevision, "工程导出版本号", emptyMeansHead: true);
        string startRevision = NormalizeRevision(request.StartRevision, "起始版本号", emptyMeansHead: false);
        HashSet<string> authors = NormalizeAuthors(request.Authors);
        if (authors.Count == 0)
        {
            throw new InvalidOperationException("账号修改导出至少需要选择一个账号。");
        }

        if (startRevision.Length > 0 && endRevision != "HEAD" &&
            long.Parse(startRevision, CultureInfo.InvariantCulture) > long.Parse(endRevision, CultureInfo.InvariantCulture))
        {
            throw new InvalidOperationException("起始版本号不能大于工程导出版本号。");
        }

        Directory.CreateDirectory(destinationRoot);
        SvnWorkingCopyInfo workingCopy = await GetWorkingCopyInfoAsync(localRoot, output, cancellationToken);

        DateTimeOffset? todayStart = null;
        string logStartRevision = startRevision;
        if (startRevision.Length == 0)
        {
            DateTime localMidnight = DateTime.Today;
            todayStart = new DateTimeOffset(localMidnight, TimeZoneInfo.Local.GetUtcOffset(localMidnight));
            logStartRevision = "{" + todayStart.Value.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture) + "}";
        }

        string range = $"{logStartRevision}:{endRevision}";
        CommandResult logCommand = await RunCommandAsync(
            [
                "log", "--xml", "--verbose", "--quiet", "--revision", range,
                "--non-interactive", AppendPegRevision(workingCopy.Url, endRevision)
            ],
            output,
            cancellationToken);

        if (!logCommand.Succeeded)
        {
            const string message = "读取账号修改记录失败，请检查上方 SVN 错误。";
            Report(output, message, SvnMessageKind.Error);
            return new SvnExportResult(false, 0, 0, 0, message);
        }

        AuthorChangeSet changeSet = ParseAuthorChanges(
            logCommand.StandardOutput,
            authors,
            workingCopy.RepositoryRelativePath,
            todayStart);

        foreach (string deletedPath in changeSet.DeletedPaths.OrderBy(path => path, StringComparer.Ordinal))
        {
            Report(output, $"已删除，无法导出：{deletedPath}", SvnMessageKind.Error);
        }

        int exportedCount = 0;
        int skippedCount = 0;
        bool allCommandsSucceeded = true;

        foreach (string relativePath in changeSet.ExportPaths.OrderBy(path => path, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            string targetPath;
            try
            {
                targetPath = GetSafeDestinationPath(destinationRoot, relativePath);
                string? parentDirectory = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrEmpty(parentDirectory))
                {
                    Directory.CreateDirectory(parentDirectory);
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
            {
                skippedCount++;
                allCommandsSucceeded = false;
                Report(output, $"无法创建“{relativePath}”的导出目录：{ex.Message}", SvnMessageKind.Error);
                continue;
            }

            string sourceUrl = CombineSvnUrl(workingCopy.Url, relativePath);
            CommandResult exportCommand = await RunCommandAsync(
                [
                    "export", "--force", "--quiet", "--revision", endRevision,
                    "--non-interactive", AppendPegRevision(sourceUrl, endRevision), targetPath
                ],
                output,
                cancellationToken);

            if (exportCommand.Succeeded)
            {
                exportedCount++;
            }
            else
            {
                skippedCount++;
                allCommandsSucceeded = false;
            }
        }

        string summary =
            $"账号修改导出完成：成功导出 {exportedCount} 个文件，跳过 {skippedCount} 个文件，发现 {changeSet.DeletedPaths.Count} 个删除项。";
        Report(output, summary, allCommandsSucceeded ? SvnMessageKind.Success : SvnMessageKind.Error);
        return new SvnExportResult(
            allCommandsSucceeded,
            exportedCount,
            skippedCount,
            changeSet.DeletedPaths.Count,
            summary);
    }

    private async Task<CommandResult> RunCommandAsync(
        IReadOnlyList<string> arguments,
        Action<SvnConsoleMessage>? output,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Report(
            output,
            "> " + QuoteForDisplay(_svnExecutablePath) + " " + string.Join(' ', arguments.Select(QuoteForDisplay)),
            SvnMessageKind.Normal);

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _svnExecutablePath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            }
        };

        foreach (string argument in arguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        try
        {
            process.Start();
        }
        catch (Win32Exception ex)
        {
            string message = $"无法启动 SVN 命令行程序“{_svnExecutablePath}”，请确认已安装 SVN 命令行工具并已加入 PATH。";
            Report(output, message, SvnMessageKind.Error);
            throw new InvalidOperationException(message, ex);
        }

        Task<string> standardOutputTask = process.StandardOutput.ReadToEndAsync();
        Task<string> standardErrorTask = process.StandardError.ReadToEndAsync();

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            TryKillProcess(process);
            await process.WaitForExitAsync(CancellationToken.None);
            throw;
        }

        string standardOutput = await standardOutputTask;
        string standardError = await standardErrorTask;
        if (!string.IsNullOrWhiteSpace(standardError))
        {
            Report(output, standardError.Trim(), SvnMessageKind.Error);
        }

        if (process.ExitCode == 0)
        {
            Report(output, "命令执行成功。", SvnMessageKind.Success);
        }
        else if (string.IsNullOrWhiteSpace(standardError))
        {
            Report(output, $"SVN 命令执行失败，退出代码：{process.ExitCode}。", SvnMessageKind.Error);
        }

        return new CommandResult(process.ExitCode, standardOutput, standardError);
    }

    private static IReadOnlyList<string> ParseListedFiles(string xml)
    {
        try
        {
            XDocument document = XDocument.Parse(xml, LoadOptions.None);
            return document.Descendants()
                .Where(element => element.Name.LocalName == "entry" &&
                    string.Equals((string?)element.Attribute("kind"), "file", StringComparison.OrdinalIgnoreCase))
                .Select(entry => FindElement(entry, "name")?.Value.Trim() ?? string.Empty)
                .Where(name => name.Length > 0)
                .Select(NormalizeRelativePath)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        catch (XmlException ex)
        {
            throw new InvalidOperationException("无法解析 SVN 子目录列表。", ex);
        }
    }

    private static AuthorChangeSet ParseAuthorChanges(
        string xml,
        HashSet<string> authors,
        string rootRepositoryPath,
        DateTimeOffset? earliestDate)
    {
        try
        {
            XDocument document = XDocument.Parse(xml, LoadOptions.None);
            var exportPaths = new HashSet<string>(StringComparer.Ordinal);
            var deletedPaths = new HashSet<string>(StringComparer.Ordinal);
            string normalizedRoot = NormalizeRepositoryPath(rootRepositoryPath);

            foreach (XElement logEntry in document.Descendants().Where(element => element.Name.LocalName == "logentry"))
            {
                string author = FindElement(logEntry, "author")?.Value.Trim() ?? string.Empty;
                if (!authors.Contains(author))
                {
                    continue;
                }

                if (earliestDate.HasValue)
                {
                    string dateText = FindElement(logEntry, "date")?.Value.Trim() ?? string.Empty;
                    if (!DateTimeOffset.TryParse(
                            dateText,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                            out DateTimeOffset commitDate) ||
                        commitDate < earliestDate.Value.ToUniversalTime())
                    {
                        continue;
                    }
                }

                foreach (XElement pathElement in logEntry.Descendants().Where(element => element.Name.LocalName == "path"))
                {
                    string repositoryPath = NormalizeRepositoryPath(pathElement.Value);
                    if (!TryMakeRootRelative(repositoryPath, normalizedRoot, out string relativePath) || relativePath.Length == 0)
                    {
                        continue;
                    }

                    string action = ((string?)pathElement.Attribute("action") ?? string.Empty).Trim().ToUpperInvariant();
                    string nodeKind = ((string?)pathElement.Attribute("kind") ?? string.Empty).Trim();
                    if (action == "D")
                    {
                        deletedPaths.Add(relativePath);
                        exportPaths.Remove(relativePath);
                        continue;
                    }

                    if (string.Equals(nodeKind, "dir", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    exportPaths.Add(relativePath);
                    deletedPaths.Remove(relativePath);
                }
            }

            return new AuthorChangeSet(exportPaths, deletedPaths);
        }
        catch (XmlException ex)
        {
            throw new InvalidOperationException("无法解析 SVN 账号修改记录。", ex);
        }
    }

    private static string ValidateLocalRoot(string localRoot)
    {
        if (string.IsNullOrWhiteSpace(localRoot) || !Directory.Exists(localRoot))
        {
            throw new InvalidOperationException("主目录不存在，请重新选择有效目录。");
        }

        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(localRoot));
    }

    private static string ValidateDestination(string destination)
    {
        if (string.IsNullOrWhiteSpace(destination))
        {
            throw new InvalidOperationException("导出目录不能为空。");
        }

        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(destination));
    }

    private static string NormalizeRevision(string? revision, string fieldName, bool emptyMeansHead)
    {
        string value = revision?.Trim() ?? string.Empty;
        if (value.Length == 0)
        {
            return emptyMeansHead ? "HEAD" : string.Empty;
        }

        if (string.Equals(value, "HEAD", StringComparison.OrdinalIgnoreCase))
        {
            if (!emptyMeansHead)
            {
                throw new InvalidOperationException($"{fieldName}必须是大于或等于 0 的整数，也可以留空。");
            }

            return "HEAD";
        }

        if (!long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out long number) || number < 0)
        {
            throw new InvalidOperationException($"{fieldName}必须是大于或等于 0 的整数，也可以留空。");
        }

        return number.ToString(CultureInfo.InvariantCulture);
    }

    private static IReadOnlyList<string> NormalizeSubdirectories(
        IReadOnlyList<string>? subdirectories,
        string localRoot)
    {
        if (subdirectories is null || subdirectories.Count == 0)
        {
            throw new InvalidOperationException("版本导出至少需要一个导出子目录。");
        }

        var normalized = new List<string>();
        foreach (string? subdirectory in subdirectories)
        {
            string entered = subdirectory?.Trim() ?? string.Empty;
            if (entered.Length == 0 || entered[0] is '/' or '\\' || Path.IsPathRooted(entered) || entered.Contains(':'))
            {
                throw new InvalidOperationException($"导出子目录“{entered}”不是有效的相对路径。");
            }

            string relative = NormalizeRelativePath(entered);
            string[] segments = relative.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0 || segments.Any(segment => segment is "." or ".."))
            {
                throw new InvalidOperationException($"导出子目录“{entered}”不是有效的相对路径。");
            }

            string combined = Path.GetFullPath(Path.Combine(localRoot, Path.Combine(segments)));
            string rootPrefix = EnsureTrailingDirectorySeparator(localRoot);
            if (!combined.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"导出子目录“{entered}”超出了主目录范围。");
            }

            string normalizedPath = string.Join('/', segments);
            if (!normalized.Contains(normalizedPath, StringComparer.OrdinalIgnoreCase))
            {
                normalized.Add(normalizedPath);
            }
        }

        return normalized;
    }

    private static IReadOnlyList<string> NormalizeFilePatterns(IReadOnlyList<string>? filePatterns)
    {
        if (filePatterns is null || filePatterns.Count == 0)
        {
            throw new InvalidOperationException("版本导出至少需要一种导出文件类型。");
        }

        string[] patterns = filePatterns
            .Select(pattern => pattern?.Trim().Replace('＊', '*').Replace('？', '?') ?? string.Empty)
            .Where(pattern => pattern.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (patterns.Length == 0 || patterns.Any(pattern => pattern.IndexOfAny(['/', '\\']) >= 0))
        {
            throw new InvalidOperationException("导出文件类型无效，请使用类似 *.cs、*.shader 的格式。");
        }

        return patterns;
    }

    private static HashSet<string> NormalizeAuthors(IReadOnlyList<string>? authors)
    {
        return authors is null
            ? new HashSet<string>(StringComparer.Ordinal)
            : authors
                .Select(author => author?.Trim() ?? string.Empty)
                .Where(author => author.Length > 0)
                .ToHashSet(StringComparer.Ordinal);
    }

    private static string GetSafeDestinationPath(string destinationRoot, string relativePath)
    {
        string normalizedRelativePath = NormalizeRelativePath(relativePath);
        string[] segments = normalizedRelativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0 || segments.Any(segment => segment is "." or ".."))
        {
            throw new ArgumentException("导出文件的相对路径无效。", nameof(relativePath));
        }

        string targetPath = Path.GetFullPath(Path.Combine(destinationRoot, Path.Combine(segments)));
        if (!targetPath.StartsWith(EnsureTrailingDirectorySeparator(destinationRoot), StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("导出文件路径超出了目标目录。", nameof(relativePath));
        }

        return targetPath;
    }

    private static string CombineSvnUrl(string baseUrl, string relativePath)
    {
        string normalized = NormalizeRelativePath(relativePath);
        if (normalized.Length == 0)
        {
            return baseUrl.TrimEnd('/');
        }

        string escapedPath = string.Join(
            '/',
            normalized.Split('/', StringSplitOptions.RemoveEmptyEntries).Select(Uri.EscapeDataString));
        return baseUrl.TrimEnd('/') + "/" + escapedPath;
    }

    private static string AppendPegRevision(string url, string revision)
    {
        return $"{url}@{revision}";
    }

    private static string CombineRelativePath(string first, string second)
    {
        string firstPart = NormalizeRelativePath(first).TrimEnd('/');
        string secondPart = NormalizeRelativePath(second).TrimStart('/');
        return firstPart.Length == 0 ? secondPart : $"{firstPart}/{secondPart}";
    }

    private static string NormalizeRelativePath(string path)
    {
        return path.Trim().Replace('\\', '/').Trim('/');
    }

    private static string NormalizeRepositoryPath(string path)
    {
        string normalized = path.Trim().Replace('\\', '/');
        return "/" + normalized.Trim('/');
    }

    private static bool TryMakeRootRelative(string repositoryPath, string rootPath, out string relativePath)
    {
        relativePath = string.Empty;
        if (string.Equals(repositoryPath, rootPath, StringComparison.Ordinal))
        {
            return true;
        }

        string prefix = rootPath == "/" ? "/" : rootPath.TrimEnd('/') + "/";
        if (!repositoryPath.StartsWith(prefix, StringComparison.Ordinal))
        {
            return false;
        }

        relativePath = NormalizeRelativePath(repositoryPath[prefix.Length..]);
        return true;
    }

    private static string GetFileName(string relativePath)
    {
        int separatorIndex = relativePath.LastIndexOf('/');
        return separatorIndex < 0 ? relativePath : relativePath[(separatorIndex + 1)..];
    }

    private static string EnsureTrailingDirectorySeparator(string path)
    {
        string fullPath = Path.GetFullPath(path);
        return Path.EndsInDirectorySeparator(fullPath) ? fullPath : fullPath + Path.DirectorySeparatorChar;
    }

    private static XElement? FindElement(XContainer container, string localName)
    {
        return container.Descendants().FirstOrDefault(element => element.Name.LocalName == localName);
    }

    private static string GetRequiredElementValue(XContainer container, string localName, string displayName)
    {
        string value = FindElement(container, localName)?.Value.Trim() ?? string.Empty;
        return value.Length > 0
            ? value
            : throw new InvalidOperationException($"SVN 返回的信息中缺少{displayName}。");
    }

    private static string QuoteForDisplay(string argument)
    {
        if (argument.Length > 0 && !argument.Any(character => char.IsWhiteSpace(character) || character == '"'))
        {
            return argument;
        }

        return "\"" + argument.Replace("\"", "\\\"") + "\"";
    }

    private static void Report(
        Action<SvnConsoleMessage>? output,
        string text,
        SvnMessageKind kind)
    {
        if (output is null || string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        try
        {
            output(new SvnConsoleMessage(text, kind));
        }
        catch
        {
            // 输出回调不应中断正在执行的 SVN 操作。
        }
    }

    private static void TryKillProcess(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (InvalidOperationException)
        {
        }
        catch (Win32Exception)
        {
        }
    }

    private sealed record CommandResult(int ExitCode, string StandardOutput, string StandardError)
    {
        public bool Succeeded => ExitCode == 0;
    }

    private sealed record ExportCandidate(string RelativePath, string SourceUrl);

    private sealed record AuthorChangeSet(
        HashSet<string> ExportPaths,
        HashSet<string> DeletedPaths);
}
