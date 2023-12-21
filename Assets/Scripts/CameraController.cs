using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController main;

    public Camera mainCamera;
    public Camera activePlayerHandCamera;
    public Camera eventCamera;

    [Header("CameraPositions")]
    public Vector3 lobbyPosition;
    public Vector3 defaultPosition;
    public Vector3 playAreaPosition, locationPosition;
    public Vector3 recruitmentEnterPos, recruitmentPos, recruitmentEnterRot, recruitmentRot;
    public Vector3 basePosition, baseRotation;
    [SerializeField] List<Vector2> hideHandCameraInPos = new List<Vector2>();
    Vector2 abstractCameraLoc;
    Dictionary<Vector2, Vector3> positions = new Dictionary<Vector2, Vector3>();
    Dictionary<Vector2, Vector3> rotations = new Dictionary<Vector2, Vector3>();
    List<int> playerXPositions = new List<int>() { 0, 0, 0, 0, 0 };

    [Header("ExploreMode")]
    public Vector3 exploreModePos, exploreModeRot;
    public Vector3[] eventCameraPathPos;
    public Vector3[] eventCameraPathRot;
    public float eventCameraMoveTime;

    [Header("Animations")]
    public float cameraMoveTime;

    [Header("Blockers")]
    public GameObject ui;

    bool handCameraActive;
    bool isAnimating;

    public void Awake()
    {
        main = this;
    }

    private void Start()
    {
        abstractCameraLoc = Vector2.zero;

        positions.Add(Vector2.zero, defaultPosition);
        positions.Add(new Vector2(0f, 1f), playAreaPosition);
        positions.Add(new Vector2(0f, 2f), locationPosition);
        positions.Add(new Vector2(0f, -1f), new Vector3(0f, 13f, -60f));
        rotations.Add(Vector2.zero, Vector3.zero);
        rotations.Add(new Vector2(0f, 1f), Vector3.zero);
        rotations.Add(new Vector2(0f, 2f), Vector3.zero);
        rotations.Add(new Vector2(0f, -1f), Vector3.zero);
    }

    public void ResetCamera()
    {
        abstractCameraLoc = Vector2.zero;
        mainCamera.transform.position = positions[Vector2.zero];
        mainCamera.transform.localEulerAngles = rotations[Vector2.zero];
        TurnOffHandCamera();
        EndEventCamera();
    }

    public void AddCameraPositions(Vector2 abstractPos, Vector3 position, Vector3 rotation, int player)
    {
        positions.Add(abstractPos, position);
        rotations.Add(abstractPos, rotation);
        playerXPositions[player] = (int)abstractPos.x;
    }

    public void TurnOnHandCamera()
    {
        handCameraActive = true;
        activePlayerHandCamera.gameObject.SetActive(handCameraActive);
    }

    public void TurnOffHandCamera()
    {
        handCameraActive = false;
        activePlayerHandCamera.gameObject.SetActive(handCameraActive);
    }

    public void MoveCameraToHomeBase()
    {
        TurnOffHandCamera();
        mainCamera.transform.localPosition = basePosition;
        mainCamera.transform.localEulerAngles = baseRotation;
    }

    public void MoveCameraToLobby()
    {
        TurnOffHandCamera();
        mainCamera.transform.localPosition = lobbyPosition;
        mainCamera.transform.localEulerAngles = Vector3.zero;
    }

    public void MoveCameraToExploreMode()
    {
        TurnOffHandCamera();
        StartCoroutine(MoveCamera(positions[abstractCameraLoc], exploreModePos, rotations[abstractCameraLoc], exploreModeRot));
    }

    public void MoveCameraToActiveZone(int zone)
    {
        Debug.Log("Moving camera to active player zone " + zone + " with abstract PosX " + playerXPositions[zone]);
        SetCameraPosition(new Vector2(playerXPositions[zone], 0f));
    }

    public void SetCameraPosition(Vector2 abstractPos)
    {
        if (positions.ContainsKey(abstractPos))
        {
            StartCoroutine(MoveCamera(positions[abstractCameraLoc], positions[abstractPos], rotations[abstractCameraLoc], rotations[abstractPos]));
            abstractCameraLoc = abstractPos;

            if (hideHandCameraInPos.Contains(abstractCameraLoc))
            {
                TurnOffHandCamera();
            }
            else
            {
                TurnOnHandCamera();
            }
        }
    }

    public void MoveCamera(Vector2 dir)
    {
        if (cameraInRecuitment) { MoveFromRecruit(); return; }

        Vector2 proposedDirection = abstractCameraLoc + dir;
        
        //Move to location position from any play area
        if(proposedDirection.y == 2f) { proposedDirection = new Vector2(0f, proposedDirection.y); }
        //Move sideways to other players play area regardless of in hand or playarea view.
        else if (proposedDirection.x != abstractCameraLoc.x) { proposedDirection = new Vector2(proposedDirection.x, 0f); }
        else if (proposedDirection.y == -1f) { proposedDirection = new Vector2(0f, -1f); }


        if (positions.ContainsKey(proposedDirection))
        {
            StartCoroutine(MoveCamera(positions[abstractCameraLoc], positions[proposedDirection], rotations[abstractCameraLoc], rotations[proposedDirection]));
            abstractCameraLoc = proposedDirection;

            if (hideHandCameraInPos.Contains(abstractCameraLoc))
            {
                TurnOffHandCamera();
            }
            else
            {
                TurnOnHandCamera();
            }
        }
    }

    private void Update()
    {
        if (isAnimating || ui.activeInHierarchy || GameController.main.phase == GameController.Phase.Explore) { return; }

        if (Input.GetKeyDown("w"))
        {
            MoveCamera(Vector2.up);
        }

        if (Input.GetKeyDown("s"))
        {
            MoveCamera(Vector2.down);
        }

        if (Input.GetKeyDown("a"))
        {
            MoveCamera(Vector2.right);
        }

        if (Input.GetKeyDown("d"))
        {
            MoveCamera(Vector2.left);
        }

        if (Input.GetKeyDown("r"))
        {
            MoveToRecruit();
        }
    }

    public void StartEventCamera()
    {
        eventCamera.gameObject.SetActive(true);
        StartCoroutine(MoveEventCamera());
    }

    public void EndEventCamera()
    {
        eventCamera.gameObject.SetActive(false);
    }

    public void MoveToRecruit()
    {
        if (cameraInRecuitment) { MoveFromRecruit(); return; }
        StartCoroutine(MoveCameraToRecruitment());
    }

    public void MoveFromRecruit()
    {
        if(!cameraInRecuitment) { return; }
        StartCoroutine(MoveCameraFromRecruitment());
    }

    IEnumerator MoveCamera(Vector3 from, Vector3 to, Vector3 fromRot, Vector3 toRot)
    {
        isAnimating = true;

        mainCamera.transform.position = from;
        float timeElapsed = 0f;

        while(timeElapsed < cameraMoveTime)
        {
            mainCamera.transform.position = Vector3.Lerp(from, to, timeElapsed / cameraMoveTime);
            mainCamera.transform.localEulerAngles = Vector3.Lerp(fromRot, toRot, timeElapsed / cameraMoveTime);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = to;
        mainCamera.transform.localEulerAngles = toRot;

        isAnimating = false;
    }

    Vector3 returnPos, returnRot;
    bool cameraInRecuitment;

    IEnumerator MoveCameraToRecruitment()
    {
        isAnimating = true;
        cameraInRecuitment = true;

        Vector3 from = mainCamera.transform.position;
        returnPos = from;
        Vector3 to = new Vector3(recruitmentEnterPos.x, recruitmentEnterPos.y, from.z);
        Vector3 fromRot = mainCamera.transform.localEulerAngles;
        returnRot = fromRot;
        Vector3 toRot = recruitmentEnterRot;
        float dur = cameraMoveTime / 2;

        //Move to above ship
        mainCamera.transform.position = from;
        float timeElapsed = 0f;

        while (timeElapsed < dur)
        {
            mainCamera.transform.position = Vector3.Lerp(from, to, timeElapsed / dur);
            mainCamera.transform.localEulerAngles = Vector3.Lerp(fromRot, toRot, timeElapsed / dur);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        //Sink into ship
        timeElapsed = 0f;

        from = mainCamera.transform.position;
        to = recruitmentEnterPos;

        while (timeElapsed < dur)
        {
            mainCamera.transform.position = Vector3.Lerp(from, to, timeElapsed / dur);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        Player.active.zones.recruit.SpawnCoins();

        mainCamera.transform.position = recruitmentPos;
        mainCamera.transform.localEulerAngles = recruitmentRot;

        isAnimating = false;
    }

    IEnumerator MoveCameraFromRecruitment()
    {
        isAnimating = true;

        Vector3 from = recruitmentEnterPos;
        Vector3 to = new Vector3(recruitmentEnterPos.x, recruitmentEnterPos.y, returnPos.z);
        float dur = cameraMoveTime / 2;

        //Move to above ship
        mainCamera.transform.position = from;
        mainCamera.transform.localEulerAngles = recruitmentEnterRot;
        float timeElapsed = 0f;

        while (timeElapsed < dur)
        {
            mainCamera.transform.position = Vector3.Lerp(from, to, timeElapsed / dur);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        //Move back to pos
        timeElapsed = 0f;

        from = mainCamera.transform.position;
        to = returnPos;
        Vector3 fromRot = mainCamera.transform.localEulerAngles;
        Vector3 toRot = returnRot;

        while (timeElapsed < dur)
        {
            mainCamera.transform.position = Vector3.Lerp(from, to, timeElapsed / dur);
            mainCamera.transform.localEulerAngles = Vector3.Lerp(fromRot, toRot, timeElapsed / dur);

            timeElapsed += Time.deltaTime;
            yield return null;
        }


        mainCamera.transform.position = returnPos;
        mainCamera.transform.localEulerAngles = returnRot;

        isAnimating = false;
        cameraInRecuitment = false;
    }

    IEnumerator MoveEventCamera()
    {
        float timeElapsed = 0f;
        eventCamera.transform.localPosition = eventCameraPathPos[0];
        eventCamera.transform.localEulerAngles = eventCameraPathRot[0];

        Vector3 start = eventCamera.transform.localPosition;
        Vector3 startRot = eventCamera.transform.localEulerAngles;
        Vector3 end = Vector3.zero;
        Vector3 endRot = Vector3.zero;


        for(int i = 1; i < eventCameraPathPos.Length; i++)
        {
            end = eventCameraPathPos[i];
            endRot = eventCameraPathRot[i]; 

            while(timeElapsed < eventCameraMoveTime)
            {
                eventCamera.transform.localPosition = Vector3.Lerp(start, end, timeElapsed / eventCameraMoveTime);
                eventCamera.transform.localEulerAngles = Vector3.Lerp(startRot, endRot, timeElapsed / eventCameraMoveTime);

                timeElapsed += Time.deltaTime;
                yield return null;
            }

            eventCamera.transform.localPosition = end;
            eventCamera.transform.localEulerAngles = endRot;

            start = eventCamera.transform.localPosition;
            startRot = eventCamera.transform.localEulerAngles;
            timeElapsed = 0f;
        }
    }

}
