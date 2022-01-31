using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Deco;
using System.Linq;
using UnityEngine.UI;
public class Environment : MonoBehaviour
{
    [Range(1000f, 10000f)]
    public float range;
    public int generation = 1;
    public float mutationRate = 0.2f;
    public List<GameObject> decos = new List<GameObject>();
    public List<GameObject> foods = new List<GameObject>();
    private float nextActionTime = 10.0f;
    private float nextLogTime = 5f;
    public List<string> names = new List<string>();

    [Range(1f, 10f)]
    public float factor = 1f;

    public LightingManager lightManager;

    string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    Dictionary<string, Vector3> sleepingPoisitions = new Dictionary<string, Vector3>();
    public Text generationText;
    public Text decosNumText;

    public Text timeOfTheDayText;
    public GameObject canvas;
    public float infectionRate = 0.2f;
    public List<string> infectedFamilies = new List<string>();
    public List<Text> familiesText = new List<Text>();
    public Color[] CreateColors()
    {
        Color[] colors = new Color[26];
        string[] hex = {
        "#85cf36",
        "#b13196",
        "#356e85",
        "#fc9f7b",
        "#dff8b4",
        "#24741c",
        "#04c077",
        "#bf702e",
        "#215be8",
        "#aa7624",
        "#1a2671",
        "#05d406",
        "#0611af",
        "#7f04b0",
        "#1d481b",
        "#857fa0",
        "#fcfd4e",
        "#02f5f0",
        "#5c391e",
        "#a85a34",
        "#af7794",
        "#8bd868",
        "#5362e2",
        "#5c2175",
        "#b9c288",
        "#08f7bc"
        };
        for (int i = 0; i < colors.Length; i++)
        {
            Color color;
            ColorUtility.TryParseHtmlString(hex[i], out color);
            colors[i] = color;
        }
        return colors;
    }

    public void SetSleepingPositions()
    {
        for (int i = 0; i < chars.Length; i++)
        {
            sleepingPoisitions.Add($"{chars[i]}", GetRandomPosition());
        }
    }
    public void createDecos(int number)
    {

        Color[] colors = CreateColors();
        for (int i = 0; i < number; i++)
        {
            // GameObject deco = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            UnityEngine.Object pPrefab = Resources.Load("Deco");
            GameObject deco = (GameObject)GameObject.Instantiate(pPrefab, Vector3.zero, Quaternion.identity);
            deco.GetComponent<Renderer>().material.SetColor("_Color", colors[i]);
            deco.name = "" + chars[i];
            deco.AddComponent(typeof(Deco));
            // decos.Add(deco);
        }
    }

    public void createFoods(int number)
    {
        for (int i = 0; i < number; i++)
        {
            UnityEngine.Object pPrefab = Resources.Load("Darth_Artisan/Free_Trees/Prefabs/Oak_Tree");
            GameObject food = (GameObject)GameObject.Instantiate(pPrefab, Vector3.zero, Quaternion.identity);
            food.AddComponent(typeof(Food));
            foods.Add(food);
        }

    }

    void init()
    {
        SetSleepingPositions();
        createFoods(26);
        createDecos(26);
        BuildUI();
    }

    void Start()
    {
        init();
    }

