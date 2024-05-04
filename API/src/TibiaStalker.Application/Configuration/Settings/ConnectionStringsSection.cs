namespace TibiaStalker.Application.Configuration.Settings;

public class ConnectionStringsSection
{
    public const string SectionName = "ConnectionStrings";
    public string PostgreSql { get; init; }
}