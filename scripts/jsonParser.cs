using System;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Numerics;
using System.Text.Json;

public class jsonParser
{  


    // Pega dados de quaternion para um frame específico 
    public JointData getQuaternion(string jsonFile, string boneName)
    {
        JointData boneData = new JointData();

        string json = File.ReadAllText(jsonFile);
        using JsonDocument doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        int frameCount = root.GetProperty("frame_count").GetInt32();
        JsonElement.ArrayEnumerator frames = root.GetProperty("frame_data").EnumerateArray();

        // ele vai pegar todos de uma vez, o que talvez dê uma engasgada na hora
        foreach (var frame in frames)
        {
            var q = frame.GetProperty(boneName);
            Console.WriteLine(q);

            // boneData.quaternion = new Quaternion(q); CONSERTAR ESSA BOMBA AQUI
            

        }


        return boneData;
    }
}