using UnityEngine;
using System.Collections;

public class GravityBody : MonoBehaviour
{
    public Attractor attractor;
    private Transform myTransform;

    void Start()
    {
        myTransform = GetComponent<Transform>();
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
        GetComponent<Rigidbody>().useGravity = false;

    }

    void Update()
    {
        attractor.Attract(myTransform);
    }
}

