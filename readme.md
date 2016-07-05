##Umbraco 7 Relation Editor

<code style="
    -moz-border-radius: 5px;
    -webkit-border-radius: 5px;
    background-color: #202020;
    border: 4px solid silver;
    border-radius: 5px;
    box-shadow: 2px 2px 3px #6e6e6e;
    color: #e2e2e2;
    display: block;
    font: 1.5em 'andale mono','lucida console',monospace;
    line-height: 1.5em;
    overflow: auto;
    padding: 15px;
">PM > install-package Umbraco.RelationEditor</code>

[![IMAGE ALT TEXT HERE](http://img.youtube.com/vi/xU3ifl_6xtk/0.jpg)](http://www.youtube.com/watch?v=xU3ifl_6xtk)

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

