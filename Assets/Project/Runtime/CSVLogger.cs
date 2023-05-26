using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CSVLogger : MonoBehaviour
{
    delegate int intDelegate();
    delegate string stingDelegate();
    delegate float floatDelegate();

     List<string> logHeaders;
     Dictionary<string, dynamic> logPairs;

    string date = DateTime.Today.ToString();
    public int participantID;
    Func<string> scene;
    Func<float> time;
    Func<int> FPS;

    private void Awake()
    {
        logPairs = new Dictionary<string, dynamic>();
        scene = () => { return SceneManager.GetActiveScene().name; };
        time = () => { return Time.time; };
        FPS = () => { return (int)(1f / Time.unscaledDeltaTime); };
    }

    void Start()
    {
        addMetaData();

    }

    
    void LateUpdate()
    {

    }

    void addMetaData()
    {
        
        addNewData("DATE",date);       
        addNewData("ID", participantID);
        addNewData("SCENE", scene);
        addNewData("TIME", time);
        addNewData("FPS", FPS);

    }

    void initCSV()
    {

    }

    void writeToCSV()
    {

    }

    public void addNewData(string header, dynamic data)
    {
        logHeaders.Add(header);
        logPairs.Add(header, data);
    }

}
