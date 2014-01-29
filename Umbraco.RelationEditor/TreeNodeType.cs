namespace Umbraco.RelationEditor
{
    public struct TreeNodeType
    {
        public string Section;
        public string TreeType;

        public TreeNodeType(string section, string treeType)
        {
            Section = section;
            TreeType = treeType;
        }
    }
}