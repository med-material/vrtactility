using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

public class BrushMovementBackAndForth : MonoBehaviour
{
    //public int PathsSize = 2;
    public string FingerTag = "Finger";
    public GameObject[] PathsObject;
    public string TargetTag = "Target";
    public float speedMultiplier = 1f;
    public int LoopsAll = 8;
    public int LoopsIndex = 15;
    public Patterns pattern;
    public Button NextButton;
    public Button MenuButton;
    private int loops = 1;
    private float speedAll = 0.065f;
    private float speedIndex = 0.057f;
    private float speed;
    private bool playing = false;
    //public float secondsBetweenTargets = 1;
    private GameObject[][] PathsArray;
    //private int currentLoop;
    private Transform target1;
    private Transform target2;
    //private int index;

    private float currentTime;
    private float waitingTime = 1;
    //private int 
    //private int index;

    // Start is called before the first frame update
    void Start()
    {
        speed = speedAll;
        //PathsObject = new GameObject[PathsSize];
        PathsArray = new GameObject[PathsObject.Length][];

        for (int i = 0; i < PathsObject.Length; i++)
        {
            PathsArray[i] = new GameObject[PathsObject[i].transform.childCount];

            for (int j = 0; j < PathsArray[i].Length; j++)
            {
                PathsArray[i][j] = PathsObject[i].transform.GetChild(j).gameObject;
            }
        }


        //target1 = PathsArray[i][0].transform.GetChild(0);
        //target2 = PathsArray[i][0].transform.GetChild(1);
        //this.transform.position = target1.position;

        //index = currentLoop = 0;
        //Speed = Vector3.Distance(target1.position, target2.position) / secondsBetweenTargets;

    }

    // Update is called once per frame
    void Update()
    {
        currentTime = currentTime + Time.deltaTime;

        //if ((!playing) && (PathsObject.Length > 0 && Input.GetKeyDown(KeyCode.Alpha1)))
        //{
        //    StopAllCoroutines();
        //    //print("uno");
        //    pattern.UpdatePattern(1);
        //    StartCoroutine(StartLoop(0));
        //}

        //if ((!playing) && (PathsObject.Length > 0 && Input.GetKeyDown(KeyCode.Alpha2)))
        //{
        //    StopAllCoroutines();
        //    //print("dos");
        //    pattern.UpdatePattern(2);
        //    StartCoroutine(StartLoop(0));
        //}

        //if ((!playing) && (PathsObject.Length > 1 && Input.GetKeyDown(KeyCode.Alpha3)))
        //{
        //    StopAllCoroutines();
        //    //print("dos");
        //    pattern.UpdatePattern(3);
        //    StartCoroutine(StartLoop(1));
        //}

        //if ((!playing) && (PathsObject.Length > 1 && Input.GetKeyDown(KeyCode.Alpha4)))
        //{
        //    StopAllCoroutines();
        //    //print("dos");
        //    pattern.UpdatePattern(4);
        //    StartCoroutine(StartLoop(1));
        //}

    }

    public void playPath(int index) {
        switch (index)
        {
            case 1:
                StopAllCoroutines();
                ConnectDevice.glovePort.Write("velec 11 *selected 0\r\n");
                speed = speedAll;
                loops = LoopsAll;
                StartCoroutine(StartLoop(0));
                break;

            case 2:
                StopAllCoroutines();
                pattern.UpdatePattern(1);
                speed = speedAll;
                loops = LoopsAll;
                StartCoroutine(StartLoop(0));
                break;

            case 3:
                StopAllCoroutines();
                pattern.UpdatePattern(2);
                speed = speedAll;
                loops = LoopsAll;
                StartCoroutine(StartLoop(0));
                break;

            case 4:
                StopAllCoroutines();
                ConnectDevice.glovePort.Write("velec 11 *selected 0\r\n");
                speed = speedIndex;
                loops = LoopsIndex;
                StartCoroutine(StartLoop(1));
                break;

            case 5:
                StopAllCoroutines();
                pattern.UpdatePattern(3);
                speed = speedIndex;
                loops = LoopsIndex;
                StartCoroutine(StartLoop(1));
                break;

            case 6:
                StopAllCoroutines();
                pattern.UpdatePattern(4);
                speed = speedIndex;
                loops = LoopsIndex;
                StartCoroutine(StartLoop(1));
                break;

        }
    }


