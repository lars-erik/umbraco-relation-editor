using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using NUnit.Framework;
using Umbraco.Core.Models;

namespace Umbraco.RelationEditor.Tests
{
    [TestFixture]
    public class ConfigSerializationTests
    {
        private const string Input = @"
                <RelationEditor>
                  <ObjectType Name=""Document"" Alias=""page"">
                    <EnabledRelation Alias=""pagePostRelation"">
                      <EnabledChildType Alias=""post""/>
                    </EnabledRelation>
                    <EnabledRelation Alias=""pageNewsPostRelation"" />
                  </ObjectType>
                  <ObjectType Name=""Document"" ObjectTypeAlias=""Content"" AllowInheritance=""true"">
                    <EnabledRelation Alias=""pagePostRelation"" />
                  </ObjectType>
                </RelationEditor>
                ";

        [Test]
        public void SerializeConfiguration()
        {
            var config = new RelationEditorConfiguration
            {
                ObjectTypes = new List<ObjectTypeConfiguration>
                {
                    new ObjectTypeConfiguration
                    {
                        Name = UmbracoObjectTypes.Document,
                        Alias = "Page",
                        EnabledRelations = new List<EnabledRelationConfiguration>
                        {
                            new EnabledRelationConfiguration{Alias="PagePostRelation"},
                            new EnabledRelationConfiguration{Alias="PageNewsPostRelation"},
                        }
                    }
                }
            };

            var stringBuilder = new StringBuilder();
            var writer = new StringWriter(stringBuilder);
            var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings {OmitXmlDeclaration = true, Indent = true});
            var namespaces = new XmlSerializerNamespaces(new[] { new XmlQualifiedName("") });

            var serializer = new XmlSerializer(typeof(RelationEditorConfiguration));
            serializer.Serialize(xmlWriter, config, namespaces);
            writer.Flush();
            Console.WriteLine(stringBuilder.ToString());
        }

        [Test]
        public void DeserializeConfiguration()
        {
            var config = DeserializeFromInput();
            Assert.IsNotNull(config);
            Assert.AreEqual(2, config.ObjectTypes.Count);
            Assert.IsFalse(config.ObjectTypes[0].AllowInheritance);
            Assert.IsTrue(config.ObjectTypes[1].AllowInheritance);
            Assert.AreEqual("pagePostRelation", config.ObjectTypes[1].EnabledRelations[0].Alias);
            Assert.AreEqual(UmbracoObjectTypes.Document, config.ObjectTypes[1].Name);
            Assert.AreEqual("post", config.ObjectTypes[0].EnabledRelations[0].EnabledChildTypes[0].Alias);
        }

        [Test]
        public void GetMethods()
        {
            var config = DeserializeFromInput();
            Assert.IsFalse(config.Get(UmbracoObjectTypes.Member, "").Enabled);
            Assert.IsTrue(config.Get(UmbracoObjectTypes.Document, "page").Enabled);
            Assert.IsFalse(config.Get(UmbracoObjectTypes.Document, "page").Get("invalidRelation").Enabled);
            Assert.IsTrue(config.Get(UmbracoObjectTypes.Document, "page").Get("pagePostRelation").Enabled);
            Assert.IsFalse(config.Get(UmbracoObjectTypes.Document, "page").Get("pagePostRelation").Get("invalidChild").Enabled);
            Assert.IsTrue(config.Get(UmbracoObjectTypes.Document, "page").Get("pagePostRelation").Get("post").Enabled);
        }

        private static RelationEditorConfiguration DeserializeFromInput()
        {
            var serializer = new XmlSerializer(typeof (RelationEditorConfiguration));
            var reader = new StringReader(Input);
            var config = (RelationEditorConfiguration) serializer.Deserialize(reader);
            return config;
        }
    }
}
