using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalManager : MonoBehaviour
{

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

    // Start is called before the first frame update
    void Start()
    {
        originPos = gameObject.transform.position;
        gl = gameObject.GetComponent<GoalObjectLogic>();
    }

    // Update is called once per frame
    void Update()
    {
       currentPos = gameObject.transform.position;
        if (currentPos == originPos) return;
        diff = Mathf.Abs(currentPos.y - originPos.y);
        if (diff >= goalDiff)
        {
            gameObject.transform.position = originPos;
            totalScore++;
            corrects++;
        }
        if (gl.pop())
        {
            particleSystem.Play();
            gameObject.transform.position = originPos;
            fails++;
        }
    }
}
