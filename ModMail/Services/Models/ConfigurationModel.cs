using System.ComponentModel;
using Newtonsoft.Json;

namespace ModMail.Services.Models
{
    public class ConfigurationModel
    {
        [JsonProperty]
        [DefaultValue("!")]
        public string Prefix { get; private set; }
        
        [JsonProperty]
        public ulong MaintainerRoleId { get; private set; }
        
        [JsonProperty]
        public ulong ModeratorRoleId { get; private set; }
        
        [JsonProperty]
        public ulong StaffRoleId { get; private set; }
        
        [JsonProperty]
        public ulong MainGuildId { get; private set; }
        
        [JsonProperty]
        public ulong ModMailCategoryId { get; private set; }
        
        [JsonProperty]
        [DefaultValue("")]
        public string LogWebhookToken { get; private set; }
        
        [JsonProperty]
        [DefaultValue(0)]
        public ulong LogWebhookId { get; private set; }
        
        [JsonProperty]
        [DefaultValue("")]
        public string LogWebhookUsername { get; private set; }
        
        [JsonProperty]
        [DefaultValue("")]
        public string LogWebhookAvatarUrl { get; private set; }
        
        [JsonProperty]
        [DefaultValue(new ulong[] {})]
        public ulong[] IgnoredCommandChannels { get; private set; }
    }
}