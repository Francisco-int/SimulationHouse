using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandlerPorton : MonoBehaviour
{

    public PortonOpenClose portonOpenClose;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerStay(Collider other)
    {
        portonOpenClose.ClosePorton();
    }
}
