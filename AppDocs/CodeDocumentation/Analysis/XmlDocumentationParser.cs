using System.Xml;
using System.Xml.Linq;

namespace AppDocs.CodeDocumentation.Analysis;

public static class XmlDocumentationParser
{
    public static string? GetSummary(string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            return null;

        try
        {
            var document = XDocument.Parse(xml);

            var summary = document
                .Descendants("summary")
                .FirstOrDefault();

            if (summary is null)
                return null;

            return Normalize(summary.Value);
        }
        catch (XmlException)
        {
            return null;
        }
    }

    private static string Normalize(string value)
    {
        return string.Join(
            ' ',
            value.Split(
                [' ', '\r', '\n', '\t'],
                StringSplitOptions.RemoveEmptyEntries));
    }
}