    IEnumerator StartLoop(int index)
    {
        playing = true;
        //MenuButton.interactable = false;
        Vector3 targetDirection;
        float enterTime = currentTime;
        for (int l = 0; l < loops; l++)
        {
            this.transform.position = PathsArray[index][0].transform.GetChild(0).position;

            for (int i = 0; i < PathsArray[index].Length; i++)
            {
                target1 = PathsArray[index][i].transform.GetChild(0);
                target2 = PathsArray[index][i].transform.GetChild(1);

                if (this.transform.position != target2.position)
                {
                    //Speed = (Vector3.Distance(this.transform.position, target1.position) / secondsBetweenTargets) * 2;

                    while (this.transform.position != target1.position)
                    {
                        targetDirection = target1.position - transform.position;
                        targetDirection.y = 0;
                        //this.transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, targetDirection, Speed * Time.deltaTime, 0.0f));
                        this.transform.rotation = Quaternion.LookRotation(targetDirection);
                        this.transform.position = Vector3.MoveTowards(this.transform.position, target1.position, speed * speedMultiplier * Time.deltaTime);
                        yield return null;
                    }

                }

                //Speed = Vector3.Distance(target1.position, target2.position) / secondsBetweenTargets;


                while (this.transform.position != target2.position)
                {
                    targetDirection = target2.position - transform.position;
                    targetDirection.y = 0;
                    //this.transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, targetDirection, Speed * Time.deltaTime, 0.0f));
                    this.transform.rotation = Quaternion.LookRotation(targetDirection);
                    this.transform.position = Vector3.MoveTowards(this.transform.position, target2.position, speed * speedMultiplier * Time.deltaTime);
                    yield return null;
                }
                //invert targets
                Transform tempTarget = target1;
                target1 = target2;
                target2 = tempTarget;

                //yield return null;
            }

            for (int i = PathsArray[index].Length - 1; i >= 0; i--)
            {
                target1 = PathsArray[index][i].transform.GetChild(1);
                target2 = PathsArray[index][i].transform.GetChild(0);

                if (this.transform.position != target2.position)
                {
                    //Speed = (Vector3.Distance(this.transform.position, target1.position) / secondsBetweenTargets) * 2;

                    while (this.transform.position != target1.position)
                    {
                        targetDirection = target1.position - transform.position;
                        targetDirection.y = 0;
                        //this.transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, targetDirection, Speed * Time.deltaTime, 0.0f));
                        this.transform.rotation = Quaternion.LookRotation(targetDirection);
                        this.transform.position = Vector3.MoveTowards(this.transform.position, target1.position, speed * speedMultiplier * Time.deltaTime);
                        yield return null;
                    }

                }

                //Speed = Vector3.Distance(target1.position, target2.position) / secondsBetweenTargets;

                while (this.transform.position != target2.position)
                {
                    targetDirection = target2.position - transform.position;
                    targetDirection.y = 0;
                    //this.transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, targetDirection, Speed * Time.deltaTime, 0.0f));
                    this.transform.rotation = Quaternion.LookRotation(targetDirection);
                    this.transform.position = Vector3.MoveTowards(this.transform.position, target2.position, speed * speedMultiplier * Time.deltaTime);
                    yield return null;
                }
                //invert targets
                Transform tempTarget = target1;
                target1 = target2;
                target2 = tempTarget;

                yield return null;
            }
            float exitTime = currentTime;
            //print(currentTime - exitTime);
            while ((currentTime - exitTime) < waitingTime)
            {
                yield return null;
            }

        }

        //print(currentTime - enterTime);
        playing = false;
        NextButton.interactable = true;
        //MenuButton.interactable = true;
        
    }

    public void Stop()
    {
        StopAllCoroutines();
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.tag == FingerTag)
    //    {
    //        print("enter time: " + currentTime);
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.tag == FingerTag)
    //    {
    //        print("exit time: " + currentTime);
    //    }
    //}
}
