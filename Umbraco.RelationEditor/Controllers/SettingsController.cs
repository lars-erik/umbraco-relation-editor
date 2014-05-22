using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Umbraco.RelationEditor.Controllers
{
    [UmbracoApplicationAuthorize("content")]
    [PluginController("RelationsEditor")]
    public class SettingsController : UmbracoApiController
    {

    }
}
