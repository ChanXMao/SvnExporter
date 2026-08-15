using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace SvnExporter;

/// <summary>
/// 在可执行文件所在目录读写应用配置。
/// </summary>
public static class ConfigService
{
    private const string ConfigFileName = "config.ini";
    private const string LegacyConfigFileName = "config.txt";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public static string ConfigPath => Path.Combine(AppContext.BaseDirectory, ConfigFileName);

    private static string LegacyConfigPath => Path.Combine(AppContext.BaseDirectory, LegacyConfigFileName);

    /// <summary>
    /// 加载配置。配置文件不存在或内容为空时返回带默认值的新配置。
    /// </summary>
    public static AppConfig Load()
    {
        string loadPath = File.Exists(ConfigPath) ? ConfigPath : LegacyConfigPath;
        if (!File.Exists(loadPath))
        {
            return new AppConfig();
        }

        string json = File.ReadAllText(loadPath, Encoding.UTF8);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new AppConfig();
        }

        return JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? new AppConfig();
    }

    /// <summary>
    /// 使用 UTF-8 将配置保存为可读的 JSON 文本。
    /// </summary>
    public static void Save(AppConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        string json = JsonSerializer.Serialize(config, JsonOptions);
        string temporaryPath = Path.Combine(
            AppContext.BaseDirectory,
            $".{ConfigFileName}.{Guid.NewGuid():N}.tmp");

        try
        {
            File.WriteAllText(
                temporaryPath,
                json,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            File.Move(temporaryPath, ConfigPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }
}
