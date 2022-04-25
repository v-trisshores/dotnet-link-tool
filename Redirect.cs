namespace DocsLinkTool
{
    public partial class UpdateLinks
    {
        internal class Redirect
        {
            private string _srcPath;
            private string _dstPath;
            internal string _srcVersion;
            internal string _dstVersion;

            internal string SrcPath
            {
                get
                {
                    return _srcPath;
                }
                set
                {
                    _srcPath = value[..value.IndexOf("?")];
                }
            }

            internal string DstPath
            {
                get
                {
                    return _dstPath;
                }
                set
                {
                    _dstPath = value;
                }
            }

            internal string SrcVersion
            {
                get
                {
                    return _srcVersion;
                }
                set
                {
                    _srcVersion = value;
                }
            }

            internal string DstVersion
            {
                get
                {
                    return _dstVersion;
                }
                set
                {
                    _dstVersion = value;
                }
            }
        }
    }
}