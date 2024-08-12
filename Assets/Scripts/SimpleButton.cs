using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleButton : MonoBehaviour
{
    public delegate void ButtonPressed();
    public event ButtonPressed OnButtonPressed;
    public bool canBeHeld;
    bool held, pressed;


    private void OnMouseDown()
    {
        OnButtonPressed();
        held = true;
        pressed = true;
    }

    private void Update()
    {
        if(pressed) { pressed = false; }
        else if(held && canBeHeld) { OnButtonPressed(); }
    }

    private void OnMouseUp()
    {
        held = false;
    }
}
