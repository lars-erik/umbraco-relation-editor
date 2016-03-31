using Umbraco.Core.Models;
using Umbraco.Web;

static internal class EntityHelper
{
    public static string FindAlias(UmbracoObjectTypes objectType, int id)
    {
        if (objectType == UmbracoObjectTypes.DocumentType)
        {
            var docType = UmbracoContext.Current.Application.Services.ContentTypeService.GetContentType(id);
            if (docType == null)
                return null;
            return docType.Alias;
        }

        var item = UmbracoContext.Current.Application.Services.EntityService.Get(id, objectType);
        if (item == null)
            return null;

        object alias = null;
        item.AdditionalData.TryGetValue("Alias", out alias);
        return alias as string;
    }
}