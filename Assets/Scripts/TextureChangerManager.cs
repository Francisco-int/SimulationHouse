using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TextureChangerManager : MonoBehaviour
{
    // MaterialReciver materialReciver;
   [SerializeField]  Button button;
    [SerializeField] Button button1;
    [SerializeField] Button button2;
    [SerializeField] Button button3;

    // Start is called before the first frame update
    void Start()
    {
        button = GameObject.Find("Button (Object)").GetComponent<Button>();
        button1 = GameObject.Find("Button (Red)").GetComponent<Button>();
        button2 = GameObject.Find("Button (Black)").GetComponent<Button>();
        button3 = GameObject.Find("Button (Green)").GetComponent<Button>();

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            button1.onClick.Invoke();
            Debug.Log("Red");
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            button2.onClick.Invoke();
            Debug.Log("black");
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            button3.onClick.Invoke(); 
            Debug.Log("Green");
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            button.onClick.Invoke();
            Debug.Log("Object");
        }
    }
    void SimularClick()
    {
        gameObject.SendMessage("OnClick");
    }

    public void SendMaterial(GameObject partHouse, GameObject materialContainer)
    {
        Renderer rendhouse = partHouse.GetComponent<Renderer>();
        Renderer rendContainer = materialContainer.GetComponent<Renderer>();

        rendhouse.material = rendContainer.material;

        Debug.Log("SendMaterial");
    }
}
