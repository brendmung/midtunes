using DeepSoundClient.Classes.Global;
using Newtonsoft.Json;

namespace DeepSound.Library.OneSignalNotif.Models
{
    public class OsObject
    {
        public class OsNotificationObject
        { 
            [JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
            public string UserId { get; set; }

            [JsonProperty("track", NullValueHandling = NullValueHandling.Ignore)]
            public string TrackId { get; set; }
             
            [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
            public string Type { get; set; }
             
            [JsonProperty("user_data", NullValueHandling = NullValueHandling.Ignore)]
            public UserDataObject UserData { get; set; }
              
            [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
            public string Url { get; set; } 
              
        } 
    }
}