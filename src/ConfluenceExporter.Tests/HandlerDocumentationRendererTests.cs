using AwesomeAssertions;
using ConfluenceExporter.CodeDocumentation.Models;
using ConfluenceExporter.CodeDocumentation.Renders;
using Xunit;

namespace ConfluenceExporter.Tests;

public sealed class HandlerDocumentationRendererTests
{
    [Fact]
    public void Render_WhenGitlabRepositoryUrlAndSourcePathPresent_UsesBlobLinkForHandler()
    {
        var documentation = new HandlerDocumentation
        {
            HandlerName = "SampleHandler",
            HandlerDescription = "Test handler",
            Request = new TypeDocumentation { Name = "Request", FullName = "Sample.Request" },
            Response = new TypeDocumentation { Name = "Response", FullName = "Sample.Response" },
            SourceRelativePath = "src/Handlers/SampleHandler.cs",
            CallTree =
            [
                new MethodDocumentation
                {
                    Name = "Process",
                    DeclaringType = "SampleProcessor",
                    Signature = "void Process()",
                    SourceRelativePath = "src/Services/SampleProcessor.cs"
                }
            ]
        };

        var html = new HandlerDocumentationRenderer().Render(
            documentation,
            new HandlerDocumentationRenderOptions
            {
                GitlabRepositoryUrl = "https://gitlab.example.com/group/project",
                GitlabReference = "main"
            });

        html.Should().Contain("/-/blob/main/src/Handlers/SampleHandler.cs");
        html.Should().Contain("https://gitlab.example.com/group/project");
    }

    [Fact]
    public void Render_WhenGitlabRepositoryUrlMissing_KeepsPlainTextHandlerAndMethodNames()
    {
        var documentation = new HandlerDocumentation
        {
            HandlerName = "SampleHandler",
            Request = new TypeDocumentation { Name = "Request", FullName = "Sample.Request" },
            Response = new TypeDocumentation { Name = "Response", FullName = "Sample.Response" },
            SourceRelativePath = "src/Handlers/SampleHandler.cs",
            CallTree =
            [
                new MethodDocumentation
                {
                    Name = "Process",
                    DeclaringType = "SampleProcessor",
                    Signature = "void Process()",
                    SourceRelativePath = "src/Services/SampleProcessor.cs"
                }
            ]
        };

        var html = new HandlerDocumentationRenderer().Render(documentation);

        html.Should().NotContain("/-/blob/");
        html.Should().Contain("SampleHandler");
        html.Should().Contain("Process");
    }
}
