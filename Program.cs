using System.Text;

namespace DocsLinkTool
{
    internal class Program
    {
        /// <summary>
        /// Update links in one or more markdown files
        /// by using the definitions.json redirect file.
        /// </summary>
        /// <param name="args">Either a folder or a markdown file path</param>
        /// <returns></returns>
        internal static async Task Main(string[] args)
        {
            string[] articleFilePaths = null;

            if (args[0].ContainsXx(@"\dotnet-desktop-guide\framework\"))
            {
                throw new Exception("dotnet-link-tool isn't compatible with .NET Framework articles.");
            }

            if (File.Exists(args[0]) && string.Equals(Path.GetExtension(args[0]), ".md"))
            {
                // Store the path of the specified markdown file.
                articleFilePaths = new string[] { args[0] };
            }
            else if (Directory.Exists(args[0]) && Directory.GetFiles(args[0], "*.md", SearchOption.AllDirectories).Length > 0)
            {
                // Store the paths of all markdown files in folder.
                articleFilePaths = Directory.GetFiles(args[0], "*.md", SearchOption.AllDirectories);
            }
            else
            {
                Console.WriteLine($"Path '{args[0]}' must be a markdown file, or a folder with markdown files (at any depth).");
                return;
            }

            // Generate local paths to repo folders.
            var baseRepoPath = args[0][..args[0].IndexOf(@"\dotnet\docs-desktop\")];
            UpdateLinks.InitializePaths(baseRepoPath);

            // Iterate through each markdown file.
            await Task.Run(() =>
            {
                foreach (string articleFilePath in articleFilePaths)
                {
                    // Validate path isn't to a .NET Framework article.
                    if (articleFilePath.ContainsXx(@"\dotnet-desktop-guide\framework\"))
                        throw new Exception("dotnet-link-tool isn't compatible with .NET Framework articles.");

                    // Update links in a file.
                    UpdateLinks.UpdateArticleLinks(articleFilePath);
                }
            });

            // Print report.
            Log.LinkItemSets = Log.LinkItemSets.Where(x => x.IsLinkChanged).OrderBy(x => x.ArticlePath).ToList();

            // Scope the report to fixed redirect issues.
            //LinkItemSets = LinkItemSets.Where(x => x.IsRedirected).ToList();

            // Scope report to fixed non-redirect issues.
            //LinkItemSets = LinkItemSets.Where(x => !x.IsRedirected).ToList();

            // Scope report to links that were ignored.
            //LinkItemSets = LinkItemSets.Where(x => !x.IsLinkChanged).ToList();

            StringBuilder sb = new();
            int i = 1;
            foreach (Log.LinkItemSet linkItemSet in Log.LinkItemSets)
            {
                string text = $"{i++}. Link in article: '{linkItemSet.ArticlePath}' {(linkItemSet.IsLinkChanged ? "*" : "")}\r\n";
                string indent = "  ";
                foreach (Log.LinkItem linkItem in linkItemSet.Items)
                {
                    text += $"{indent}{linkItem.Action}: {linkItem.Link}\r\n";
                    indent += "  ";
                }
                Console.WriteLine(text);
                sb.AppendLine(text);
            }
        }
    }
}
