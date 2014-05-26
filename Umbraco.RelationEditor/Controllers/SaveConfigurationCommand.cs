using Newtonsoft.Json;

namespace Umbraco.RelationEditor.Controllers
{
    public class SaveConfigurationCommand
    {
        public SaveConfigurationCommand()
        {
        }
        
        public SaveConfigurationCommand(int id, string type, ObjectTypeConfiguration configuration)
        {
            this.Id = id;
            this.Type = type;
            this.Configuration = configuration;
        }

        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("configuration")]
        public ObjectTypeConfiguration Configuration { get; set; }
    }
}