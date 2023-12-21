using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetArrow : MonoBehaviour
{
    public Transform anchoredPoint;
    public Vector3 targetedPoint;
    public Transform headMarker, tail, tailMarker, tailObj;

    public void Setup(Transform anchorPoint)
    {
        anchoredPoint = anchorPoint;

        float zDepth = anchoredPoint.position.z - Camera.main.transform.position.z;
        targetedPoint = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, zDepth));

        tailMarker.position = anchoredPoint.position;
        headMarker.position = targetedPoint;
        UpdateTail();
    }

    public void UpdateTargetPos()
    {
        float zDepth = anchoredPoint.position.z - Camera.main.transform.position.z;
        targetedPoint = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, zDepth));
        headMarker.position = targetedPoint;
        UpdateTail();
    }
    
    void UpdateTail()
    {
        tail.localScale = new Vector3(1f, 1f, Vector3.Distance(headMarker.localPosition, tailMarker.localPosition));
        tail.LookAt(headMarker);
        if(tail.localEulerAngles.y > 180)
        {
            tailObj.localEulerAngles = new Vector3(0f, 90f, 180f);
        }
        else
        {
            tailObj.localEulerAngles = new Vector3(0f, 270f, 0f);
        }
    }
}
