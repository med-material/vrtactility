using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GoalManager : MonoBehaviour
{

    public TextMeshProUGUI counterText;
    public TextMeshProUGUI msgText;
    public GameObject gameObject;
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

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        originPos = gameObject.transform.position;
        gl = gameObject.GetComponent<GoalObjectLogic>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (animator.GetCurrentAnimatorClipInfo(0).Length != 0) return;
            currentPos = gameObject.transform.position;
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
