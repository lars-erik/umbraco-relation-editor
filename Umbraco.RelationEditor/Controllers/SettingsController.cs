using System.Collections.Generic;
using System.Linq;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Umbraco.RelationEditor.Controllers
{
    [UmbracoApplicationAuthorize("content")]
    [PluginController("RelationsEditor")]
    public class SettingsController : UmbracoAuthorizedApiController
    {
        private IContentTypeService ContentTypeService
        {
            get { return ApplicationContext.Services.ContentTypeService; }
        }

        public object GetConfiguration(string type, int id)
        {
            var legacyTreeNodeType = new TreeNodeType("settings", type);
            var sevenThreeTreeNodeType = new TreeNodeType(type, null);
            var objectType = Mappings.TreeNodeObjectTypes.ContainsKey(legacyTreeNodeType) ?
                Mappings.TreeNodeObjectTypes[legacyTreeNodeType] :
                Mappings.TreeNodeObjectTypes[sevenThreeTreeNodeType];
            var contentTypes = ContentTypeService.GetAllContentTypes().ToList();
            var mediaTypes = ApplicationContext.Services.ContentTypeService.GetAllMediaTypes().ToList();

            var types = new[]
            {
                (objectType == UmbracoObjectTypes.DocumentType ? UmbracoObjectTypes.Document : UmbracoObjectTypes.Media).GetGuid(),
            };

            var relationTypes = ApplicationContext.Services.RelationService.GetAllRelationTypes()
                .Where(rt => types.Contains(rt.ParentObjectType))
                ;

            var contentType = objectType == UmbracoObjectTypes.DocumentType ? 
                (IContentTypeBase)contentTypes.Single(ct => ct.Id == id) :
                mediaTypes.Single(ct => ct.Id == id);
            var contentObjectType = objectType == UmbracoObjectTypes.DocumentType
                ? UmbracoObjectTypes.Document
                : UmbracoObjectTypes.Media;

            return new
            {
                contentTypes,
                mediaTypes,
                relationTypes,
                configuration = RelationEditor.Configuration.Get(contentObjectType, contentType.Alias)
            };
        }

        public void SaveConfiguration(SaveConfigurationCommand saveConfigurationCommand)
        {
            var legacyTreeNodeType = new TreeNodeType("settings", saveConfigurationCommand.Type);
            var sevenThreeTreeNodeType = new TreeNodeType(saveConfigurationCommand.Type, null);
            var objectType = Mappings.TreeNodeObjectTypes.ContainsKey(legacyTreeNodeType) ?
                Mappings.TreeNodeObjectTypes[legacyTreeNodeType] :
                Mappings.TreeNodeObjectTypes[sevenThreeTreeNodeType];
            var contentTypes = ContentTypeService.GetAllContentTypes().ToList();
            var mappedObjectType = objectType == UmbracoObjectTypes.DocumentType ? 
                UmbracoObjectTypes.Document : 
                UmbracoObjectTypes.Media;
            var contentType = objectType == UmbracoObjectTypes.DocumentType ?
                (IContentTypeBase)ContentTypeService.GetContentType(saveConfigurationCommand.Id) :
                ContentTypeService.GetMediaType(saveConfigurationCommand.Id);
            RelationEditor.Configuration.Set(mappedObjectType, contentType.Alias, saveConfigurationCommand.Configuration);
        }
    }
}
