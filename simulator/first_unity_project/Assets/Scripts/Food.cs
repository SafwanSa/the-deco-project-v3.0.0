using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    Environment environment;
    public float nutrients;
    public float maxNutrients = 25f;

    public bool isBeingEaten = false;
    public bool isReloading = false;
    void Start()
    {
        nutrients = maxNutrients;
        environment = FindObjectOfType<Environment>();
        SphereCollider trigger = gameObject.AddComponent<SphereCollider>();
        transform.position = GetRandomPosition();
        transform.localScale = new Vector3((nutrients / 10f) + 3f, (nutrients / 10f) + 3f, (nutrients / 10f) + 3f);
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        gameObject.tag = "food";
    }

    // Update is called once per frame
    void Update()
    {
        if (!isBeingEaten && nutrients < maxNutrients)
        {
            isReloading = true;
            nutrients += 3 * environment.GetFactor();
            transform.localScale = new Vector3((nutrients / 10f) + 3f, (nutrients / 10f) + 3f, (nutrients / 10f) + 3f);
        }
        else
        {
            isReloading = false;
        }
    }

    Vector3 GetRandomPosition()
    {
        Vector3 pos = environment.GetRandomPosition();
        return new Vector3(pos.x, 0f, pos.z);
    }

    public void Eaten()
    {
        nutrients -= 3 * environment.GetFactor();
        transform.localScale = new Vector3((nutrients / 10f) + 3f, (nutrients / 10f) + 3f, (nutrients / 10f) + 3f);
        isBeingEaten = true;
    }
}
