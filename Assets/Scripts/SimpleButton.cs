using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleButton : MonoBehaviour
{
    public delegate void ButtonPressed();
    public event ButtonPressed OnButtonPressed;

    private void OnMouseDown()
    {
        OnButtonPressed();
    }
}
