using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PortonOpenClose : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            transform.Translate(Vector3.down * Time.deltaTime * 1);   
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.Translate(Vector3.up * Time.deltaTime * 1);       
        }
           if (transform.position.z > -4.876)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, -4.876f);
        }
        
        if (transform.position.z < -7.5229)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, -7.5229f);
        }
    }
    public void OpenPorton()
    {
        transform.Translate(Vector3.up * Time.deltaTime * 1);
    }
    public void ClosePorton()
    {
        transform.Translate(Vector3.down * Time.deltaTime * 1);
    }
}
