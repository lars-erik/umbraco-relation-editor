using System;
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

            if (sender.TreeAlias == "content")
            {
                var menuItem = eventArgs.Menu.Items.Add<EditRelationsAction>("Edit relations");
                menuItem.LaunchDialogView(
                    urlHelper.Content("~/App_Plugins/RelationEditor/editrelations.html"), 
                    "Edit relations"
                );
            }
        }
    }
}
