using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SendMaterial : MonoBehaviour
{
    [SerializeField] GameObject materialExhivitor;
    MaterialReciver materialReciver;
    

    // Start is called before the first frame update
    void Start()
    {
        materialReciver = FindObjectOfType<MaterialReciver>().GetComponent<MaterialReciver>();
        materialExhivitor = this.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.N))
        {
            SendObject();
        }
    }
    public void SendObject()
    {
        materialReciver.materialContainer = materialExhivitor;
        materialReciver.ChangerMaterial();
        Debug.Log("SendMaterial");
    }
}
