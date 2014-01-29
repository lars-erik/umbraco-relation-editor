using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.RelationEditor;

static internal class Mappings
{
    public static readonly Dictionary<TreeNodeType, UmbracoObjectTypes> TreeNodeObjectTypes = new Dictionary<TreeNodeType, UmbracoObjectTypes>
    {
        { new TreeNodeType("content", null), UmbracoObjectTypes.Document },
        { new TreeNodeType("media", null), UmbracoObjectTypes.Media },
        { new TreeNodeType("settings", "nodeTypes"), UmbracoObjectTypes.DocumentType },
        { new TreeNodeType("settings", "mediaTypes"), UmbracoObjectTypes.MediaType }
    };

    public static readonly Dictionary<Guid, TreeNodeType> ObjectTypeTreeTypes = new Dictionary<Guid, TreeNodeType>
    {
        { UmbracoObjectTypes.Document.GetGuid(), new TreeNodeType("content", null) },
        { UmbracoObjectTypes.Media.GetGuid(), new TreeNodeType("media", null) },
        { UmbracoObjectTypes.DocumentType.GetGuid(), new TreeNodeType("settings", "nodeTypes") },
        { UmbracoObjectTypes.MediaType.GetGuid(), new TreeNodeType("settings", "mediaTypes") }
    };

    public static readonly Dictionary<UmbracoObjectTypes, UmbracoObjectTypes[]> AllowedRelations = new Dictionary<UmbracoObjectTypes, UmbracoObjectTypes[]>
    {
        {UmbracoObjectTypes.DocumentType, new[] {UmbracoObjectTypes.DocumentType, UmbracoObjectTypes.MediaType}},
        {UmbracoObjectTypes.MediaType, new[] {UmbracoObjectTypes.DocumentType, UmbracoObjectTypes.MediaType}},
        {UmbracoObjectTypes.Document, new[] {UmbracoObjectTypes.Document, UmbracoObjectTypes.Media}},
        {UmbracoObjectTypes.Media, new[] {UmbracoObjectTypes.Document, UmbracoObjectTypes.Media}},
    };

    public static readonly TreeNodeType[] TreeNodeTypes =
    {
        new TreeNodeType("content", null),
        new TreeNodeType("media", null),
        new TreeNodeType("settings", "nodeTypes"), 
        new TreeNodeType("settings", "mediaTypes")
    };
}