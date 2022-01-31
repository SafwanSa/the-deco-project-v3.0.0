using UnityEngine.Networking;
using System.Text;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class ColorTypeConverter
{
    public static string ToRGBHex(Color c)
    {
        return string.Format("#{0:X2}{1:X2}{2:X2}", ToByte(c.r), ToByte(c.g), ToByte(c.b));
    }

    private static byte ToByte(float f)
    {
        f = Mathf.Clamp01(f);
        return (byte)(f * 255);
    }
}

[System.Serializable]
class IDNA
{
    public float health;
    public float size;
    public float maxSpeed;
    public float perception;
    public int gender;

    public int reportedAtGeneration;
    public float createdAt;

    public IDNA(DNA dna, int generation, float createdAt)
    {
        this.health = dna.health;
        this.size = dna.size;
        this.gender = dna.gender;
        this.perception = dna.perception;
        this.maxSpeed = dna.maxSpeed;
        this.reportedAtGeneration = generation;
        this.createdAt = createdAt;
    }
}
[System.Serializable]
class IDeco
{
    public string name;
    public IDNA dna;
    public string partnerName;
    public List<string> parentsNames = new List<string>();
    public int generationTag;
    public string color;
    public string family;
    public IDeco(string name, IDNA dna, string partnerName, List<string> parentsNames, int generationTag, string color)
    {
        this.name = name;
        this.family = "" + name[0];
        this.color = color;
        this.dna = dna;
        this.parentsNames = parentsNames;
        this.partnerName = partnerName;
        this.generationTag = generationTag;
    }
}


[System.Serializable]
public class JsonListWrapper<T>
{
    public List<T> list;
    public JsonListWrapper(List<T> list) => this.list = list;
}


class HService
{


    public List<IDeco> constrcutDecos(List<GameObject> decos, int generation, float createdAt)
    {
        List<IDeco> idecos = new List<IDeco>();
        for (int i = 0; i < decos.Count; i++)
        {
            Deco dd = decos[i].GetComponent<Deco>();
            IDNA dna = new IDNA(dd.dna, generation, createdAt);
            string partnerName = null;
            if (dd.partner != null)
            {
                partnerName = dd.partner.name;
            }

            IDeco deco = new IDeco(decos[i].name, dna, partnerName, dd.parentsNames,
            dd.generationTag, ColorTypeConverter.ToRGBHex(decos[i].GetComponent<Renderer>().material.color));
            idecos.Add(deco);
        }
        return idecos;
    }

    public void Post(List<GameObject> decos, int generation, float createdAt)
    {
        List<IDeco> idecos = constrcutDecos(decos, generation, createdAt);
        string json = JsonUtility.ToJson(new JsonListWrapper<IDeco>(idecos));
        UnityWebRequest webRequest = new UnityWebRequest("http://localhost:8000/decos/", "POST");
        byte[] encodedPayload = new System.Text.UTF8Encoding().GetBytes(json);
        webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(encodedPayload);
        webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("cache-control", "no-cache");

        UnityWebRequestAsyncOperation requestHandel = webRequest.SendWebRequest();
        requestHandel.completed += delegate (AsyncOperation pOperation)
        {
            // Debug.Log(webRequest.responseCode);
            // Debug.Log(webRequest.downloadHandler.text);
        };

    }

}