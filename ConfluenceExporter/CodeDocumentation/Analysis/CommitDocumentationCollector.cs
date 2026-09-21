namespace AppDocs.CodeDocumentation.Analysis;

using AppDocs.CodeDocumentation.Models;
using LibGit2Sharp;

public sealed class CommitDocumentationCollector
{
    public IReadOnlyList<CommitDocumentation> Collect(
        string filePath)
    {
        var repositoryPath =
            GitRepositoryLocator.FindRepositoryRoot(filePath);

        if (repositoryPath is null)
            return [];

        using var repository = new Repository(repositoryPath);

        var relativePath = Path.GetRelativePath(
            repositoryPath,
            filePath);

        relativePath = NormalizePath(relativePath);

        return repository.Commits
            .Where(commit => ChangesFile(
                repository,
                commit,
                relativePath))
            .Select(commit => new CommitDocumentation
            {
                AuthorDate = commit.Author.When,
                AuthorName = commit.Author.Name,
                AuthorEmail = commit.Author.Email,
                Message = commit.Message,
                Hash = commit.Sha,
            })
            .ToList();
    }

    private static bool ChangesFile(
        Repository repository,
        Commit commit,
        string filePath)
    {
        if (commit.Parents.Count() == 0)
            return commit[filePath] is not null;

        var parent = commit.Parents.First();

        var changes = repository.Diff.Compare<TreeChanges>(
            parent.Tree,
            commit.Tree);

        return changes.Any(change =>
            NormalizePath(change.Path) == filePath ||
            NormalizePath(change.OldPath) == filePath);
    }

    private static string NormalizePath(string? path)
    {
        return path?
            .Replace('\\', '/')
            .TrimStart('/') ?? string.Empty;
    }
}

public static class GitRepositoryLocator
{
    public static string? FindRepositoryRoot(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);

        if (string.IsNullOrWhiteSpace(directory))
            return null;

        var current = new DirectoryInfo(directory);

        while (current is not null)
        {
            var gitPath = Path.Combine(current.FullName, ".git");

            if (Directory.Exists(gitPath) ||
                File.Exists(gitPath))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return null;
    }
}