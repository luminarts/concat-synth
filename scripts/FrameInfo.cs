using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

public class FrameInfo
{
  public int FrameId { get; set; }

  [JsonExtensionData]
  public Dictionary<string, JsonElement> AdditionalData { get; set; }

  public Dictionary<string, float[]> Bones
  {
    get
    {
      var auxDict = new Dictionary<string, float[]>();

      if (AdditionalData == null)
      {
        return auxDict;
      }

      foreach (var kv in AdditionalData)
      {
        if (kv.Value.ValueKind != JsonValueKind.Array)
        {
          continue;
        }

        var values = new List<float>();
        foreach (var item in kv.Value.EnumerateArray())
        {
          if (item.ValueKind == JsonValueKind.Number)
          {
            values.Add(item.GetSingle());
          }
        }

        auxDict[kv.Key] = values.ToArray();
      }

      return auxDict;
    }
  }
}