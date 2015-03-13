using umbraco.interfaces;

namespace Umbraco.RelationEditor
{
    public class EditRelationsAction : IAction
    {
        public char Letter { get { return (char)0x1337; } }
        public bool ShowInNotifier { get { return true; } }
        public bool CanBePermissionAssigned { get { return false; } }
        public string Icon { get { return "trafic"; } }
        public string Alias { get { return "editRelations"; } }
        public string JsFunctionName { get { return "relationeditor.edit()"; } }
        public string JsSource { get { return null; } }
    }

    public class EnableRelationsAction : IAction
    {
        public char Letter { get { return (char)0x7331; } }
        public bool ShowInNotifier { get { return true; } }
        public bool CanBePermissionAssigned { get { return false; } }
        public string Icon { get { return "trafic"; } }
        public string Alias { get { return "enableRelations"; } }
        public string JsFunctionName { get { return "relationeditor.enable()"; } }
        public string JsSource { get { return null; } }
    }
}