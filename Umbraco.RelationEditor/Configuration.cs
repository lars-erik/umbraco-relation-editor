using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;

namespace Umbraco.RelationEditor
{
    public class EntityConfiguration
    {
        [XmlAttribute]
        public string Alias { get; set; }

        [XmlIgnore]
        public virtual bool Enabled
        {
            get { return Alias != null; }
        }
    }

    [XmlRoot("RelationEditor")]
    public class RelationEditorConfiguration
    {
        [XmlElement("ObjectType")]
        public List<ObjectTypeConfiguration> ObjectTypes { get; set; }

        public ObjectTypeConfiguration Get(UmbracoObjectTypes objectType, string alias)
        {
            return ObjectTypes
                .FirstOrDefault(t => t.Name == objectType && t.Alias == alias) 
                ?? new ObjectTypeConfiguration();
        }

        public RelationEditorConfiguration()
        {
            ObjectTypes = new List<ObjectTypeConfiguration>();
        }
    }

    public class ObjectTypeConfiguration : EntityConfiguration
    {
        [XmlAttribute]
        public UmbracoObjectTypes Name { get; set; }
        [XmlAttribute]
        public bool AllowInheritance { get; set; }
        [XmlElement("EnabledRelation")]
        public List<EnabledRelationConfiguration> EnabledRelations { get; set; }

        public EnabledRelationConfiguration Get(string alias)
        {
            return EnabledRelations
                .FirstOrDefault(r => r.Alias == alias)
                ?? new EnabledRelationConfiguration();
        }

        public ObjectTypeConfiguration()
        {
            EnabledRelations = new List<EnabledRelationConfiguration>();
        }

        public override bool Enabled
        {
            get { return Name != UmbracoObjectTypes.Unknown; }
        }
    }

    public class EnabledRelationConfiguration : EntityConfiguration
    {
        [XmlElement("EnabledChildType")]
        public List<EnabledChildTypeConfiguration> EnabledChildTypes { get; set; }

        public EnabledChildTypeConfiguration Get(string alias)
        {
            return EnabledChildTypes
                .FirstOrDefault(c => c.Alias == alias)
                ?? new EnabledChildTypeConfiguration();
        }

        public EnabledRelationConfiguration()
        {
            EnabledChildTypes = new List<EnabledChildTypeConfiguration>();
        }
    }

    public class EnabledChildTypeConfiguration : EntityConfiguration
    {
    }

    public class Configuration
    {
        private static RelationEditorConfiguration configuration = null;

        private static IEnumerable<ObjectTypeConfiguration> ObjectTypes
        {
            get
            {
                EnsureConfiguration();
                return configuration.ObjectTypes;
            }
        }

        public static ObjectTypeConfiguration Get(UmbracoObjectTypes objectType, string alias)
        {
            return ObjectTypes
                .FirstOrDefault(t => t.Name == objectType && t.Alias == alias)
                ?? new ObjectTypeConfiguration();
        }

        private static void EnsureConfiguration()
        {
            if (configuration == null)
            {
                try
                {
                    var serializer = new XmlSerializer(typeof (RelationEditorConfiguration));
                    using (var reader = new StreamReader(HttpContext.Current.Server.MapPath("~/config/RelationEditor.config")))
                    { 
                        configuration = (RelationEditorConfiguration) serializer.Deserialize(reader);
                    }
                }
                catch(Exception ex)
                {
                    LogHelper.Error<Configuration>("Could not read config/RelationEditor.config", ex);
                    configuration = new RelationEditorConfiguration();
                }
            }
        }
    }
}
