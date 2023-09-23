using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuControlUI : MonoBehaviour
{
    [SerializeField]
    private MenuControl menuControl;

    public void Back()
    {
        menuControl.Back();
    }
}
