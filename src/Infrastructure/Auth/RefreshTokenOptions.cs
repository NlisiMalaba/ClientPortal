namespace Infrastructure.Auth;

public sealed class RefreshTokenOptions
{
    public const string SectionName = "RefreshToken";

    public int TokenSizeBytes { get; set; } = 32;

    public int LifetimeDays { get; set; } = 7;

    public int Iterations { get; set; } = 4;

    public int MemorySizeKb { get; set; } = 65536;

    public int DegreeOfParallelism { get; set; } = 2;

    public string Pepper { get; set; } = string.Empty;
}
