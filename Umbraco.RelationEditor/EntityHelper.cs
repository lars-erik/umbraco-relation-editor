using Umbraco.Core.Models;
using Umbraco.Web;

namespace Umbraco.RelationEditor
{
    internal static class EntityHelper
    {
        public static string FindAlias(UmbracoObjectTypes objectType, int id)
        {
            var item = UmbracoContext.Current.Application.Services.EntityService.Get(id, objectType);
            if (item != null && item.AdditionalData != null){
                object alias = null;
                item.AdditionalData.TryGetValue("Alias", out alias);
                return alias as string;
            } else {
                return null;
            }
        }
    }
}
