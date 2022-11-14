using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrushMovement2 : MonoBehaviour
{
    //public int PathsSize = 2;
    public GameObject[] PathsObject;
    public string TargetTag = "Target";
    public float Speed = 0.1f;
    public int Loops = 3;
    //public float secondsBetweenTargets = 1;
    private GameObject[][] PathsArray;
    //private int currentLoop;
    private Transform target1;
    private Transform target2;
    //private int index;

    private float currentTime;
    //private int 
    //private int index;

    // Start is called before the first frame update
    void Start()
    {
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

        if (PathsObject.Length > 0 && Input.GetKeyDown(KeyCode.Alpha1))
        {
            StopAllCoroutines();
            print("uno");
            StartCoroutine(StartLoop(0));
        }

        if (PathsObject.Length > 1 && Input.GetKeyDown(KeyCode.Alpha2))
        {
            StopAllCoroutines();
            print("dos");
            StartCoroutine(StartLoop(1));
        }

    }


    IEnumerator StartLoop(int index)
    {
        for (int l = 0; l < Loops; l++)
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
                        this.transform.position = Vector3.MoveTowards(this.transform.position, target1.position, Speed * Time.deltaTime);
                        yield return null;
                    }

                }

                print(currentTime);
                //Speed = Vector3.Distance(target1.position, target2.position) / secondsBetweenTargets;

                while (this.transform.position != target2.position)
                {
                    this.transform.position = Vector3.MoveTowards(this.transform.position, target2.position, Speed * Time.deltaTime);
                    yield return null;
                }
                //invert targets
                print(currentTime);
                Transform tempTarget = target1;
                target1 = target2;
                target2 = tempTarget;

                yield return null;
            }
        }
        
    }
 }
