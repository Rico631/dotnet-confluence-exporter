
namespace WebApp4.Shared.Dpv;


/// <summary>
/// Request DTO used to save one or more documents in the Dpv subsystem.
/// </summary>
public sealed class AddDocumentRequest
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Documents to be saved. Provide an empty list when there are no documents.
    /// This property is mutable to allow model binding and modifications.
    /// </summary>
    public List<Document> Documents { get; set; } = new List<Document>();
}

/// <summary>
/// Represents metadata for a single document.
/// </summary>
public class Document
{
    /// <summary>
    /// Unique identifier of the document.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Human-readable name of the document.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Format/type of the document.
    /// </summary>
    public DocumentType DocumentType { get; set; }
}

/// <summary>
/// Supported document formats.
/// </summary>
public enum DocumentType
{
    /// <summary>PDF document.</summary>
    Pdf,

    /// <summary>Microsoft Word document.</summary>
    Word,

    /// <summary>Microsoft Excel spreadsheet.</summary>
    Excel,

    /// <summary>Plain text document.</summary>
    Text
}
