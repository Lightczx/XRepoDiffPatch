using LibGit2Sharp;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace XRepoDiffPatch;

public static class Program
{
    public static int Main(string[] args)
    {
        string description = """
            XRepoDiffPatch - A tool to generate and apply diffs between two repositories.
            """;
        RootCommand root = new(description);
        root.AddArgument(InvocationOptions.GitRoot);
        root.AddArgument(InvocationOptions.FromPath);
        root.AddArgument(InvocationOptions.ToPath);
        root.AddOption(InvocationOptions.From);
        root.AddOption(InvocationOptions.To);

        root.SetHandler(Run);

        return root.Invoke(args);
    }

    private static async Task Run(InvocationContext context)
    {
        string gitRoot = context.ParseResult.GetValueForArgument(InvocationOptions.GitRoot);
        string fromPath = context.ParseResult.GetValueForArgument(InvocationOptions.FromPath);
        string toPath = context.ParseResult.GetValueForArgument(InvocationOptions.ToPath);
        string? from = context.ParseResult.GetValueForOption(InvocationOptions.From);
        string? to = context.ParseResult.GetValueForOption(InvocationOptions.To);

        Console.WriteLine("Start to port latest changes from '{0}' to '{1}'", fromPath, toPath);

        using (Repository repo = new(fromPath))
        {
            Patch diff = repo.Diff.Compare<Patch>(repo.Lookup<Commit>(from ?? "HEAD~1").Tree, repo.Lookup<Commit>(to ?? "HEAD").Tree);

            string patchPath = Path.Combine(gitRoot, @"usr\bin\patch.exe");

            Process? patchProcess = Process.Start(new ProcessStartInfo()
            {
                FileName = patchPath,
                Arguments = "--no-backup-if-mismatch -p1",
                WorkingDirectory = toPath,
                RedirectStandardInput = true,
                UseShellExecute = false,
            });

            if (patchProcess == null)
            {
                Console.WriteLine("Failed to start patch process.");
                return;
            }

            await patchProcess.StandardInput.WriteAsync(diff.Content).ConfigureAwait(false);
            patchProcess.StandardInput.Close();

            await patchProcess.WaitForExitAsync().ConfigureAwait(false);
        }
    }
}