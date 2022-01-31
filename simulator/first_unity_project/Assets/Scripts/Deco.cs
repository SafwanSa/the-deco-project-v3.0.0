using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using static HealthBar;
using UnityEngine.UI;

[System.Serializable]
public class DNA
{
    public float health;
    public float size;
    public float maxSpeed;
    public float perception;
    public float aggressionRate;
    public int gender;
    public bool isInfected = false;
    public float[] gene = new float[2];
    public DNA()
    {
        gender = Random.Range(0, 2);
        aggressionRate = 0f;
        health = 100.0f;
        size = 2f;
        maxSpeed = 120f / size;
        perception = 1f;
        GetGene();
    }

    public DNA Crossover(DNA other)
    {
        DNA dna = new DNA();
        int rand1 = Random.Range(0, 1);
        int rand2 = Random.Range(0, 1);
        dna.perception = rand1 == 1 ? perception : other.perception;
        dna.aggressionRate = rand2 == 1 ? aggressionRate : other.aggressionRate;
        return dna;
    }

    public void Mutation(float mutationRate)
    {
        if (Random.Range(0f, 100f) < mutationRate)
        {
            perception = Random.Range(1f, perception);
            size = Random.Range(2, 5);
            maxSpeed = 120f / size;
        }
    }

    float[] GetGene()
    {
        gene[0] = perception;
        gene[1] = aggressionRate;
        return gene;
    }
}
[System.Serializable]
public class Deco : MonoBehaviour
{
    Environment environment;
    public DNA dna;
    public bool foundIntreset = false;
    public Vector3 targetPoisition;
    public GameObject currentFood;
    public bool isEating;
    public GameObject healthBarGameObject;
    public HealthBar healthBar;
    public GameObject partner;
    public string partnerName;
    public List<GameObject> parents = new List<GameObject>();
    public List<string> parentsNames = new List<string>();
    public int generationTag = 1;

    public bool isFighting = false;
    public bool startFight = false;
    public GameObject otherFighter;

    public string family;
    public float terrainHeight;

    public bool isSleeping = true;
    public Vector3 sleepingPosition;


    void Init()
    {
        if (dna == null)
        {
            dna = new DNA();
        }

        environment = FindObjectOfType<Environment>();
        terrainHeight = Terrain.activeTerrain.SampleHeight(transform.position);
        environment.decos.Add(gameObject);
        if (family == null)
        {
            family = "" + gameObject.name[0];
            sleepingPosition = environment.GetSleepingPosition(family);
        }

        UnityEngine.Object pPrefab = Resources.Load("HealthBar");
        healthBarGameObject = (GameObject)GameObject.Instantiate(pPrefab, Vector3.zero, Quaternion.identity);
        healthBarGameObject.AddComponent(typeof(HealthBar));
        healthBarGameObject.transform.rotation = Quaternion.identity;
        healthBarGameObject.transform.localScale = new Vector3(0.009f, 0.009f, 0.009f);
        healthBarGameObject.transform.position = new Vector3(0f, transform.localScale.y + 1f, 0f);
        healthBarGameObject.transform.parent = transform;
        healthBar = healthBarGameObject.transform.GetChild(0).gameObject.GetComponent<HealthBar>();
        healthBar.SetMaxHealth(dna.health);
        if (dna.isInfected)
        {
            gameObject.transform.Find("eye1").GetComponent<Renderer>().material.SetColor("_Color", Color.red);
            gameObject.transform.Find("eye2").GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        }

        SphereCollider trigger = gameObject.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = dna.perception;

        gameObject.tag = "deco";

        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationZ;
        rb.constraints = RigidbodyConstraints.FreezeRotationX;
        // rb.constraints = RigidbodyConstraints.FreezePositionY;

        transform.localScale = new Vector3(dna.size, dna.size, dna.size);
        transform.position = GetRandomPosition();

        targetPoisition = GetRandomPosition();
    }
    void Start()
    {
        Init();
    }

    Vector3 GetRandomPosition()
    {
        Vector3 pos = environment.GetRandomPosition();
        pos.y = GetYPos();
        return pos;
    }

