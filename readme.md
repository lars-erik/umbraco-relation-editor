##Umbraco 7 Relation Editor

<div style="position: relative;	padding-bottom: 56.25%; padding-top: 25px; height: 0;">
<iframe width="560" height="315" src="https://www.youtube.com/embed/xU3ifl_6xtk" frameborder="0" allowfullscreen style="position: absolute; top: 0; left: 0; width: 100%; height: 100%;"></iframe>
</div>

The editor will appear for supported objects after they're configured.

Supported ObjectTypes are
* Document
* Media
* DocumentType
* MediaType

Supported relation types are
* Document -> Document
* Document -> Media
* Media -> Document
* Media -> Media
* DocumentType -> DocumentType
* DocumentType -> MediaType
* MediaType -> DocumentType
* MediaType -> MediaType

To enable relations on an object type, right click its definition and select "Enable Relations".
For Document- and Media types, you can select which kinds types to enable the relation for.
You can for instance configure a relation to just accept one type of document type.
Once a relation type is enabled for an object type, you can right click individual objects and relate them.
Enabling a relation on a document type for example enables the "Edit Relations" action in its context menu.

There are extensions for IPublishedContent that can fetch related content.  
These can be found in the `Umbraco.RelationEditor.Extensions` namespace.  
Example usage:

    var relatedBlogposts = Model.Content.Related<IPublishedContent>("relatedBlogposts");
    var relatedTextPages = Model.Content.RelatedParents<TextPage>("relatedTextpages");
    var relatedFiles = Model.Content.RelatedChildren<File>("relatedFiles");

