using AppDocs.CodeDocumentation.Models;
using System.Net;
using System.Text;

namespace AppDocs.CodeDocumentation.Renders;

public sealed class HandlerDocumentationRenderer
{
    private const string TableHeaderStyle =
        "background-color:#f4f5f7;font-weight:bold;";

    private const string NestedTypeStyle =
        "background-color:#f7f8f9;font-weight:bold;";

    private const string SecondaryTextStyle =
        "color:#6b778c;font-size:12px;";

    private const string CodeStyle =
        "font-family:monospace;";

    public string Render(
        HandlerDocumentation documentation,
        HandlerDocumentationRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(documentation);

        options ??= new HandlerDocumentationRenderOptions();

        var builder = new StringBuilder();

        RenderHeader(builder, documentation);

        RenderRequest(builder, documentation.Request);

        RenderValidations(builder, documentation.Validations);

        RenderResponse(builder, documentation.Response);

        RenderDomainEvents(builder, documentation.DomainEvents);

        RenderEntities(builder, documentation.Entities);

        RenderCallTree(builder, documentation.CallTree);

        RenderCommits(
            builder,
            documentation.Commits,
            options);

        return builder.ToString();
    }

    #region Header

    private static void RenderHeader(
        StringBuilder builder,
        HandlerDocumentation documentation)
    {
        builder.AppendLine(
            $"<h1>{Encode(documentation.Request.Name)}</h1>");

        builder.AppendLine("<table>");
        builder.AppendLine("<tbody>");

        RenderKeyValueRow(
            builder,
            "Handler",
            $"<code>{Encode(documentation.HandlerName)}</code>");

        if (!string.IsNullOrWhiteSpace(documentation.HandlerDescription))
        {
            RenderKeyValueRow(
                builder,
                "Описание",
                Encode(documentation.HandlerDescription));
        }

        builder.AppendLine("</tbody>");
        builder.AppendLine("</table>");

        builder.AppendLine("<p />");
    }

    private static void RenderKeyValueRow(
        StringBuilder builder,
        string name,
        string value)
    {
        builder.AppendLine("<tr>");

        builder.AppendLine(
            $"<th style=\"{TableHeaderStyle}\">" +
            $"{Encode(name)}</th>");

        builder.AppendLine(
            $"<td>{value}</td>");

        builder.AppendLine("</tr>");
    }

    #endregion

    #region Request / Response

    private static void RenderRequest(
        StringBuilder builder,
        TypeDocumentation request)
    {
        builder.AppendLine("<h2>Входящие данные</h2>");

        RenderTypeInfo(builder, request);

        RenderPropertiesTable(builder, request);
    }

    private static void RenderResponse(
        StringBuilder builder,
        TypeDocumentation response)
    {
        builder.AppendLine("<h2>Исходящие данные</h2>");

        RenderTypeInfo(builder, response);

        RenderPropertiesTable(builder, response);
    }

    private static void RenderTypeInfo(
        StringBuilder builder,
        TypeDocumentation type)
    {
        builder.AppendLine("<p>");

        builder.Append(
            $"<strong>{Encode(type.Name)}</strong>");

        if (!string.IsNullOrWhiteSpace(type.FullName))
        {
            builder.Append("<br />");

            builder.Append(
                $"<code style=\"{CodeStyle}\">" +
                $"{Encode(type.FullName)}</code>");
        }

        builder.AppendLine("</p>");

        if (!string.IsNullOrWhiteSpace(type.Description))
        {
            builder.AppendLine(
                $"<p>{Encode(type.Description)}</p>");
        }
    }

    #endregion

    #region Properties

