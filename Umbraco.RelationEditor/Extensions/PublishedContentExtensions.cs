using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace Umbraco.RelationEditor.Extensions
{
    public static class PublishedContentExtensions
    {
        public static IEnumerable<T> RelatedChildren<T>(this IPublishedContent content, string relationAlias)
        {
            return GetRelatedContent<T>(
                RelationsByParent<T>(content, relationAlias), 
                r => r.ChildId
                );
        }

        public static IEnumerable<T> RelatedParents<T>(this IPublishedContent content, string relationAlias)
        {
            return GetRelatedContent<T>(
                RelationsByChild<T>(content, relationAlias),
                r => r.ParentId
                );
        }

        public static IEnumerable<T> Related<T>(this IPublishedContent content, string relationAlias)
        {
            return GetRelatedContent<T>(
                RelationsByAny<T>(content, relationAlias),
                r => content.Id == r.ChildId ? r.ParentId : r.ChildId
                );
        }

        private static IEnumerable<RelationRecord> RelationsByParent<T>(IPublishedContent content, string alias)
        {
            return Query(alias, "parentId = " + content.Id);
        }

        private static IEnumerable<RelationRecord> RelationsByChild<T>(IPublishedContent content, string alias)
        {
            return Query(alias, "childId = " + content.Id);
        }

        private static IEnumerable<RelationRecord> RelationsByAny<T>(IPublishedContent content, string alias)
        {
            return Query(alias, "parentId = " + content.Id + " OR childId = " + content.Id);
        }

        private static IEnumerable<RelationRecord> Query(string alias, string predicate)
        {
            var uow = new PetaPocoUnitOfWorkProvider(ApplicationContext.Current.ProfilingLogger.Logger).GetUnitOfWork();
            var relations = uow.Database.Query<RelationRecord>(@"
                SELECT ParentId, ChildId, Comment
                FROM umbracoRelation
                INNER JOIN umbracoRelationType ON umbracoRelationType.alias = '" + alias + @"' AND umbracoRelationType.id = umbracoRelation.relType
                WHERE 
                (" + predicate + ")").ToList();
            return relations;
        }

        private static UmbracoHelper Umbraco
        {
            get { return new UmbracoHelper(UmbracoContext.Current); }
        }

        private static IRelationService RelationService
        {
            get { return UmbracoContext.Current.Application.Services.RelationService; }
        }

        private static IEnumerable<T> GetRelatedContent<T>(IEnumerable<RelationRecord> relations, Func<RelationRecord, int> selector)
        {
            var relatedIds = relations.OrderBy(r => r.Order).Select(selector);
            var relatedContent = Umbraco.TypedContent(relatedIds);
            return relatedContent.OfType<T>();
        }

        public class RelationRecord
        {
            public int ParentId { get; set; }
            public int ChildId { get; set; }

            public string Comment { get; set; }

            public int Order
            {
                get
                {
                    int value;
                    int.TryParse(Comment, out value);
                    return value;
                }
            }
        }
    }
}
