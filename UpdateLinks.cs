using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace DocsLinkTool
{
    public partial class UpdateLinks
    {
        // Path to definitions.json.
        private static string redirectFilePath;

        // Alias path for the \dotnet\docs-desktop\dotnet-desktop-guide repo.
        private static readonly string docsetAliasPath = "/dotnet/desktop/";

        // Absolute unaliased folder paths for the \dotnet\docs-desktop\dotnet-desktop-guide repo.
        // Only the fist two folder paths are relevant to the redirects, since they have mirrored content.
        private static string docsetAbsoluteNetPath;
        private static string docsetAbsoluteFrmPath;
        private static string docsetAbsoluteBasePath;

        /// <summary>
        /// Use the definitions.json redirects to update article links within the current document.
        /// </summary>
        /// <param name="articleFilePath">The file path of the current document.</param>
        /// <returns>Link redirect stats.</returns>
        internal static async Task UpdateArticleLinksAsync(string articleFilePath)
        {
            string articleDirPath = Path.GetDirectoryName(articleFilePath);

            // Run task asynchronously.
            await Task.Run(() =>
            {
                #region Read article

                // Read article content.
                string articleContent = File.ReadAllText(articleFilePath);

                // Remove any commented text in article (may contain invalid links).
                //articleContent = Regex.Replace(articleContent, @"<!--.*?-->", "", RegexOptions.Singleline);

                #endregion

                #region Read and parse redirect file

                // Get redirects from definitions.json (not .openpublishing.redirection.json).
                string redirectJson = string.Join("\r\n", File.ReadAllLines(redirectFilePath).Where(x => !x.Trim().StartsWith("//")));
                List<Redirect> redirectList = new();

                // Parse json.
                JsonNode rootJsonNode = JsonNode.Parse(redirectJson);
                JsonNode[] arrr = rootJsonNode["Entries"].AsArray().ToArray();
                foreach (JsonNode item in arrr)
                {
                    Redirect redirect = new();
                    redirect.SrcPath = item["SourceUrl"].GetValue<string>();
                    redirect.DstPath = item["TargetUrl"].GetValue<string>();
                    redirect.SrcVersion = Regex.Match(item["SourceUrl"].GetValue<string>(), @"(netframeworkdesktop|netdesktop)-(?<version>\d\.\d)").Groups["version"].Value;
                    redirect.DstVersion = Regex.Match(item["TargetUrl"].GetValue<string>(), @"(netframeworkdesktop|netdesktop)-(?<version>\d\.\d)").Groups["version"].Value;
                    // Validate redirects.
                    if (double.Parse(redirect.SrcVersion) > double.Parse(redirect.DstVersion))
                        continue;   // TODO: invert if TwoWay redirect.
                    if (File.Exists(redirect.SrcPath.Replace(docsetAliasPath, docsetAbsoluteNetPath).Replace("/", "\\") + ".md") &&
                        File.Exists(redirect.SrcPath.Replace(docsetAliasPath, docsetAbsoluteFrmPath).Replace("/", "\\") + ".md"))
                        throw new Exception("Ambigous source path");
                    if (!File.Exists(redirect.SrcPath.Replace(docsetAliasPath, docsetAbsoluteNetPath).Replace("/", "\\") + ".md") &&
                        !File.Exists(redirect.SrcPath.Replace(docsetAliasPath, docsetAbsoluteFrmPath).Replace("/", "\\") + ".md"))
                        continue;   // TODO: look into why one of the redirects specifies a non-existent source file.
                    redirectList.Add(redirect);
                }
                redirectList = redirectList.OrderBy(x => Path.GetFileName(x.DstPath)).ToList();

                #endregion

                // Define a regex pattern for a non-greedy match [some text](some-text).
                string findPattern = @"(?<=\[.*?\]\()(?<link>.*?)(?=\))";
                // Iterate through each link within the current document.
                string newFileContent = Regex.Replace(articleContent, findPattern, m =>
                {
                    // Get the link.
                    string origLink = m.Groups["link"].ToString().Trim();

                    // Initialize local variables.
                    string origLinkBase = GetBaseLink(origLink);
                    string query = "";
                    string fragment = "";
                    bool isOrigRelativeLink = false;
                    bool isNewRelativeLink = false;

                    #region Log setup

                    // Setup link logging.
                    LinkItemSet linkItemSet = new(articlePath: articleFilePath[(articleFilePath.IndexOf("\\dotnet-desktop-guide") + "\\dotnet-desktop-guide".Length)..].Replace("/", "\\"));
                    linkItemSet.Items.Add(new LinkItem("ORIGINAL-LINK", origLink));
                    Program.LinkItemSets.Add(linkItemSet);

                    #endregion

                    #region Debug statements

                    // Debugging break line.
                    if (origLink.Contains("/dotnet/desktop/wpf/advanced/routed-events-overview?view=netframeworkdesktop-4.8&preserve-view=true#why-use-routed-events"))
                    { }

                    #endregion

                    #region Ignorable links

                    // Don't update non-article links.
                    if (origLink.ContainsXx("/includes/"))
                    {
                        // Keep include file links as is.
                        linkItemSet.Items.Add(new LinkItem("UNCHANGED", "include file link"));
                        return origLink;
                    }
                    if (origLink.ContainsXx("xref"))
                    {
                        // Keep xref links as is.
                        linkItemSet.Items.Add(new LinkItem("UNCHANGED", "xref link"));
                        return origLink;
                    }
                    if (origLink.ContainsXx(".png") || origLink.ContainsXx(".jpg") || origLink.ContainsXx(".gif"))
                    {
                        // Keep image links as is.
                        linkItemSet.Items.Add(new LinkItem("UNCHANGED", "image link"));
                        return origLink;
                    }
                    if (origLink.StartsWith("#"))
                    {
                        // Keep fragment links as is.
                        linkItemSet.Items.Add(new LinkItem("UNCHANGED", "fragment link"));
                        return origLink;
                    }
                    if (origLink.StartsWithXx("http"))
                    {
                        // Keep http(s) links as is.
                        linkItemSet.Items.Add(new LinkItem("UNCHANGED", "http(s) link"));
                        return origLink;
                    }
                    if (origLink.StartsWith("/") && !origLink.StartsWithXx(docsetAliasPath))
                    {
                        // Keep http(s) links as is.
                        linkItemSet.Items.Add(new LinkItem("UNCHANGED", "links to another repo"));
                        return origLink;
                    }
                    if (origLink.StartsWith("~") && !origLink.StartsWithXx(docsetAliasPath))
                    {
                        // Keep ~ links as is.
                        linkItemSet.Items.Add(new LinkItem("UNCHANGED", "unsupported link (tilda)"));
                        return origLink;
                    }
                    if (origLink.ContainsXx(".yml") || origLink.ContainsXx(".xaml") || origLink.ContainsXx(".cs") || origLink.ContainsXx(".vb"))
                    {
                        // Keep non-markdown links as is.
                        linkItemSet.Items.Add(new LinkItem("UNCHANGED", "unsupported link (non-markdown file)"));
                        return origLink;
                    }
                    if (origLink.Contains(' ') || origLink.Contains('(') || origLink.Contains(')') || origLink.Contains('[') || origLink.Contains(']'))
                    {
                        // Not a link.
                        linkItemSet.Items.Add(new LinkItem("UNCHANGED", "unsupported link (unrecognized format)"));
                        return origLink;
                    }
                    linkItemSet.IsLinkChanged = true;

                    #endregion

                    #region Reformat article link for redirect check.

                    isOrigRelativeLink = !origLink.StartsWithXx(docsetAliasPath);

                    // Expand relative paths to repo alias path.
                    string updatedLink = origLink;
                    if (updatedLink.StartsWith(".."))
                    {
                        // relative link in different folder to current article.
                        updatedLink = GetAliasPath(Path.GetFullPath(Path.Combine(articleDirPath, updatedLink)));
                        linkItemSet.Items.Add(new LinkItem("ALIAS-PATH", updatedLink));
                    }
                    if (updatedLink.StartsWith("./"))
                    {
                        // relative link in same folder as current article.
                        updatedLink = updatedLink[2..];
                        linkItemSet.Items.Add(new LinkItem("ALIAS-PATH", updatedLink));
                    }
                    if (string.Equals(Path.GetFileName(updatedLink), updatedLink, StringComparison.OrdinalIgnoreCase))
                    {
                        // relative link in same folder as current article.
                        updatedLink = GetAliasPath(Path.Combine(articleDirPath, updatedLink));
                        linkItemSet.Items.Add(new LinkItem("ALIAS-PATH", updatedLink));
                    }

                    // Remove and store url parameters.
                    if (updatedLink.Contains('#'))
                    {
                        // Remove & store fragment.
                        fragment = updatedLink[updatedLink.IndexOf("#")..];
                        updatedLink = updatedLink[..updatedLink.IndexOf("#")];
                        linkItemSet.Items.Add(new LinkItem("REMOVE-FRAGMENT", updatedLink));
                    }
                    if (updatedLink.Contains('?'))
                    {
                        // Remove & store query.
                        query = updatedLink[updatedLink.IndexOf("?")..];
                        updatedLink = updatedLink[..updatedLink.IndexOf("?")];
                        linkItemSet.Items.Add(new LinkItem("REMOVE-QUERY", updatedLink));
                    }
                    if (!updatedLink.StartsWith("/"))
                    {
                        // relative link in a subfolder of the current article.
                        updatedLink = GetAliasPath(Path.Combine(articleDirPath, updatedLink));
                        linkItemSet.Items.Add(new LinkItem("EXPAND-PATH", updatedLink));
                    }

                    #endregion

                    #region Validate that the article link is now an alias path.

                    if (!updatedLink.StartsWith(docsetAliasPath))
                    {
                        throw new Exception("Unable to get repo alias path.");
                    }

                    #endregion

                    #region Process redirects.

                    // Check for and replace with redirect.
                    Redirect[] redirectLinks = redirectList.Where(x => string.Equals(x.SrcPath, updatedLink, StringComparison.OrdinalIgnoreCase)).ToArray();
                    if (redirectLinks.Length > 1 && !redirectLinks.All(x => string.Equals(x.DstPath, redirectLinks.First().DstPath, StringComparison.OrdinalIgnoreCase)))
                        throw new Exception("Multiple divergent redirects for same file.");
                    if (redirectLinks.Length == 1)
                    {
                        // Replace with redirect.
                        linkItemSet.IsRedirected = true;
                        updatedLink = redirectLinks.First().DstPath;
                        linkItemSet.Items.Add(new LinkItem("REDIRECT-TO", updatedLink));

                        // Remove & store the new redirect query.
                        if (updatedLink.Contains('?'))
                        {
                            query = updatedLink[updatedLink.IndexOf("?")..] + "&preserve-view=true";
                            updatedLink = updatedLink[..updatedLink.IndexOf("?")];
                            linkItemSet.Items.Add(new LinkItem("REMOVE-NEW-QUERY", updatedLink));
                        }
                        else query = "";
                    }
                    else
                    {
                        // No redirect found.
                        linkItemSet.Items.Add(new LinkItem("REDIRECT-TO", "no redirect"));
                    }

                    #endregion

                    #region Validate that the article exists at the alias path.

                    // Handle a redirect that omits index.md.
                    if (updatedLink.EndsWith("/"))
                    {
                        updatedLink += "index";
                        linkItemSet.Items.Add(new LinkItem("COMPLETE-PATH", updatedLink));
                    }

                    // Validate that the article exists on disk.
                    string absolutePath = "";
                    if (isOrigRelativeLink && !linkItemSet.IsRedirected)
                    {
                        absolutePath = Path.GetFullPath(Path.Combine(articleDirPath, origLinkBase));
                        if (!File.Exists(absolutePath))
                            absolutePath = null;
                    }
                    else
                    {
                        absolutePath = GetAbsolutePath(updatedLink);
                    }

                    if (absolutePath == null)
                    {
                        linkItemSet.IsLinkChanged = false;
                        linkItemSet.Items.Add(new LinkItem("INVALID-LINK", "file not found"));
                        return origLink;
                    }

                    #endregion

                    #region Relative path restoration

                    // Debugging statement.
                    //if (updatedLink.Contains("getting-started/index") && articleFilePath.Contains(@"\framework\wpf\index.md"))
                    //{ }

                    // Convert to a relative path if the original link was a relative path.
                    if (isOrigRelativeLink && !linkItemSet.IsRedirected)
                    {
                        updatedLink = Path.GetRelativePath(articleDirPath, absolutePath).Replace("\\", "/");
                        isNewRelativeLink = true;
                        linkItemSet.Items.Add(new LinkItem("RELATIVE-PATH", updatedLink));
                    }

                    #endregion

                    #region Restore query parameters

                    // Restore query.
                    if (!isNewRelativeLink && query.Length > 0)
                    {
                        updatedLink += query;
                        linkItemSet.Items.Add(new LinkItem("RESTORE-QUERY", updatedLink));
                    }

                    // Restore fragment.
                    if (fragment.Length > 0)
                    {
                        updatedLink += fragment;
                        linkItemSet.Items.Add(new LinkItem("RESTORE-FRAGMENT", updatedLink));
                    }

                    #endregion

                    #region Log unchanged links

                    // Check for unchanged links.
                    if (Equals(updatedLink, origLink))
                    {
                        linkItemSet.IsLinkChanged = false;
                        linkItemSet.Items.Add(new LinkItem("UNCHANGED", "same as original link"));
                    }

                    #endregion

                    return updatedLink;
                });

                #region Update links on disk

                // Update old links in the current document.
                if (!Equals(newFileContent, articleContent))
                {
                    File.WriteAllText(articleFilePath, newFileContent);
                }

                #endregion
            });
        }

        #region Path conversion helper methods

        // Generate absolute repo paths using your local repo base path.
        internal static void InitializePaths(string baseRepoPath)
        {
            redirectFilePath = Path.Combine(baseRepoPath, @"dotnet\docs-desktop\redirects_generator\definitions.json");
            docsetAbsoluteNetPath = Path.Combine(baseRepoPath, @"dotnet\docs-desktop\dotnet-desktop-guide\net\");
            docsetAbsoluteFrmPath = Path.Combine(baseRepoPath, @"dotnet\docs-desktop\dotnet-desktop-guide\framework\");
            docsetAbsoluteBasePath = Path.Combine(baseRepoPath, @"dotnet\docs-desktop\dotnet-desktop-guide\");
        }

        private static string GetBaseLink(string link)
        {
            string baseLink = link.Contains('#') ? link[..link.IndexOf("#")] : link;
            baseLink = baseLink.Contains('?') ? baseLink[..baseLink.IndexOf("?")] : baseLink;
            return baseLink;
        }

        private static string GetAliasPath(string link)
        {
            return link
                .Replace(docsetAbsoluteNetPath, docsetAliasPath)
                .Replace(docsetAbsoluteFrmPath, docsetAliasPath)
                .Replace(docsetAbsoluteBasePath, docsetAliasPath)
                .Replace("\\", "/")
                .Replace(".md", "");
        }

        private static string GetAbsolutePath(string link)
        {
            string path1 = link.Replace(docsetAliasPath, docsetAbsoluteNetPath).Replace("/", "\\") + ".md";
            string path2 = link.Replace(docsetAliasPath, docsetAbsoluteFrmPath).Replace("/", "\\") + ".md";
            string path3 = link.Replace(docsetAliasPath, docsetAbsoluteBasePath).Replace("/", "\\") + ".md";

            if (File.Exists(path1) && File.Exists(path2) && File.Exists(path3))
                throw new Exception("Ambiguous absolute path.");
            if (File.Exists(path1))
                return path1;
            if (File.Exists(path2))
                return path2;
            if (File.Exists(path3))
                return path3;

            // Return null if the article doesn't exist at any of the potential locations.
            return null;
        }

        #endregion
    }
}