using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using Umbraco.Core.Models;
using Formatting = Newtonsoft.Json.Formatting;

namespace Umbraco.RelationEditor.Tests
{
    [TestFixture]
    public class ConfigSerializationTests
    {
        private const string XmlToolTip = @"<RelationEditor BreadCrumbMode=""ToolTip"" BreadCrumbSeparator=""/"">
  <ObjectType Alias=""page"" Name=""Document"">
    <EnabledRelation Alias=""pagePostRelation"">
      <EnabledChildType Alias=""post"" />
    </EnabledRelation>
    <EnabledRelation Alias=""pageNewsPostRelation"" />
  </ObjectType>
  <ObjectType Alias=""post"" Name=""Document"">
    <EnabledRelation Alias=""pagePostRelation"" />
  </ObjectType>
</RelationEditor>";

        private const string XmlCaption = @"<RelationEditor BreadCrumbMode=""Caption"" BreadCrumbSeparator=""-&gt;"">
  <ObjectType Alias=""page"" Name=""Document"">
    <EnabledRelation Alias=""pagePostRelation"">
      <EnabledChildType Alias=""post"" />
    </EnabledRelation>
    <EnabledRelation Alias=""pageNewsPostRelation"" />
  </ObjectType>
  <ObjectType Alias=""post"" Name=""Document"">
    <EnabledRelation Alias=""pagePostRelation"" />
  </ObjectType>
</RelationEditor>";

        private const string JsonToolTip = @"{
  ""BreadCrumbMode"": ""ToolTip"",
  ""BreadCrumbSeparator"": ""/"",
  ""ObjectTypes"": [
    {
      ""Name"": ""Document"",
      ""Alias"": ""page"",
      ""EnabledRelations"": [
        {
          ""Alias"": ""pagePostRelation"",
          ""EnabledChildTypes"": [
            {
              ""Alias"": ""post""
            }
          ]
        },
        {
          ""Alias"": ""pageNewsPostRelation"",
          ""EnabledChildTypes"": []
        }
      ]
    },
    {
      ""Name"": ""Document"",
      ""Alias"": ""post"",
      ""EnabledRelations"": [
        {
          ""Alias"": ""pagePostRelation"",
          ""EnabledChildTypes"": []
        }
      ]
    }
  ]
}";

