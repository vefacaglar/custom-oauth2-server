namespace CustomLogin.Application.Configuration;

public sealed class MongoDBSettings
{
    public const string SectionName = "MongoDB";
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}
