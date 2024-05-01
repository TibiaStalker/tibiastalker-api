using Newtonsoft.Json;

namespace TibiaStalker.Application.TibiaData.Dtos.v4;

public class TibiaDataV4WorldResponse
{
    [JsonProperty("world")]
    public WorldResponseV4 World { get; set; }

    [JsonProperty("information")]
    public InformationResponseV4 Information { get; set; }
}








