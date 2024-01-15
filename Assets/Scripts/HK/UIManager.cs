using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject canvas;
    private bool isHide = true;
    void Update()
    {

        if(Input.GetKeyDown(KeyCode.Q))
        {
            isHide = !isHide;
            canvas.SetActive(isHide);
        }
    }
}
