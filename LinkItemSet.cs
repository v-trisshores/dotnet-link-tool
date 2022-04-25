namespace DocsLinkTool
{
    internal class LinkItemSet
    {
        internal string ArticlePath;
        internal bool IsLinkChanged;
        internal bool IsRedirected;
        internal List<LinkItem> Items = new();

        public LinkItemSet(string articlePath)
        {
            ArticlePath = articlePath;
        }
    }

    internal class LinkItem
    {
        internal string Action;
        internal string Link;

        public LinkItem(string action, string link)
        {
            Action = action;
            Link = link;
        }
    }
}