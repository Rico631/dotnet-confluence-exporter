using AwesomeAssertions;
using ConfluenceExporter.CodeDocumentation.Models;
using Xunit;

namespace ConfluenceExporter.Tests;

public sealed class AnalysisOptionsTests
{
    [Fact]
    public void AnalysisOptions_DefaultsToIRequestHandler()
    {
        var options = new AnalysisOptions();

        options.RequestHandlerName.Should().Be("IRequestHandler");
    }
}
