using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Activation : MonoBehaviour
{
    public void ActivateThisObject()
    {
        if (UIHelperObjectMover.Instance != null)
        {
            UIHelperObjectMover.Instance.ToggleMovement(this.gameObject);
        }
        else
        {
            Debug.LogError("UIHelperObjectMover.Instance no está inicializado.");
        }
    }
}
