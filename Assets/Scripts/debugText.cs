using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class debugText : MonoBehaviour
{
    [SerializeField]private TMP_Text[] distance;
    [SerializeField]private GameObject[] leg;
    void Update()
    {
        for (int i = 0; i< distance.Length; i++){
            distance[i].text = "distance"+i+": "+leg[i].GetComponent<controlJoints>().FootDistance.ToString();
        }
    }
}
