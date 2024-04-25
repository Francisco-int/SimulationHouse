using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Handler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

  
    private void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.CompareTag("hand"))
        {
            transform.position = collision.transform.position;
        }
    }

}
