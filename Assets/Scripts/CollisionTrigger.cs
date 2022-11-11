using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading;

public class CollisionTrigger : MonoBehaviour
{
    public static int indexHelping = -1;
    public static int indexPattern = 0;
    public Patterns patterns;
    private float AllFIngersTime = 0.4f;
    private float IndexTime = 1.4f;
    private BrushMovementBackAndForth brushMovement;
    private ButtonsManager buttonsManager;
    private float currentPatternTime;
    private Patterns.Pattern actualPattern;
    private int count = 0;
    private float timeElapsed = 0;

    private int[] helpingArrayAllSimple, helpingArrayAllComplex, helpingArrayIndexSimple, helpingArrayIndexComplex, currentArray;

    // Start is called before the first frame update
    void Start()
    {
        buttonsManager = FindObjectOfType<ButtonsManager>();
        brushMovement = FindObjectOfType<BrushMovementBackAndForth>();
        helpingArrayAllSimple = new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        helpingArrayAllComplex = new int[] { 3, 3, 3, 3, 1, 1, 3, 3, 3, 3 };
        helpingArrayIndexSimple = new int[] { 1 };
        helpingArrayIndexComplex = new int[] { 10 };

        patternUpdated();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        timeElapsed = 0;
        int actualPath = buttonsManager.getActualCondition();
        if ((actualPath != 1) && (actualPath != 4))
        {
            indexHelping++; 
            count++;
            //PlayStim();
            StopAllCoroutines();
            StartCoroutine(PlayStimCoroutine());
        }
    }

    private void OnTriggerStay(Collider other)
    {
        timeElapsed += Time.deltaTime;
        //print(timeElapsed);
    }

    private void OnTriggerExit(Collider other)
    {
        //ConnectDevice.glovePort.Write("stim off\r\n");
        if (count > 1)
            count = 0;
        print(name + ": " + timeElapsed);
    }

    //IEnumerator playStim() {
    //    for (int i = 0; i < currentArray[indexHelping]; i++)
    //    {
    //        ConnectDevice.glovePort.Write(patterns.ActivateString(indexPattern));
    //        Thread.Sleep(currentPatternTime/ currentArray[indexHelping]);
    //        if (indexPattern + 1 < currentArray.Sum())
    //        {
    //            indexPattern++;
    //        }
    //        else {
    //            indexPattern = 0;
    //        }
    //    }
    //    yield return null;
    //}

    //public void PlayStim()
    //{
    //    patternUpdated();

    //    int loops = currentArray[indexHelping];

    //    ConnectDevice.glovePort.Write("stim on\r\n");
    //    float waitTime = 0;
    //    while (waitTime < 0.02)
    //    {
    //        waitTime += Time.deltaTime;
    //        print(waitTime);
    //    }

    //    for (int i = 0; i < loops; i++)
    //    {
    //        ConnectDevice.glovePort.Write(patterns.ActivateString(indexPattern));
    //        print(patterns.ActivateString(indexPattern));
    //        //Thread.Sleep(currentPatternTime / currentArray[indexHelping]);
    //        float stimTime = 0;
    //        //print(currentPatternTime);
    //        while (stimTime < (currentPatternTime * (1/brushMovement.speedMultiplier) / loops)) {
    //            stimTime += Time.deltaTime;
    //            print(stimTime);
    //        }
    //        //print(stimTime);
    //        if (indexPattern + 1 < currentArray.Sum())
    //        {
    //            indexPattern++;

    //        }
    //        else
    //        {
    //            indexPattern = 0;
    //            indexHelping = -1;
    //        }
    //    }

    //    //ConnectDevice.glovePort.Write("velec 11 *special_anodes 1 *name test *elec 1 *pads 19=C, *amp 19=0, *width 19=300, *selected 0 *sync 0\r\n");
    //    //print("velec 11 *special_anodes 1 *name test *elec 1 *pads 19=C, *amp 19=0, *width 19=300, *selected 0 *sync 0\r\n");
    //    //ConnectDevice.glovePort.Write("velec 11 *selected 0\r\n");
    //    //print("velec 11 *selected 0\r\n");
    //}

    IEnumerator PlayStimCoroutine()
    {
        patternUpdated();

        int loops = currentArray[indexHelping];

        //ConnectDevice.glovePort.Write("stim on\r\n");
        float waitTime = 0;
        while (waitTime < 0.02)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < loops; i++)
        {
            ConnectDevice.glovePort.Write(patterns.ActivateString(indexPattern));
            print(patterns.ActivateString(indexPattern));
            //Thread.Sleep(currentPatternTime / currentArray[indexHelping]);
            float stimTime = 0;
            //print(currentPatternTime);
            while (stimTime < (currentPatternTime * (1/brushMovement.speedMultiplier) / loops))
            {
                stimTime += Time.deltaTime;
                yield return null;
            } 
            if (indexPattern + 1 < currentArray.Sum())
            {
                indexPattern++;

            }
            else
            {
                indexPattern = 0;
                indexHelping = -1;
            }
        }
        ConnectDevice.glovePort.Write("velec 11 *selected 0\r\n");
    }

    public void patternUpdated()
    {
        if (actualPattern != patterns.currentPattern) {
            actualPattern = patterns.currentPattern;
            switch (actualPattern)
            {
                case Patterns.Pattern.AllSimple:
                    currentArray = helpingArrayAllSimple;
                    currentPatternTime = AllFIngersTime;
                    break;
                case Patterns.Pattern.AllComplex:
                    currentArray = helpingArrayAllComplex;
                    currentPatternTime = AllFIngersTime;
                    break;
                case Patterns.Pattern.IndexSimple:
                    currentArray = helpingArrayIndexSimple;
                    currentPatternTime = IndexTime;
                    break;
                case Patterns.Pattern.IndexComplex:
                    currentArray = helpingArrayIndexComplex;
                    currentPatternTime = IndexTime;
                    break;
            }
        }
    }
}
