using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;

namespace Umbraco.RelationEditor.Extensions
{
    public static class RelationExtensions
    {
        public static int Order(this IRelation relation)
        {
            int.TryParse(relation.Comment, out var order);
            return order;
        }
    }
}
