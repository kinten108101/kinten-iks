using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameManager: Singleton<gameManager> {
    public LayerMask gndLayer;
}
public class Game : MonoBehaviour
{
    [Tooltip("Which layer is the ground?")][SerializeField]private LayerMask groundLayerMask;
    void Start()
    {
        gameManager.Instance.gndLayer = groundLayerMask;
        Debug.Log(groundLayerMask.value.ToString());
        Debug.Log(gameManager.Instance.gndLayer.value.ToString());
    }


}
