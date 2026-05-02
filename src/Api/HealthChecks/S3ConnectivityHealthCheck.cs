using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Api.HealthChecks;

/// <summary>
/// Optional connectivity probe for AWS S3–compatible object storage. Registered only when
/// <c>HealthChecks:S3:Enabled</c> is true in application configuration.
/// </summary>
public sealed class S3ConnectivityHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public S3ConnectivityHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        string? region = _configuration["HealthChecks:S3:Region"];
        string? bucketName = _configuration["HealthChecks:S3:BucketName"];

        if (string.IsNullOrWhiteSpace(region))
        {
            return HealthCheckResult.Unhealthy("S3 health check requires HealthChecks:S3:Region.");
        }

        if (string.IsNullOrWhiteSpace(bucketName))
        {
            return HealthCheckResult.Unhealthy("S3 health check requires HealthChecks:S3:BucketName.");
        }

        RegionEndpoint regionEndpoint = RegionEndpoint.GetBySystemName(region);

        try
        {
            using AmazonS3Client s3Client = new(regionEndpoint);
            GetBucketLocationResponse response = await s3Client.GetBucketLocationAsync(
                new GetBucketLocationRequest { BucketName = bucketName },
                cancellationToken);

            return HealthCheckResult.Healthy($"S3 bucket '{bucketName}' is reachable in region '{response.Location?.Value ?? "unknown"}'.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("S3 connectivity check failed.", exception);
        }
    }
}
