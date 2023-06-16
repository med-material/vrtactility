using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class GoalManager : MonoBehaviour
{
    public GameObject experimentSetup;

    public Transform rightHand;
    public Transform leftHand;

    public TextMeshProUGUI counterText;
    public TextMeshProUGUI msgText;
    public GameObject goalObject;
    public ParticleSystem particleSystem;
    public float goalDiff;
    public int totalScore = 0;
    public int corrects = 0;
    public int fails = 0;
    public float diff;
    private Vector3 originPos;
    private Vector3 currentPos;
    private GoalObjectLogic gl;
    Animator animator;

    Func<string> getGoalInfo;
    Func<string> getObjectPos;
    // Start is called before the first frame update
    private void Awake()
    {
        getGoalInfo = () => { return "" + corrects + ", " + fails + ", " + totalScore; };
        getObjectPos = () => { return ""+goalObject.transform.position.x + ":" + goalObject.transform.position.y + ":" + goalObject.transform.position.z; };

    }
    void Start()
    {
        CSVLogger.instance.addNewData("CORRECTS,FAILS,TOTAL ATTEMPTS", getGoalInfo);
        CSVLogger.instance.addNewData("GOAL OBJECT POSITION",getObjectPos);
        animator = GetComponent<Animator>();
        originPos = goalObject.transform.position;
        gl = goalObject.GetComponent<GoalObjectLogic>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (animator.GetCurrentAnimatorClipInfo(0).Length != 0) return;
            currentPos = goalObject.transform.position;
        if (currentPos == originPos|| gl.hidden) return;
        diff = Mathf.Abs(currentPos.y - originPos.y);
        if (diff >= goalDiff)
        {
            animator.Play("Corr");
        }
        else if (gl.pop())
        {
            animator.Play("Fail");
        }
    }
#if UNITY_EDITOR
    public bool moveGoalObjectToRightHand = false;
    private void moveToRightHand()
    {
        experimentSetup.transform.position = rightHand.position + new Vector3(0, 0.15f, 0);
        gl._originPoint = goalObject.transform.position;
        originPos = goalObject.transform.position;
    }
    public bool moveGoalObjectToLeftHand = false;
    private void moveToLeftHand()
    {
        experimentSetup.transform.position = leftHand.position + new Vector3(0, 0.15f, 0);
        gl._originPoint = goalObject.transform.position;
        originPos = goalObject.transform.position;
    }
    private void OnValidate()
    {
        if (moveGoalObjectToRightHand)
        {
            moveToRightHand();
            moveGoalObjectToRightHand = false;
        }
        if (moveGoalObjectToLeftHand)
        {
            moveToLeftHand();
            moveGoalObjectToLeftHand = false;
        }

    }


#endif
    public void incrementCorrects()
    {
        corrects++;
    }
    public void incrementTotal()
    {
        totalScore++;
    }
    public void incrementFails()
    {
        fails++;
    }

    public void playBurst()
    {
        particleSystem.Play();
    }

    public void updateCounter()
    {
        counterText.text = ""+corrects;
    }

    public void correctMsg()
    {
        msgText.text = "good";
    }
    public void failMsg()
    {
        msgText.text = "try again";
    }

    public void hideMsg()
    {
        msgText.text = "";
    }

    public void hideObj()
    {
        gl.hideObject();
    }

    public void resetObj()
    {
        gl.resetObject();
    }
}