    void BuildUI()
    {
        lightManager = FindObjectOfType<LightingManager>();
        generationText = GameObject.Find("Generation").GetComponent<Text>();
        decosNumText = GameObject.Find("NumOfDecos").GetComponent<Text>();
        timeOfTheDayText = GameObject.Find("timeOfTheDay").GetComponent<Text>();
        canvas = GameObject.Find("Canvas").gameObject;
        Color[] colors = CreateColors();
        for (int i = 0; i < 26; i++)
        {

            GameObject go = new GameObject($"{chars[i]}_text");
            go.transform.position = new Vector3(69f, -93f - (i * 28f), 0f);
            go.transform.SetParent(canvas.transform, false);
            Text text = go.AddComponent<Text>();
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0.5f, 0.5f);
            text.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            text.color = colors[i];
            text.fontSize = 16;
            familiesText.Add(text);
        }
    }
    void UpdateUI()
    {
        generationText.text = $"Generation: {generation}";
        decosNumText.text = $"Decos: {decos.Count}";
        int hours = (int)lightManager.TimeOfDay;
        int minutes = (int)((lightManager.TimeOfDay - hours) * 60f);

        timeOfTheDayText.text = $"Time: {(hours >= 10 ? "" : "0")}{hours}:{(minutes >= 10 ? "" : "0")}{minutes}";
        for (int i = 0; i < 26; i++)
        {
            string f = "" + chars[i];
            int count = decos.Where(d => d.GetComponent<Deco>().family == f).ToList().Count;
            familiesText[i].text = $"{chars[i]}: {count}";
        }
    }

    void Update()
    {
        decos.RemoveAll(deco => deco == null);
        UpdateUI();
        if (Time.time > nextActionTime)
        {
            nextActionTime += 25f / factor;
            if (!IsSleepTime())
            {
                if (generation > 3)
                {
                    infectedFamilies.Clear();
                    GenerateInfection();
                }
                StartNewGeneration();
                Debug.Log($"New Generation: {generation}");

            }
        }

        if (Time.time > nextLogTime)
        {
            if (!IsSleepTime())
            {
                float logtime = nextLogTime;
                nextLogTime += 4f / factor;
                HService ht = new HService();
                ht.Post(decos, generation, logtime);

            }
        }

    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(Vector3.zero, new Vector3(range, range, range));
    }
    public Vector3 GetRandomPosition()
    {
        return new Vector3(Random.Range(-range / 2, range / 2), 5f, Random.Range(-range / 2, range / 2));
    }

    public float GetFactor()
    {
        return Time.deltaTime * factor;
    }

    public void StartNewGeneration()
    {
        // Calculate the fitness
        List<GameObject> matingPool = CreateMatingPool();
        for (int i = 0; i < 26; i++)
        {
            // Natural selection
            List<GameObject> parents = NaturalSelection(matingPool);
            // Crossover
            DNA dna = Crossover(parents);
            // Possible infection
            if (infectedFamilies.Contains(parents[0].GetComponent<Deco>().family))
            {
                dna.isInfected = true;
            }
            // Mutation
            Mutation(dna);
            // Spawn the new child
            // GameObject newObjdeco = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            UnityEngine.Object pPrefab = Resources.Load("Deco");
            GameObject newObjdeco = (GameObject)GameObject.Instantiate(pPrefab, Vector3.zero, Quaternion.identity);
            // GameObject father = parents[0].GetComponent<Deco>().dna.health > parents[1].GetComponent<Deco>().dna.health ? parents[0] : parents[1];
            GameObject father = parents[0].GetComponent<Deco>().dna.gender >= parents[1].GetComponent<Deco>().dna.gender ? parents[0] : parents[1];
            Color fatherColor = father.GetComponent<Renderer>().material.color;
            newObjdeco.GetComponent<Renderer>().material.SetColor("_Color", fatherColor);
            Deco newDeco = newObjdeco.AddComponent<Deco>();
            newDeco.dna = dna;
            newDeco.generationTag = generation + 1;
            newObjdeco.name = GetName(father);
            names.Add(newObjdeco.name);
            newDeco.parentsNames.Add(parents[0].name);
            newDeco.parentsNames.Add(parents[1].name);
            newDeco.parents.Add(parents[1]);
            newDeco.parents.Add(parents[0]);
        }
        generation++;
    }

    public int GetNumOfFamilyMembers(string family)
    {
        return decos.Where(d =>
        {
            Deco deco = d.GetComponent<Deco>();
            return deco.family == family;
        }).ToList().Count;
    }

    public List<GameObject> CreateMatingPool()
    {
        List<GameObject> matingPool = new List<GameObject>();
        float maxHealth = 0;
        float maxPerception = 0;
        for (int i = 0; i < decos.Count; i++)
        {
            if (decos[i] != null)
            {
                Deco deco = decos[i].GetComponent<Deco>();
                bool shouldBeAdded = Mathf.Abs(deco.generationTag - generation) <= 2;
                if (shouldBeAdded)
                {
                    if (deco.dna.health > maxHealth)
                        maxHealth = deco.dna.health;
                    if (deco.dna.perception > maxPerception)
                        maxPerception = deco.dna.perception;
                }

            }
        }
        for (int i = 0; i < decos.Count; i++)
        {
            if (decos[i] != null)
            {
                Deco deco = decos[i].GetComponent<Deco>();
                bool shouldBeAdded = Mathf.Abs(deco.generationTag - generation) <= 2;
                if (shouldBeAdded)
                {
                    int perceptionProp = (int)(((deco.dna.perception / maxPerception) * 100) * 0.7f);
                    int healthProp = (int)(((deco.dna.health / maxHealth) * 100) * 0.3f);

                    for (int j = 0; j < (int)(perceptionProp + healthProp); j++)
                    {
                        matingPool.Add(decos[i]);
                    }
                }
            }
        }
        return matingPool;
    }

    public List<GameObject> NaturalSelection(List<GameObject> matingPool)
    {
        List<GameObject> parents = new List<GameObject>();
        int rand1 = Random.Range(0, matingPool.Count);
        GameObject parent1 = matingPool[rand1];
        GameObject parent2 = parent1;
        Deco p1 = parent1.GetComponent<Deco>();
        Deco p2 = parent2.GetComponent<Deco>();
        // while (parent1 == parent2 || p1.parentsNames.Contains(p2.name) || p2.parentsNames.Contains(p1.name) || p1.dna.gender == p2.dna.gender)
        // {
        //     int rand2 = Random.Range(0, matingPool.Count);
        //     parent2 = matingPool[rand2];
        //     p2 = parent2.GetComponent<Deco>();
        // }

        parents.Add(parent1);
        parents.Add(parent2);
        return parents;
    }

    public DNA Crossover(List<GameObject> parents)
    {
        Deco parent1 = parents[0].GetComponent<Deco>();
        Deco parent2 = parents[1].GetComponent<Deco>();
        DNA dna = parent1.dna.Crossover(parent2.dna);
        return dna;

    }

    public void Mutation(DNA dna)
    {
        dna.Mutation(mutationRate);
    }
    public string GetName(GameObject father)
    {
        string name = null;
        while (names.Contains(name) || name == null)
        {
            int rand = Random.Range(0, chars.Length);
            name = $"{father.name}{chars[rand]}";
        }
        return name;
    }

    public Vector3 GetSleepingPosition(string family)
    {
        Vector3 slp = sleepingPoisitions[family];
        Vector3 new_pos = GetRandomPosition();
        int numOfDecos = decos.Where(d => d.GetComponent<Deco>().family == family).ToList().Count;
        while (Vector3.Distance(slp, new_pos) > (5 * numOfDecos))
        {
            new_pos = GetRandomPosition();
        }
        return new_pos;
    }

    public bool IsSleepTime()
    {
        float tfd = lightManager.TimeOfDay;
        return (((tfd > 0 && tfd <= 4) || (tfd > 20 && tfd <= 24)));
    }

    public void GenerateInfection()
    {
        var groupFamily = decos.GroupBy(d => d.GetComponent<Deco>().family);
        int max = 0;
        foreach (var family in groupFamily)
        {
            if (family.Count() > max)
                max = family.Count();
            // Console.WriteLine("{0} {1}", grp.Key, grp.Count());
        }
        foreach (var family in groupFamily)
        {
            float infectionProp = family.Count() / max;
            if (Random.Range(0f, 1f) < infectionProp)
            {
                infectedFamilies.Add(family.Key);
            }
        }
    }
}