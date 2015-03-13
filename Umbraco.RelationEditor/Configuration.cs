using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using Newtonsoft.Json;
using umbraco.controls;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;

namespace Umbraco.RelationEditor
{
    public class EntityConfiguration
    {
        [XmlAttribute]
        [JsonProperty(Order = 2)]
        public string Alias { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        public virtual bool Enabled
        {
            get { return Alias != null; }
        }
    }

    [XmlRoot("RelationEditor")]
    public class RelationEditorConfiguration
    {
        [XmlIgnore]
        [JsonIgnore]
        public BreadCrumbMode BreadCrumbMode;

        [XmlAttribute("BreadCrumbMode")]
        [JsonProperty("BreadCrumbMode", NullValueHandling = NullValueHandling.Ignore)]
        public string StrBreadCrumbMode
        {
            get { return BreadCrumbMode.ToString(); }
            set { BreadCrumbMode = Enum.IsDefined(typeof(BreadCrumbMode), value) ? (BreadCrumbMode)Enum.Parse(typeof(BreadCrumbMode), value) : BreadCrumbMode.ToolTip; }
        }

        [XmlAttribute]
        [JsonProperty("BreadCrumbSeparator", NullValueHandling = NullValueHandling.Ignore)]
        public string BreadCrumbSeparator { get; set; }
        
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
        private const StringComparison IgnoreCase = StringComparison.InvariantCultureIgnoreCase;

        [XmlAttribute]
        [JsonProperty(Order = 1)]
        public UmbracoObjectTypes Name { get; set; }
        
        [XmlElement("EnabledRelation")]
        [JsonProperty(Order = 5)]
        public List<EnabledRelationConfiguration> EnabledRelations { get; set; }

        public EnabledRelationConfiguration Get(string alias)
        {
            return EnabledRelations
                .FirstOrDefault(r => r.Alias.Equals(alias, IgnoreCase))
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
        [JsonProperty(Order = 3)]
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
                return Instance.ObjectTypes;
            }
        }

        public static RelationEditorConfiguration Instance
        {
            get
            {
                EnsureConfiguration();
                return configuration.ObjectTypes;
            }
        }

        public static ObjectTypeConfiguration Get(UmbracoObjectTypes objectType, string alias)
        {
            lock (LockObj)
            {
            return ObjectTypes
                .FirstOrDefault(t => t.Name == objectType && t.Alias == alias)
                ?? new ObjectTypeConfiguration();
        }
        }

        public static void Set(UmbracoObjectTypes objectType, string alias, ObjectTypeConfiguration typeConfiguration)
        {
            lock (LockObj)
            {
                var existing = ObjectTypes.FirstOrDefault(t => t.Name == objectType && t.Alias == alias);
                if (existing != null)
                    configuration.ObjectTypes.Remove(existing);
                if (typeConfiguration.EnabledRelations.Any())
                { 
                    typeConfiguration.Alias = alias;
                    typeConfiguration.Name = objectType;
                    configuration.ObjectTypes.Add(typeConfiguration);
                }
                Save();
            }
        }

        private static void Save()
        {
            try
            {
                var serializer = new XmlSerializer(typeof(RelationEditorConfiguration));
                using (var writer = new StreamWriter(HttpContext.Current.Server.MapPath("~/config/RelationEditor.config")))
                {
                    serializer.Serialize(writer, configuration);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<Configuration>("Could not write config/RelationEditor.config", ex);
            }
        }

        private static void EnsureConfiguration()
        {
            lock (LockObj)
            {
                if (configuration == null)
            {
                try
                {
                        var serializer = new XmlSerializer(typeof(RelationEditorConfiguration));
                    using (var reader = new StreamReader(HttpContext.Current.Server.MapPath("~/config/RelationEditor.config")))
                    { 
                            configuration = (RelationEditorConfiguration)serializer.Deserialize(reader);
                    }
                }
                    catch (Exception ex)
                {
                    LogHelper.Error<Configuration>("Could not read config/RelationEditor.config", ex);
                    configuration = new RelationEditorConfiguration();
                }
            }
        }

    public enum BreadCrumbMode
    {
        ToolTip,
        Caption
    }
}
}
