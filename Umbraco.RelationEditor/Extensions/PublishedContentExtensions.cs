using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace Umbraco.RelationEditor.Extensions
{
    public static class PublishedContentExtensions
    {
        public static IEnumerable<T> RelatedChildren<T>(this IPublishedContent content, string relationAlias)
        {
            return GetRelatedContent<T>(
                relationAlias, 
                RelationService.GetByParentId(content.Id), 
                r => r.ChildId
                );
        }

        public static IEnumerable<T> RelatedParents<T>(this IPublishedContent content, string relationAlias)
        {
            return GetRelatedContent<T>(
                relationAlias,
                RelationService.GetByChildId(content.Id),
                r => r.ParentId
                );
        }

        public static IEnumerable<T> Related<T>(this IPublishedContent content, string relationAlias)
        {
            return GetRelatedContent<T>(
                relationAlias,
                RelationService.GetByParentOrChildId(content.Id),
                r => content.Id == r.ChildId ? r.ParentId : r.ChildId
                );
        }

        private static UmbracoHelper Umbraco
        {
            get { return new UmbracoHelper(UmbracoContext.Current); }
        }

        private static IRelationService RelationService
        {
            get { return UmbracoContext.Current.Application.Services.RelationService; }
        }

        private static IEnumerable<T> GetRelatedContent<T>(string relationAlias, IEnumerable<IRelation> relations, Func<IRelation, int> selector)
        {
            return Umbraco.TypedContent(
                relations
                    .Where(r => r.RelationType.Alias.InvariantEquals(relationAlias))
                    .OrderBy(r =>
                    {
                        int value;
                        int.TryParse(r.Comment, out value);
                        return value;
                    })
                    .Select(selector)
                )
                .OfType<T>();
        }
    }
}
