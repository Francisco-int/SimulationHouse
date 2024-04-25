using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MaterialReciver : MonoBehaviour
{
    public GameObject partHouse;
    public GameObject materialContainer;
    public TextureChangerManager textureChangerManager;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void ChangerMaterial()
    {      
        textureChangerManager.SendMaterial(partHouse, materialContainer);
        Debug.Log("Changermaterial");

    }
}
