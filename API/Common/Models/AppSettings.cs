namespace SDET_POC.API.Common.Models;

public sealed class AppSettings
{
    public bool IsProduction { get; set; }
    public Guid UserId { get; set; }
}

public sealed class CW1Settings
{
    public string ServerId { get; set; } = string.Empty;

    public string BaseUri { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}