        private const string JsonCaption = @"{
  ""BreadCrumbMode"": ""Caption"",
  ""BreadCrumbSeparator"": ""->"",
  ""ObjectTypes"": [
    {
      ""Name"": ""Document"",
      ""Alias"": ""page"",
      ""EnabledRelations"": [
        {
          ""Alias"": ""pagePostRelation"",
          ""EnabledChildTypes"": [
            {
              ""Alias"": ""post""
            }
          ]
        },
        {
          ""Alias"": ""pageNewsPostRelation"",
          ""EnabledChildTypes"": []
        }
      ]
    },
    {
      ""Name"": ""Document"",
      ""Alias"": ""post"",
      ""EnabledRelations"": [
        {
          ""Alias"": ""pagePostRelation"",
          ""EnabledChildTypes"": []
        }
      ]
    }
  ]
}";

        [Test]
        public void SerializeConfiguration()
        {
            var config = CreateToolTipConfig();

            var stringBuilder = new StringBuilder();
            var writer = new StringWriter(stringBuilder);
            var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true });
            var namespaces = new XmlSerializerNamespaces(new[] { new XmlQualifiedName("") });

            var serializer = new XmlSerializer(typeof(RelationEditorConfiguration));
            serializer.Serialize(xmlWriter, config, namespaces);
            writer.Flush();

            Console.WriteLine(stringBuilder.ToString());

            Assert.AreEqual(XmlToolTip, stringBuilder.ToString());
        }

        [Test]
        public void SerializeConfigurationAsJson()
        {
            var config = CreateToolTipConfig();

            var output = JsonConvert.SerializeObject(config, Formatting.Indented, new StringEnumConverter());

            Console.WriteLine(output);

            Assert.AreEqual(JsonToolTip, output);
        }

        [Test]
        public void DeserializeBreadCrumbToolTipConfiguration()
        {
            var config = DeserializeFromBreadCrumbToolTipInput();
            AssertBreadCrumbToolTipDeserialized(config);
        }

        [Test]
        public void DeserializeBreadCrumbToolTipConfigurationFromJson()
        {
            var config = JsonConvert.DeserializeObject<RelationEditorConfiguration>(JsonToolTip, new StringEnumConverter());
            AssertBreadCrumbToolTipDeserialized(config);
        }

        [Test]
        public void SerializeBreadCrumbCaptionConfiguration()
        {
            var config = CreateBreadCrumbCaptionConfig();

            var stringBuilder = new StringBuilder();
            var writer = new StringWriter(stringBuilder);
            var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true });
            var namespaces = new XmlSerializerNamespaces(new[] { new XmlQualifiedName("") });

            var serializer = new XmlSerializer(typeof(RelationEditorConfiguration));
            serializer.Serialize(xmlWriter, config, namespaces);
            writer.Flush();

            Console.WriteLine(stringBuilder.ToString());

            Assert.AreEqual(XmlCaption, stringBuilder.ToString());
        }

        [Test]
        public void SerializeBreadCrumbCaptionConfigurationAsJson()
        {
            var config = CreateBreadCrumbCaptionConfig();

            var output = JsonConvert.SerializeObject(config, Formatting.Indented, new StringEnumConverter());

            Console.WriteLine(output);

            Assert.AreEqual(JsonCaption, output);
        }

        [Test]
        public void DeserializeBreadCrumbCaptionConfiguration()
        {
            var config = DeserializeFromBreadCrumbCaptionInput();
            AssertBreadCrumbCaptionDeserialized(config);
        }

        [Test]
        public void DeserializeBreadCrumbCaptionConfigurationFromJson()
        {
            var config = JsonConvert.DeserializeObject<RelationEditorConfiguration>(JsonCaption, new StringEnumConverter());
            AssertBreadCrumbCaptionDeserialized(config);
        }

        [Test]
        public void GetMethods()
        {
            var config = DeserializeFromBreadCrumbToolTipInput();
            Assert.IsFalse(config.Get(UmbracoObjectTypes.Member, "").Enabled);
            Assert.IsTrue(config.Get(UmbracoObjectTypes.Document, "page").Enabled);
            Assert.IsFalse(config.Get(UmbracoObjectTypes.Document, "page").Get("invalidRelation").Enabled);
            Assert.IsTrue(config.Get(UmbracoObjectTypes.Document, "page").Get("pagePostRelation").Enabled);
            Assert.IsFalse(config.Get(UmbracoObjectTypes.Document, "page").Get("pagePostRelation").Get("invalidChild").Enabled);
            Assert.IsTrue(config.Get(UmbracoObjectTypes.Document, "page").Get("pagePostRelation").Get("post").Enabled);
        }

        private static void AssertDeserialized(RelationEditorConfiguration config)
        {
            Assert.IsNotNull(config);
            
            Assert.AreEqual(2, config.ObjectTypes.Count);
            
            Assert.AreEqual("post", config.ObjectTypes[0].EnabledRelations[0].EnabledChildTypes[0].Alias);

            Assert.AreEqual("pagePostRelation", config.ObjectTypes[1].EnabledRelations[0].Alias);
            Assert.AreEqual(UmbracoObjectTypes.Document, config.ObjectTypes[1].Name);
        }

        private static void AssertBreadCrumbCaptionDeserialized(RelationEditorConfiguration config)
        {
            AssertDeserialized(config);

            Assert.AreEqual("->", config.BreadCrumbSeparator);
            Assert.AreEqual(config.BreadCrumbMode,BreadCrumbMode.Caption);
        }

        private static void AssertBreadCrumbToolTipDeserialized(RelationEditorConfiguration config)
        {
            AssertDeserialized(config);

            Assert.AreEqual("/", config.BreadCrumbSeparator);
            Assert.AreEqual(config.BreadCrumbMode, BreadCrumbMode.ToolTip);
        }

        private static RelationEditorConfiguration CreateToolTipConfig()
        {
            var config = new RelationEditorConfiguration
            {
                BreadCrumbSeparator = "/",
                ObjectTypes = new List<ObjectTypeConfiguration>
                {
                    new ObjectTypeConfiguration
                    {
                        Name = UmbracoObjectTypes.Document,
                        Alias = "page",
                        EnabledRelations = new List<EnabledRelationConfiguration>
                        {
                            new EnabledRelationConfiguration
                            {
                                Alias = "pagePostRelation",
                                EnabledChildTypes = new List<EnabledChildTypeConfiguration>
                                {
                                    new EnabledChildTypeConfiguration { Alias = "post" }
                                }
                            },
                            new EnabledRelationConfiguration {Alias = "pageNewsPostRelation"},
                        }
                    },
                    new ObjectTypeConfiguration
                    {
                        Name = UmbracoObjectTypes.Document,
                        Alias = "post",
                        EnabledRelations = new List<EnabledRelationConfiguration>
                        {
                            new EnabledRelationConfiguration
                            {
                                Alias = "pagePostRelation"
                            }
                        }
                    }
                }
            };
            return config;
        }

        private static RelationEditorConfiguration CreateBreadCrumbCaptionConfig()
        {
            var config = CreateToolTipConfig();
            config.BreadCrumbMode = BreadCrumbMode.Caption;
            config.BreadCrumbSeparator = "->";
            return config;
        }

        private static RelationEditorConfiguration DeserializeFromBreadCrumbToolTipInput()
        {
            var serializer = new XmlSerializer(typeof(RelationEditorConfiguration));
            var reader = new StringReader(XmlToolTip);
            var config = (RelationEditorConfiguration)serializer.Deserialize(reader);
            return config;
        }

        private static RelationEditorConfiguration DeserializeFromBreadCrumbCaptionInput()
        {
            var serializer = new XmlSerializer(typeof(RelationEditorConfiguration));
            var reader = new StringReader(XmlCaption);
            var config = (RelationEditorConfiguration)serializer.Deserialize(reader);
            return config;
        }
    }
}
