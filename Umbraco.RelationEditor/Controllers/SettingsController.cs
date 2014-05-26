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
    public class SettingsController : UmbracoApiController
    {
        private IContentTypeService ContentTypeService
        {
            get { return ApplicationContext.Services.ContentTypeService; }
        }

        public object GetConfiguration(string type, int id)
        {
            var objectType = Mappings.TreeNodeObjectTypes[new TreeNodeType("settings", type)];
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
            var objectType = Mappings.TreeNodeObjectTypes[new TreeNodeType("settings", saveConfigurationCommand.Type)];
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