    public float GetYPos()
    {
        // This helps keeping the Deco always at the top of the terrain
        return terrainHeight + transform.localScale.y;
    }
    void MoveTo(Vector3 targetPosition)
    {
        Vector3 new_position = new Vector3(targetPosition.x, GetYPos(), targetPosition.z);

        if (!isEating && !isFighting)
            transform.LookAt(new_position);
        else
        {
            transform.rotation = Quaternion.identity;
        }

        transform.position = Vector3.MoveTowards(transform.position, new_position, dna.maxSpeed * environment.GetFactor());
    }

    void GetOld()
    {
        // Increase the size as they got old
        transform.localScale = new Vector3(dna.size, dna.size, dna.size);
        // As long as they are not fighting, the loose 1 health each factor
        if (!isFighting)
        {
            float healthLoosed = dna.isInfected ? 2f : 1f;
            dna.health -= healthLoosed * environment.GetFactor();

        }
        // Update their health bars
        healthBar.SetHealth(dna.health);
        // If their healths reaches 0, they die
        if (dna.health <= 0)
        {
            // If they died while they were fighting, increase the health of the other fighter by 20
            if (isFighting)
            {
                Deco deco = otherFighter.GetComponent<Deco>();
                deco.dna.health += 50f;
            }
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Clear the parents from died parents
        parents.RemoveAll(deco => deco == null);
        // Update the activate terrian height, so they always stays at the top
        terrainHeight = Terrain.activeTerrain.SampleHeight(transform.position);
        // Update their sleeping position to match their updated heights
        sleepingPosition = new Vector3(sleepingPosition.x, GetYPos(), sleepingPosition.z);
        // This also prevents some weired behavior of Decos moving under the ground :)
        if (transform.position.y < transform.localScale.y)
            transform.position = new Vector3(transform.position.x, GetYPos(), transform.position.z);

        // Check if it is sleeping time
        if (environment.IsSleepTime())
        {
            Sleep();
        }
        else
        {
            // Awake and behave
            isSleeping = false;
            GetOld();
            Behave();
        }
    }

    void Eat()
    {
        // Each time deco eats portion of food, their health, aggressions rate, and size increase 
        Food foodObj = currentFood.GetComponent<Food>();
        foodObj.Eaten();
        dna.health += 2 * environment.GetFactor();
        dna.size += 0.5f * environment.GetFactor();
        dna.aggressionRate += 1f * environment.GetFactor();
        dna.maxSpeed = 120f / dna.size;
        dna.perception += 0.1f * environment.GetFactor();
        // The perception is simply theier radius of view
        gameObject.GetComponent<SphereCollider>().radius = dna.perception;
        // Check if the all food consumed, to stop eating
        if (foodObj.nutrients < 10 || foodObj.isReloading)
        {
            isEating = false;
            foundIntreset = false;
            foodObj.isBeingEaten = false;
            currentFood = null;
            // Move away
            targetPoisition = GetRandomPosition();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "food" && !isSleeping)
        {
            Food food = collision.gameObject.GetComponent<Food>();
            if (!food.isReloading)
            {
                Rigidbody rb = gameObject.GetComponent<Rigidbody>();
                rb.constraints = RigidbodyConstraints.FreezePosition;
            }
        }
        if (collision.gameObject.tag == "deco" && !isSleeping)
        {
            Deco deco = collision.gameObject.GetComponent<Deco>();
            if (ShouldFight(collision.gameObject, deco))
            {
                DecideToFight(collision.gameObject, deco);
                Fight(collision.gameObject, deco);
            }

        }
    }

    void OnCollisionStay(Collision collision)
    {
        // Collide with food
        if (collision.gameObject.tag == "food" && !isSleeping && foundIntreset)
        {
            Food food = collision.gameObject.GetComponent<Food>();
            if (!food.isReloading)
            {
                foundIntreset = true;
                currentFood = collision.gameObject;
                isEating = true;
            }
            else
            {
                foundIntreset = false;
                currentFood = null;
                isEating = false;
                targetPoisition = GetRandomPosition();
            }
        }
        // Collide with another Deco
        else if (collision.gameObject.tag == "deco" && !isSleeping)
        {
            Deco deco = collision.gameObject.GetComponent<Deco>();
            if (ShouldKeepFight(collision.gameObject, deco))
                Fight(collision.gameObject, deco);

        }
    }
    void OnTriggerEnter(Collider other)
    {
        // Food is detected
        if (other.gameObject.tag == "food" && !foundIntreset && !isFighting && !isSleeping)
        {
            Food food = other.gameObject.GetComponent<Food>();
            if (!food.isReloading)
            {
                foundIntreset = true;
                Vector3 pos = other.gameObject.transform.position;
                targetPoisition = new Vector3(pos.x + Random.Range(-3f, 3f), transform.position.y, pos.z + Random.Range(-3f, 3f));

            }
        }
    }
    void OnTriggerStay(Collider other)
    {
        // Still detecting food
        if (other.gameObject.tag == "food" && !foundIntreset && !isSleeping)
        {
            Food food = other.gameObject.GetComponent<Food>();
            if (!food.isReloading)
            {
                foundIntreset = true;
                Vector3 pos = other.gameObject.transform.position;
                targetPoisition = new Vector3(pos.x + Random.Range(-3f, 3f), transform.position.y, pos.z + Random.Range(-3f, 3f));

            }
        }
        // Still detecting Deco
        else if (other.gameObject.tag == "deco" && !isSleeping)
        {
            Deco deco = other.gameObject.GetComponent<Deco>();
            if (ShouldKeepFight(other.gameObject, deco))
            {
                MoveTowardsEachOther(other.gameObject, deco);
            }
        }
    }

    public bool ShouldFight(GameObject other, Deco deco)
    {
        bool shouldIFight = !foundIntreset && !isFighting && !isEating && Random.Range(0f, 100f) < dna.aggressionRate;
        bool shouldOtherFight = !deco.isFighting && !deco.foundIntreset && !deco.isEating;
        return shouldIFight && shouldOtherFight && deco.family != family;
    }

    public bool ShouldKeepFight(GameObject other, Deco deco)
    {
        bool shouldIKeepFight = foundIntreset && isFighting && otherFighter == other && startFight;
        bool shouldOtherKeepFight = deco.isFighting && deco.foundIntreset && deco.otherFighter == gameObject;
        return shouldIKeepFight && shouldOtherKeepFight;
    }

    public void MoveTowardsEachOther(GameObject other, Deco deco)
    {
        Vector3 pos = other.transform.position;
        targetPoisition = new Vector3(pos.x, GetYPos(), pos.z);
        Vector3 pos2 = gameObject.transform.position;
        deco.targetPoisition = new Vector3(pos2.x, deco.GetYPos(), pos2.z);
    }

    public void Sleep()
    {
        targetPoisition = sleepingPosition;
        isFighting = false;
        startFight = false;
        isEating = false;
        foundIntreset = false;
        currentFood = null;
        otherFighter = null;
        isSleeping = true;
        MoveTo(targetPoisition);
    }

    public void Behave()
    {
        if (!foundIntreset)
        {
            float dist = Vector3.Distance(transform.position, new Vector3(targetPoisition.x, GetYPos(), targetPoisition.z));
            if (dist <= 1)
            {
                targetPoisition = GetRandomPosition();
            }
            MoveTo(targetPoisition);
        }
        else
        {
            if (isEating)
            {
                Eat();
                MoveTo(targetPoisition);
            }
            else
            {
                if (otherFighter == null && isFighting && !currentFood)
                {
                    isFighting = false;
                    startFight = false;
                    targetPoisition = GetRandomPosition();
                    foundIntreset = false;
                }
                MoveTo(targetPoisition);
            }
        }
    }

    public void Fight(GameObject other, Deco deco)
    {
        // MoveTowardsEachOther(other, deco);
        // targetPoisition = transform.position;
        // deco.targetPoisition = other.transform.position;
        dna.health -= (deco.dna.aggressionRate / 10f) * environment.GetFactor();
        deco.dna.health -= (dna.aggressionRate / 10f) * environment.GetFactor();
    }

    public void DecideToFight(GameObject other, Deco deco)
    {
        startFight = true;
        foundIntreset = true;
        isFighting = true;
        otherFighter = other;
        Vector3 pos = other.transform.position;
        targetPoisition = new Vector3(pos.x, GetYPos(), pos.z);

        deco.foundIntreset = true;
        deco.isFighting = true;
        deco.otherFighter = gameObject;
        Vector3 pos2 = gameObject.transform.position;
        deco.targetPoisition = new Vector3(pos2.x, deco.GetYPos(), pos2.z);
    }
}
