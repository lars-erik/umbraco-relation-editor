using umbraco.interfaces;

namespace Umbraco.RelationEditor
{
    public class EditRelationsAction : IAction
    {
        public char Letter { get { return 'r'; } }
        public bool ShowInNotifier { get { return true; } }
        public bool CanBePermissionAssigned { get { return false; } }
        public string Icon { get { return "trafic"; } }
        public string Alias { get { return "editRelations"; } }
        public string JsFunctionName { get { return "relationeditor.edit()"; } }
        public string JsSource { get { return null; } }
    }
}