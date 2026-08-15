namespace SvnExporter;

internal static class AppIcon
{
    private const string ResourceName = "SvnExporter.Assets.SvnExporterIcon.ico";

    public static Icon? Create()
    {
        using Stream? stream = typeof(AppIcon).Assembly.GetManifestResourceStream(ResourceName);
        if (stream is null)
        {
            return null;
        }

        using var sourceIcon = new Icon(stream);
        return (Icon)sourceIcon.Clone();
    }
}
