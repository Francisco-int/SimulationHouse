using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPhysic : MonoBehaviour
{

    public Transform target;
    Rigidbody rb;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        rb.MovePosition(target.transform.position);
    }
}
