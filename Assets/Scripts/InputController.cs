using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    public static InputController main;

    public TargetArrow targetArrow;

    public GameObject objectAwaitingTarget;
    bool targetMode;


    private void Awake()
    {
        main = this;
    }

    public bool Busy()
    {
        return targetMode;
    }

    public bool TargetMode()
    {
        return targetMode;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (targetMode)
            {
                ExitTargetMode();
            }
        }

        if (targetMode)
        {
            targetArrow.UpdateTargetPos();
        }
    }

    public void EnterTargetMode(GameObject triggeredObject)
    {
        objectAwaitingTarget = triggeredObject;
        targetArrow.gameObject.SetActive(true);
        targetArrow.Setup(triggeredObject.transform);
        targetMode = true;
    }

    public void SubmitTarget(GameObject target)
    {
        Debug.Log("Testing target: " + target.name);

        if(objectAwaitingTarget == null)
        {
            Debug.Log("LOST OBJECT WAITING FOR TARGET!");
            return;
        }

        objectAwaitingTarget.SendMessage("TargetFound", target);
    }

    public void ExitTargetMode()
    {
        objectAwaitingTarget = null;
        targetArrow.gameObject.SetActive(false);
        targetMode = false;
    }
}
