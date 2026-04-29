using Shared;

namespace Domain;

public sealed class DocumentVersion : Entity<Guid>
{
    public Guid DocumentId { get; private set; }

    public int VersionNumber { get; private set; }

    public string S3Key { get; private set; } = string.Empty;

    public DateTime UploadedAt { get; private set; }

    public Guid UploadedBy { get; private set; }

    public string? ChangeNotes { get; private set; }

    private DocumentVersion()
    {
    }

    private DocumentVersion(
        Guid id,
        Guid documentId,
        int versionNumber,
        string s3Key,
        DateTime uploadedAtUtc,
        Guid uploadedBy,
        string? changeNotes)
        : base(id)
    {
        DocumentId = documentId;
        VersionNumber = NormalizeVersionNumber(versionNumber);
        S3Key = NormalizeS3Key(s3Key);
        UploadedAt = NormalizeUploadedAt(uploadedAtUtc);
        UploadedBy = uploadedBy;
        ChangeNotes = NormalizeChangeNotes(changeNotes);
    }

    public static DocumentVersion Create(
        Guid id,
        Guid documentId,
        int versionNumber,
        string s3Key,
        DateTime uploadedAtUtc,
        Guid uploadedBy,
        string? changeNotes = null)
    {
        return new DocumentVersion(id, documentId, versionNumber, s3Key, uploadedAtUtc, uploadedBy, changeNotes);
    }

    public void UpdateChangeNotes(string? changeNotes)
    {
        ChangeNotes = NormalizeChangeNotes(changeNotes);
        MarkUpdated();
    }

    private static int NormalizeVersionNumber(int versionNumber)
    {
        if (versionNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(versionNumber), "Version number must be greater than zero.");
        }

        return versionNumber;
    }

    private static string NormalizeS3Key(string s3Key)
    {
        return Guard.NotEmpty(s3Key, nameof(s3Key)).Trim();
    }

    private static DateTime NormalizeUploadedAt(DateTime uploadedAtUtc)
    {
        DateTime normalized = uploadedAtUtc;
        if (normalized.Kind == DateTimeKind.Local)
        {
            normalized = normalized.ToUniversalTime();
        }
        else if (normalized.Kind == DateTimeKind.Unspecified)
        {
            normalized = DateTime.SpecifyKind(normalized, DateTimeKind.Utc);
        }

        if (normalized > DateTime.UtcNow)
        {
            throw new ArgumentException("Upload timestamp cannot be in the future.", nameof(uploadedAtUtc));
        }

        return normalized;
    }

    private static string? NormalizeChangeNotes(string? changeNotes)
    {
        return string.IsNullOrWhiteSpace(changeNotes) ? null : changeNotes.Trim();
    }
}
