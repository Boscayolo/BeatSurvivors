using UnityEngine;
using System.Collections;

public class Attractor : MonoBehaviour
{
    public static Attractor instance;
    public float gravity = -10;

    private void Awake()
    {
        instance = this;
    }

    public void Attract(Transform body)
    {
        Vector3 gravityUp = (body.position - transform.position).normalized;
        Vector3 bodyUp = body.up;

        body.GetComponent<Rigidbody>().AddForce(gravityUp * gravity);

        Quaternion targetRotation = Quaternion.FromToRotation(bodyUp, gravityUp) * body.rotation;
        body.rotation = Quaternion.Slerp(body.rotation, targetRotation, 50 * Time.deltaTime);
    }
}