    private static void RenderPropertiesTable(
        StringBuilder builder,
        TypeDocumentation type)
    {
        if (type.Properties.Count == 0)
        {
            builder.AppendLine("<p>Нет свойств.</p>");
            return;
        }

        builder.AppendLine("<table>");

        builder.AppendLine("<thead>");
        builder.AppendLine("<tr>");

        builder.AppendLine(
            $"<th style=\"{TableHeaderStyle}\">" +
            "Наименование</th>");

        builder.AppendLine(
            $"<th style=\"{TableHeaderStyle}\">" +
            "Тип</th>");

        builder.AppendLine(
            $"<th style=\"{TableHeaderStyle}\">" +
            "Описание</th>");

        builder.AppendLine("</tr>");
        builder.AppendLine("</thead>");

        builder.AppendLine("<tbody>");

        foreach (var property in type.Properties)
        {
            RenderProperty(
                builder,
                property,
                level: 0,
                isLast: false);
        }

        builder.AppendLine("</tbody>");
        builder.AppendLine("</table>");
    }

    private static void RenderProperty(
        StringBuilder builder,
        PropertyDocumentation property,
        int level,
        bool isLast)
    {
        var typeDocumentation = property.TypeDocumentation;

        builder.AppendLine("<tr>");

        RenderPropertyNameCell(
            builder,
            property.Name,
            level);

        builder.AppendLine("<td>");

        builder.Append(
            $"<code style=\"{CodeStyle}\">" +
            $"{Encode(property.Type)}</code>");

        builder.AppendLine("</td>");

        builder.AppendLine("<td>");

        RenderPropertyDescription(
            builder,
            property);

        builder.AppendLine("</td>");

        builder.AppendLine("</tr>");

        if (typeDocumentation is null)
        {
            return;
        }

        RenderNestedType(
            builder,
            typeDocumentation,
            level + 1);
    }

    private static void RenderPropertyNameCell(
        StringBuilder builder,
        string name,
        int level)
    {
        builder.AppendLine("<td>");

        if (level == 0)
        {
            builder.Append(
                $"<strong>{Encode(name)}</strong>");
        }
        else
        {
            var indent = level * 20;

            builder.Append(
                $"<span style=\"margin-left:{indent}px\">");

            builder.Append("↳ ");

            builder.Append(
                Encode(name));

            builder.Append("</span>");
        }

        builder.AppendLine("</td>");
    }

    private static void RenderPropertyDescription(
        StringBuilder builder,
        PropertyDocumentation property)
    {
        var typeDocumentation = property.TypeDocumentation;

        if (!string.IsNullOrWhiteSpace(property.Description))
        {
            builder.Append(
                Encode(property.Description));
        }

        if (typeDocumentation is null ||
            string.IsNullOrWhiteSpace(typeDocumentation.Description))
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(property.Description))
        {
            builder.Append("<br />");
        }

        builder.Append(
            $"<span style=\"{SecondaryTextStyle}\">");

        builder.Append(
            Encode(typeDocumentation.Description));

