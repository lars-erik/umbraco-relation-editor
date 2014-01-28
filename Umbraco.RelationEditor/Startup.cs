using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI.WebControls;
using Umbraco.Core;
using Umbraco.Web.Trees;

namespace Umbraco.RelationEditor
{
    public class Startup : ApplicationEventHandler
    {
        private readonly TreeNodeType[] treeNodeTypes =
        {
            new TreeNodeType("content", null),
            new TreeNodeType("media", null),
            new TreeNodeType("settings", "nodeTypes"), 
            new TreeNodeType("settings", "mediaTypes")
        };
        //private readonly string[] types = {}

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            base.ApplicationStarted(umbracoApplication, applicationContext);

            TreeControllerBase.MenuRendering += TreeControllerBaseOnMenuRendering;
            TreeControllerBase.TreeNodesRendering += TreeControllerBaseOnTreeNodesRendering;
        }

        private void TreeControllerBaseOnTreeNodesRendering(TreeControllerBase sender, TreeNodesRenderingEventArgs eventArgs)
        {
            
        }

        private void TreeControllerBaseOnMenuRendering(TreeControllerBase sender, MenuRenderingEventArgs eventArgs)
        {
            var context = new HttpContextWrapper(HttpContext.Current);
            var urlHelper = new UrlHelper(new RequestContext(context, new RouteData()));
            var treeNodeType = new TreeNodeType(sender.TreeAlias ?? eventArgs.QueryStrings.Get("section"), eventArgs.QueryStrings.Get("treeType"));
            if (treeNodeTypes.Contains(treeNodeType) && Convert.ToInt32(eventArgs.NodeId) > 0)
            {
                var menuItem = eventArgs.Menu.Items.Add<EditRelationsAction>("Edit relations");
                menuItem.LaunchDialogView(
                    urlHelper.Content("~/App_Plugins/RelationEditor/editrelations.html"),
                    "Edit relations"
                );
            }
        }
    }

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
