using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI.WebControls;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Trees;

namespace Umbraco.RelationEditor
{
    public class RelationEditorEvents : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            base.ApplicationStarted(umbracoApplication, applicationContext);

            TreeControllerBase.MenuRendering += TreeControllerBaseOnMenuRendering;
            TreeControllerBase.TreeNodesRendering += TreeControllerBaseOnTreeNodesRendering;
        }

        private void TreeControllerBaseOnTreeNodesRendering(TreeControllerBase sender, TreeNodesRenderingEventArgs eventArgs)
        {
            if (eventArgs.QueryStrings.HasKey("relationEditor"))
            {
                var parentType = (UmbracoObjectTypes)Enum.Parse(typeof(UmbracoObjectTypes), eventArgs.QueryStrings.Get("parentType"));
                var parentAlias = eventArgs.QueryStrings.Get("parentTypeAlias");
                var relationAlias = eventArgs.QueryStrings.Get("relationAlias");

                var config = Configuration.Get(parentType, parentAlias);
                if (!config.Enabled)
                    return;

                var relConfig = config.Get(relationAlias);
                if (!relConfig.Enabled)
                    return;

                var childTypes = relConfig.EnabledChildTypes.Select(t => t.Alias).ToArray();

                if (!childTypes.Any())
                    return;

                var relation = UmbracoContext.Current.Application.Services.RelationService.GetRelationTypeByAlias(relationAlias);
                var childObjectType = UmbracoObjectTypesExtensions.GetUmbracoObjectType(relation.ChildObjectType);

                foreach (var node in eventArgs.Nodes)
                {
                    var id = Convert.ToInt32(node.Id);
                    var alias = EntityHelper.FindAlias(childObjectType, id);
                    if (!childTypes.Contains(alias))
                    {
                        node.SetNotPublishedStyle();
                        node.AdditionalData.Add("relationDisallowed", "true");
                    }
                }
            }
        }

        private void TreeControllerBaseOnMenuRendering(TreeControllerBase sender, MenuRenderingEventArgs eventArgs)
        {
            var context = new HttpContextWrapper(HttpContext.Current);
            var urlHelper = new UrlHelper(new RequestContext(context, new RouteData()));
            var treeNodeType = new TreeNodeType(sender.TreeAlias ?? eventArgs.QueryStrings.Get("section"), eventArgs.QueryStrings.Get("treeType"));
            var objectType = Mappings.TreeNodeObjectTypes.ContainsKey(treeNodeType) ?
                Mappings.TreeNodeObjectTypes[treeNodeType] :
                UmbracoObjectTypes.Unknown;
            if (objectType != UmbracoObjectTypes.Unknown && Convert.ToInt32(eventArgs.NodeId) > 0)
            {
                var type = Mappings.TreeNodeObjectTypes[treeNodeType];
                var id = Convert.ToInt32(eventArgs.NodeId);
                var alias = EntityHelper.FindAlias(type, id);
                var typeConfig = Configuration.Get(type, alias);
                if (!typeConfig.Enabled || !typeConfig.EnabledRelations.Any())
                    return;
                
                var menuItem = eventArgs.Menu.Items.Add<EditRelationsAction>("Edit relations");
                menuItem.LaunchDialogView(
                    urlHelper.Content("~/App_Plugins/RelationEditor/editrelations.html"),
                    "Edit relations"
                );
            }
        }
    }
}
