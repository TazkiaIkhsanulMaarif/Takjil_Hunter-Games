using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Groundcheck : MonoBehaviour
{
    PlayerLogic logicmovement;
    private void Start()
    {
        logicmovement = this.GetComponentInParent<PlayerLogic>();
    }
    // Start is called before the first frame update
    public void OnTriggerEnter(Collider other)
    {
        logicmovement.groundedchanger();
        Debug.Log("Touch The Ground");
    }
}