namespace SvnExporter;

/// <summary>
/// 应用程序中需要持久化的导出设置。
/// </summary>
public sealed class AppConfig
{
    private string _projectPath = string.Empty;
    private string _exportSubPath = string.Empty;
    private string _exportFileType = string.Empty;
    private string _projectVersionNumber = string.Empty;
    private List<string> _exportAuthors = [];
    private List<string> _allAuthors = [];
    private string _beginVersionNumber = string.Empty;
    private string _outputPath = string.Empty;

    public string ProjectPath
    {
        get => _projectPath;
        set => _projectPath = value ?? string.Empty;
    }

    public string ExportSubPath
    {
        get => _exportSubPath;
        set => _exportSubPath = value ?? string.Empty;
    }

    public string ExportFileType
    {
        get => _exportFileType;
        set => _exportFileType = value ?? string.Empty;
    }

    public string ProjectVersionNumber
    {
        get => _projectVersionNumber;
        set => _projectVersionNumber = value ?? string.Empty;
    }

    public bool ExportVersionEnabled { get; set; } = true;

    public List<string> ExportAuthors
    {
        get => _exportAuthors;
        set => _exportAuthors = value ?? [];
    }

    public List<string> AllAuthors
    {
        get => _allAuthors;
        set => _allAuthors = value ?? [];
    }

    public string BeginVersionNumber
    {
        get => _beginVersionNumber;
        set => _beginVersionNumber = value ?? string.Empty;
    }

    public string OutputPath
    {
        get => _outputPath;
        set => _outputPath = value ?? string.Empty;
    }

    public bool ExportAuthorChangesEnabled { get; set; }
}
