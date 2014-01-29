Umbraco 7 Relation Editor

The editor will appear for supported objects after they're configured in /config/relationeditor.config.

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

Document and Media need a specified Alias to be enabled.
Alias is the alias of the respective type.
IE. <ObjectType Name="Document" Alias="myDocumentTypeAlias">

Types does not need, nor should have alias.
IE. <ObjectType Name="DocumentType"/>

Relations to be displayed has to be enabled by adding EnabledRelation elements.
IE. <EnabledRelation Alias="myRelationType"/>

For documents and media, you can limit which types of related items to support.
IE. <EnabledChildType Alias="otherDocumentTypeAlias"/>

If no enabled child types are enabled, all types will be selectable.

Note: Names and aliases are case sensitive.

Here's a sample configuration:
<RelationEditor>
  <ObjectType Name="Document" Alias="page">
    <EnabledRelation Alias="documentRelation">
      <EnabledChildType Alias="post"/>
    </EnabledRelation>
    <EnabledRelation Alias="docMediaRel">
      <EnabledChildType Alias="Image"/>
    </EnabledRelation>
  </ObjectType>
  <ObjectType Name="Document" Alias="post">
    <EnabledRelation Alias="documentRelation">
      <EnabledChildType Alias="page"/>
    </EnabledRelation>
  </ObjectType>
  <ObjectType Name="DocumentType">
    <EnabledRelation Alias="typeToType"/>
  </ObjectType>
</RelationEditor>