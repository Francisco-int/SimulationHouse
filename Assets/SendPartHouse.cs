using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SendPartHouse : MonoBehaviour
{
    public GameObject partHouseGameObject;
    [SerializeField] MaterialReciver materialReciver;

    // Start is called before the first frame update
    void Start()
    {
        partHouseGameObject = this.gameObject;
        materialReciver = FindObjectOfType<MaterialReciver>().GetComponent<MaterialReciver>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.L))
        {
            sendObject();
        }
    }
    public void sendObject()
    {
        Debug.Log("SendObjecto");
        materialReciver.partHouse = partHouseGameObject;
    }
}

