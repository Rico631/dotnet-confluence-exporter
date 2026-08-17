
using System.Diagnostics;

namespace AppDocs.CodeDocumentation.Models;

[DebuggerDisplay($"Property:{{{nameof(Property)}}} Validator:{{{nameof(Validator)}}} Message:{{{nameof(Message)}}}")]
public sealed class ValidationDocumentation
{
    public required string Property { get; init; }

    public required string Validator { get; init; }

    public string? Message { get; init; }
}