        builder.Append("</span>");
    }

    private static void RenderNestedType(
        StringBuilder builder,
        TypeDocumentation type,
        int level)
    {
        /*
         * Вложенный объект занимает всю ширину таблицы.
         *
         * Например:
         *
         * Request
         * ├── User
         * │   ├── Id
         * │   └── Name
         *
         * User будет отдельной строкой colspan=3,
         * а Id/Name обычными строками ниже.
         */

        builder.AppendLine("<tr>");

        builder.AppendLine(
            $"<td colspan=\"3\" style=\"{NestedTypeStyle}\">");

        var indent = level * 20;

        builder.Append(
            $"<span style=\"margin-left:{indent}px\">");

        builder.Append("↳ ");

        builder.Append(
            $"<strong>{Encode(type.Name)}</strong>");

        if (!string.IsNullOrWhiteSpace(type.FullName))
        {
            builder.Append(" ");

            builder.Append(
                $"<code style=\"{CodeStyle}\">");

            builder.Append(
                Encode(type.FullName));

            builder.Append("</code>");
        }

        if (!string.IsNullOrWhiteSpace(type.Description))
        {
            builder.Append(" — ");

            builder.Append(
                Encode(type.Description));
        }

        builder.Append("</span>");

        builder.AppendLine("</td>");
        builder.AppendLine("</tr>");

        foreach (var property in type.Properties)
        {
            RenderProperty(
                builder,
                property,
                level,
                isLast: false);
        }
    }

    #endregion

    #region Validations

    private static void RenderValidations(
        StringBuilder builder,
        IReadOnlyCollection<ValidationDocumentation> validations)
    {
        if (validations.Count == 0)
        {
            return;
        }

        builder.AppendLine("<h2>Валидация</h2>");

        builder.AppendLine("<table>");

        builder.AppendLine("<thead>");
        builder.AppendLine("<tr>");

        builder.AppendLine(
            $"<th style=\"{TableHeaderStyle}\">" +
            "Свойство</th>");

        builder.AppendLine(
            $"<th style=\"{TableHeaderStyle}\">" +
            "Валидатор</th>");

        builder.AppendLine(
            $"<th style=\"{TableHeaderStyle}\">" +
            "Сообщение</th>");

        builder.AppendLine("</tr>");
        builder.AppendLine("</thead>");

        builder.AppendLine("<tbody>");

        foreach (var validation in validations)
        {
            builder.AppendLine("<tr>");

            builder.AppendLine(
                $"<td><code style=\"{CodeStyle}\">" +
                $"{Encode(validation.Property)}</code></td>");

            builder.AppendLine(
                $"<td><code style=\"{CodeStyle}\">" +
                $"{Encode(validation.Validator)}</code></td>");

            builder.AppendLine(
                $"<td>{EncodeNullable(validation.Message)}</td>");

            builder.AppendLine("</tr>");
        }

        builder.AppendLine("</tbody>");
        builder.AppendLine("</table>");
    }

    #endregion

    #region Domain Events

    private static void RenderDomainEvents(
        StringBuilder builder,
        IReadOnlyCollection<DomainEventDocumentation> events)
    {
        builder.AppendLine(
            "<h2>Порождаемые доменные события</h2>");

        if (events.Count == 0)
        {
            builder.AppendLine("<p>Нет.</p>");
            return;
        }

        builder.AppendLine("<table>");

        builder.AppendLine("<thead>");
        builder.AppendLine("<tr>");

        builder.AppendLine(
            $"<th style=\"{TableHeaderStyle}\">" +
            "Событие</th>");

        builder.AppendLine(
            $"<th style=\"{TableHeaderStyle}\">" +
            "Описание</th>");

        builder.AppendLine("</tr>");
        builder.AppendLine("</thead>");

        builder.AppendLine("<tbody>");

        foreach (var domainEvent in events)
        {
            builder.AppendLine("<tr>");

            builder.AppendLine(
                $"<td><code style=\"{CodeStyle}\">" +
                $"{Encode(domainEvent.Name)}</code></td>");

            builder.AppendLine(
                $"<td>{EncodeNullable(domainEvent.Description)}</td>");

            builder.AppendLine("</tr>");
        }

        builder.AppendLine("</tbody>");
        builder.AppendLine("</table>");
    }

    #endregion

    #region Entities

    private static void RenderEntities(
        StringBuilder builder,
        IReadOnlyCollection<EntityDocumentation> entities)
    {
        builder.AppendLine(
            "<h2>Затрагиваемые сущности</h2>");

        if (entities.Count == 0)
        {
            builder.AppendLine("<p>Нет.</p>");
            return;
        }

        builder.AppendLine("<table>");

        builder.AppendLine("<thead>");
        builder.AppendLine("<tr>");

        builder.AppendLine(
            $"<th style=\"{TableHeaderStyle}\">" +
            "Название</th>");

        builder.AppendLine(
            $"<th style=\"{TableHeaderStyle}\">" +
            "Тип</th>");

        builder.AppendLine(
            $"<th style=\"{TableHeaderStyle}\">" +
            "Описание</th>");

        builder.AppendLine("</tr>");
        builder.AppendLine("</thead>");

        builder.AppendLine("<tbody>");

        foreach (var entity in entities)
        {
            builder.AppendLine("<tr>");

            builder.AppendLine(
                $"<td><strong>{Encode(entity.Name)}</strong></td>");

            builder.AppendLine(
                $"<td><code style=\"{CodeStyle}\">" +
                $"{Encode(entity.Type)}</code></td>");

            builder.AppendLine(
                $"<td>{EncodeNullable(entity.Description)}</td>");

            builder.AppendLine("</tr>");
        }

        builder.AppendLine("</tbody>");
        builder.AppendLine("</table>");
    }

    #endregion

    #region Call Tree

    private static void RenderCallTree(
        StringBuilder builder,
        IReadOnlyCollection<MethodDocumentation> callTree)
    {
        builder.AppendLine("<h2>Дерево вызовов</h2>");

        if (callTree.Count == 0)
        {
            builder.AppendLine("<p>Нет данных.</p>");
            return;
        }

        builder.AppendLine("<ul>");

        foreach (var method in callTree)
        {
            RenderMethod(
                builder,
                method);
        }

        builder.AppendLine("</ul>");
    }

    private static void RenderMethod(
        StringBuilder builder,
        MethodDocumentation method)
    {
        builder.AppendLine("<li>");

        /*
         * Метод без дочерних вызовов.
         */
        if (method.Children.Count == 0)
        {
            RenderMethodContent(
                builder,
                method);

            builder.AppendLine("</li>");
            return;
        }

        /*
         * Метод с дочерними вызовами.
         *
         * Используем Confluence Expand Macro,
         * чтобы дерево не занимало всю страницу.
         */
        builder.AppendLine(
            "<ac:structured-macro ac:name=\"expand\">");

        builder.AppendLine(
            "<ac:parameter ac:name=\"title\">");

        builder.Append(
            EncodeMethodTitle(method));

        builder.AppendLine(
            "</ac:parameter>");

        builder.AppendLine(
            "<ac:rich-text-body>");

        builder.AppendLine("<ul>");

        foreach (var child in method.Children)
        {
            RenderMethod(
                builder,
                child);
        }

        builder.AppendLine("</ul>");

        builder.AppendLine(
            "</ac:rich-text-body>");

        builder.AppendLine(
            "</ac:structured-macro>");

        builder.AppendLine("</li>");
    }

    private static void RenderMethodContent(
        StringBuilder builder,
        MethodDocumentation method)
    {
        builder.Append(
            $"<strong>{Encode(method.Name)}</strong>");

        if (!string.IsNullOrWhiteSpace(method.Description))
        {
            builder.Append(" — ");

            builder.Append(
                Encode(method.Description));
        }

        builder.Append("<br />");

        builder.Append(
            $"<span style=\"{SecondaryTextStyle}\">");

        builder.Append(
            Encode(method.DeclaringType));

        builder.Append("</span>");

        builder.Append("<br />");

        builder.Append(
            $"<code style=\"{CodeStyle}\">");

        builder.Append(
            Encode(method.Signature));

        builder.Append("</code>");
    }

    private static string EncodeMethodTitle(
        MethodDocumentation method)
    {
        var builder = new StringBuilder();

        builder.Append(
            Encode(method.Name));

        if (!string.IsNullOrWhiteSpace(method.Description))
        {
            builder.Append(" — ");

            builder.Append(
                Encode(method.Description));
        }

        return builder.ToString();
    }

    #endregion

    #region Encoding

    private static string EncodeNullable(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : Encode(value);
    }

    private static string Encode(string value)
    {
        return WebUtility.HtmlEncode(value);
    }

    #endregion

    #region Commits

    private static void RenderCommits(
        StringBuilder builder,
        IReadOnlyCollection<CommitDocumentation> commits,
        HandlerDocumentationRenderOptions options)
    {
        builder.AppendLine("<h2>История изменений</h2>");

        if (commits.Count == 0)
        {
            builder.AppendLine("<p>Нет данных.</p>");
            return;
        }

        builder.AppendLine("<table>");

        builder.AppendLine("<thead>");
        builder.AppendLine("<tr>");

        builder.AppendLine(
            $"<th style=\"{TableHeaderStyle}\">" +
            "Дата</th>");

        builder.AppendLine(
            $"<th style=\"{TableHeaderStyle}\">" +
            "Автор</th>");

        builder.AppendLine(
            $"<th style=\"{TableHeaderStyle}\">" +
            "Сообщение</th>");

        builder.AppendLine(
            $"<th style=\"{TableHeaderStyle}\">" +
            "Commit</th>");

        builder.AppendLine("</tr>");
        builder.AppendLine("</thead>");

        builder.AppendLine("<tbody>");

        foreach (var commit in commits)
        {
            builder.AppendLine("<tr>");

            RenderCommitDate(
                builder,
                commit);

            RenderCommitAuthor(
                builder,
                commit);

            RenderCommitMessage(
                builder,
                commit);

            RenderCommitHash(
                builder,
                commit,
                options);

            builder.AppendLine("</tr>");
        }

        builder.AppendLine("</tbody>");
        builder.AppendLine("</table>");
    }

    private static void RenderCommitDate(
        StringBuilder builder,
        CommitDocumentation commit)
    {
        builder.AppendLine(
            $"<td>{commit.AuthorDate:dd.MM.yyyy HH:mm}</td>");
    }

    private static void RenderCommitAuthor(
        StringBuilder builder,
        CommitDocumentation commit)
    {
        builder.AppendLine("<td>");

        if (!string.IsNullOrWhiteSpace(commit.AuthorEmail))
        {
            RenderConfluenceUser(
                builder,
                commit.AuthorEmail);
        }
        else
        {
            builder.Append(
                Encode(commit.AuthorName));
        }

        builder.AppendLine("</td>");
    }

    private static void RenderCommitMessage(
        StringBuilder builder,
        CommitDocumentation commit)
    {
        builder.AppendLine(
            $"<td>{Encode(commit.Message)}</td>");
    }

    private static void RenderCommitHash(
        StringBuilder builder,
        CommitDocumentation commit,
        HandlerDocumentationRenderOptions options)
    {
        builder.AppendLine("<td>");

        var shortHash = GetShortHash(commit.Hash);

        if (string.IsNullOrWhiteSpace(options.GitlabRepositoryUrl))
        {
            builder.Append(
                $"<code style=\"{CodeStyle}\">" +
                $"{Encode(shortHash)}</code>");

            builder.AppendLine("</td>");
            return;
        }

        var commitUrl = BuildGitlabCommitUrl(
            options.GitlabRepositoryUrl,
            commit.Hash);

        builder.Append(
            $"<a href=\"{Encode(commitUrl)}\">");

        builder.Append(
            $"<code style=\"{CodeStyle}\">");

        builder.Append(
            Encode(shortHash));

        builder.Append("</code>");

        builder.Append("</a>");

        builder.AppendLine("</td>");
    }

    private static void RenderConfluenceUser(
    StringBuilder builder,
    string email)
    {
        builder.Append(
            "<ac:link>");

        builder.Append(
            $"<ri:user ri:username=\"{Encode(email)}\" />");

        builder.Append(
            "</ac:link>");
    }

    private static string BuildGitlabCommitUrl(
    string repositoryUrl,
    string hash)
    {
        return $"{repositoryUrl.TrimEnd('/')}/-/commit/{Uri.EscapeDataString(hash)}";
    }

    private static string GetShortHash(string hash)
    {
        const int length = 8;

        return hash.Length <= length
            ? hash
            : hash[..length];
    }

    #endregion
}