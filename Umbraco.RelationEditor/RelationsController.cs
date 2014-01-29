using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using System.Web.Http.Controllers;
using umbraco;
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
        private readonly IMediaService mediaService;
        private readonly IContentTypeService contentTypeService;
        private readonly IEntityService entityService;

        public RelationsController()
        {
            relationService = ApplicationContext.Services.RelationService;
            contentService = ApplicationContext.Services.ContentService;
            mediaService = ApplicationContext.Services.MediaService;
            contentTypeService = ApplicationContext.Services.ContentTypeService;
            entityService = ApplicationContext.Services.EntityService;
        }

        public string[] GetObjectTypes()
        {
            return Enum.GetNames(typeof (UmbracoObjectTypes));
        }

        [HttpGet]
        public ContentRelationsDto GetRelations(
            string section,
            string treeType,
            int parentId
            )
        {
            var treeNodeType = new TreeNodeType(section, treeType);
            var fromType = UmbracoObjectTypes.Unknown;

            if (
                !Mappings.TreeNodeObjectTypes.TryGetValue(treeNodeType, out fromType)
                || fromType == UmbracoObjectTypes.Unknown
            )
                throw new Exception("Cannot get relation types for unknown object type");

            var entity = entityService.Get(parentId, fromType);
            object alias = null;
            entity.AdditionalData.TryGetValue("Alias", out alias);
            var typeConfig = RelationEditor.Configuration.Get(fromType, alias as string);

            var allRelations = relationService.GetByParentOrChildId(parentId);
            var allowedObjectTypes = Mappings.AllowedRelations[fromType];
            var enabledRelations = typeConfig.EnabledRelations.Select(r => r.Alias).ToArray();
            var relationSets = relationService.GetAllRelationTypes()
                .Where(rt => 
                    rt.ParentObjectType == fromType.GetGuid() && 
                        enabledRelations.Contains(rt.Alias) &&
                        allowedObjectTypes.Any(ar => ar.GetGuid() == rt.ChildObjectType)
                )
                .Select(rt => new RelationSetDto
                {
                    RelationTypeId = rt.Id,
                    Direction = rt.IsBidirectional ? "bidirectional" : "parentchild",
                    ChildType = Mappings.ObjectTypeTreeTypes[rt.ChildObjectType],
                    Alias = rt.Alias,
                    Name = rt.Name,
                    Relations = allRelations
                        .Where(r => 
                            r.RelationTypeId == rt.Id &&
                            (rt.IsBidirectional || r.ParentId == parentId)
                        )
                        .Select(r =>
                        {
                            int otherId;
                            string otherName;
                            if (r.ParentId == parentId)
                            {
                                otherId = r.ChildId;
                                otherName = GetChildName(rt.ChildObjectType, r.ChildId);
                            }
                            else
                            {
                                otherId = r.ParentId;
                                otherName = GetChildName(rt.ParentObjectType, r.ParentId);
                            }
                            return new RelationDto
                            {
                                ChildId = otherId,
                                ChildName = otherName,
                                State = RelationStateEnum.Unmodified
                            };
                        }).ToList()
                }).ToList();

            return new ContentRelationsDto
            {
                ParentId = parentId,
                ParentType = fromType,
                ParentAlias = alias as string,
                Sets = relationSets
            };
        }

        [HttpPost]
        public void SaveRelations(ContentRelationsDto contentRelations)
        {
            if (contentRelations.Sets == null || !contentRelations.Sets.Any())
                return;
            
            var relations = relationService.GetByParentId(contentRelations.ParentId).ToList();
            var parentEntity = entityService.Get(contentRelations.ParentId, contentRelations.ParentType);
            
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

                    var childEntity = entityService.Get(relation.ChildId, UmbracoObjectTypesExtensions.GetUmbracoObjectType(type.ChildObjectType));
                    relationService.Relate(parentEntity, childEntity, type);
                }
            }
        }

        [HttpGet]
        public IsAllowedResult IsAllowedEntity(string parentTypeName, string parentAlias, string relationAlias, string treeAlias, int id)
        {
            var parentType = (UmbracoObjectTypes)Enum.Parse(typeof(UmbracoObjectTypes), parentTypeName);
            var config = RelationEditor.Configuration.Get(parentType, parentAlias);
            var relConfig = config.Get(relationAlias);
            if (relConfig.Enabled && !relConfig.EnabledChildTypes.Any())
                return new IsAllowedResult(true);
            var treeNodeType = new TreeNodeType(treeAlias, null);
            if (Mappings.TreeNodeTypes.Contains(treeNodeType))
            {
                var objectType = Mappings.TreeNodeObjectTypes[treeNodeType];
                var alias = EntityHelper.FindAlias(objectType, id);
                return new IsAllowedResult(relConfig.Get(alias).Enabled);
            }
            return new IsAllowedResult(false);
        }

        private string GetChildName(Guid childObjectType, int childId)
        {
            switch (UmbracoObjectTypesExtensions.GetUmbracoObjectType(childObjectType))
            {
                case UmbracoObjectTypes.Document:
                    return contentService.GetById(childId).Name;
                case UmbracoObjectTypes.Media:
                    return mediaService.GetById(childId).Name;
                case UmbracoObjectTypes.DocumentType:
                    return contentTypeService.GetContentType(childId).Name;
                case UmbracoObjectTypes.MediaType:
                    return contentTypeService.GetMediaType(childId).Name;
            }
            throw new Exception("Unknown child type");
        }
    }

    public class IsAllowedResult
    {
        public bool IsAllowed { get; set; }

        public IsAllowedResult(bool isAllowed)
        {
            IsAllowed = isAllowed;
        }
    }

    public class ContentRelationsDto
    {
        public int ParentId { get; set; }
        public UmbracoObjectTypes ParentType { get; set; }
        public string ParentAlias { get; set; }
        public IList<RelationSetDto> Sets { get; set; }         
    }

    public class RelationSetDto
    {
        public int RelationTypeId { get; set; }
        public TreeNodeType ChildType { get; set; }
        public string Alias { get; set; }
        public string Name { get; set; }
        public IList<RelationDto> Relations { get; set; }
        public string Direction { get; set; }
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
