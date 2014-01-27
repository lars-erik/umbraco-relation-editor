using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using System.Web.Http.Controllers;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Umbraco.RelationEditor
{
    [UmbracoApplicationAuthorize("content")]
    [PluginController("RelationsEditor")]
    public class RelationsController : UmbracoApiController
    {
        private readonly IRelationService relationService;
        private readonly IContentService contentService;

        public RelationsController()
        {
            relationService = ApplicationContext.Services.RelationService;
            contentService = ApplicationContext.Services.ContentService;
        }

        public string[] GetObjectTypes()
        {
            return Enum.GetNames(typeof (UmbracoObjectTypes));
        }

        public ContentRelationsDto GetRelations(
            string from,
            string to,
            int parentId
            )
        {
            UmbracoObjectTypes fromType;
            UmbracoObjectTypes toType;

            if (
                !Enum.TryParse(from, out fromType) ||
                !Enum.TryParse(to, out toType) ||
                fromType == UmbracoObjectTypes.Unknown ||
                toType == UmbracoObjectTypes.Unknown
            )
                throw new Exception("Cannot get relation types for unknown object type");

            if (fromType != UmbracoObjectTypes.Document || toType != UmbracoObjectTypes.Document)
                throw new Exception("Haven't implemented anything but document relations yet");

            var allRelations = relationService.GetByParentOrChildId(parentId);
            var relationSets = relationService.GetAllRelationTypes()
                .Where(rt => rt.ParentObjectType == fromType.GetGuid() && rt.ChildObjectType == toType.GetGuid())
                .Select(rt => new RelationSetDto
                {
                    RelationTypeId = rt.Id,
                    Alias = rt.Alias,
                    Name = rt.Name,
                    Relations = allRelations
                        .Where(r => r.RelationTypeId == rt.Id)
                        .Select(r => new RelationDto
                        {
                            ChildId = r.ChildId,
                            ChildName = contentService.GetById(r.ChildId).Name,
                            State = RelationStateEnum.Unmodified
                        }).ToList()
                }).ToList();

            return new ContentRelationsDto
            {
                ParentId = parentId,
                Sets = relationSets
            };
        }

        [HttpPost]
        public void SaveRelations(ContentRelationsDto contentRelations)
        {
            if (contentRelations.Sets == null || !contentRelations.Sets.Any())
                return;
            var relations = relationService.GetByParentId(contentRelations.ParentId).ToList();
            foreach (var set in contentRelations.Sets)
            {
                var typeId = set.RelationTypeId;
                var type = relationService.GetRelationTypeById(set.RelationTypeId);
                var setRelations = relations.Where(r => r.RelationTypeId == typeId);
                foreach (var removeRelation in setRelations)
                    relationService.Delete(removeRelation);

                foreach (var relation in set.Relations)
                {
                    if (relation.State == RelationStateEnum.Deleted)
                        continue;

                    relationService.Save(new Relation(contentRelations.ParentId, relation.ChildId, type));
                }
            }
        }
    }

    public class ContentRelationsDto
    {
        public int ParentId { get; set; }
        public IList<RelationSetDto> Sets { get; set; }         
    }

    public class RelationSetDto
    {
        public int RelationTypeId { get; set; }
        public string Alias { get; set; }
        public string Name { get; set; }
        public IList<RelationDto> Relations { get; set; }
    }

    public class RelationDto
    {
        public int ChildId { get; set; }
        public string ChildName { get; set; }
        public RelationStateEnum State { get; set; }
    }

    public enum RelationStateEnum
    {
        New,
        Deleted,
        Unmodified
    }

    /// <summary>
    /// Ensures that the current user has access to the specified application
    /// </summary>
    internal sealed class UmbracoApplicationAuthorizeAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Can be used by unit tests to enable/disable this filter
        /// </summary>
        internal static bool Enable = true;

        private readonly string[] _appNames;

        /// <summary>
        /// Constructor to set any number of applications that the user needs access to to be authorized
        /// </summary>
        /// <param name="appName">
        /// If the user has access to any of the specified apps, they will be authorized.
        /// </param>
        public UmbracoApplicationAuthorizeAttribute(params string[] appName)
        {
            _appNames = appName;
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            if (Enable == false)
            {
                return true;
            }

            return UmbracoContext.Current.UmbracoUser != null
                   && _appNames.Any(app => UmbracoContext.Current.UmbracoUser.GetApplications().Any(a => app == a.alias));
        }
    }
}
