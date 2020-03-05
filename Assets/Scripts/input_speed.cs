using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class input_speed : MonoBehaviour
{    
    public GameObject inputfield;    

    public GameObject target;
    

    public void changeSpeed()
    {      
        string speed = inputfield.GetComponent<Text>().text;
        speed = speed.Replace(".", ",");
        float newSpeed;       

        if(float.TryParse(speed, out newSpeed))
        {
            target.GetComponent<createST>().Speed(newSpeed);
        }
    }
}
