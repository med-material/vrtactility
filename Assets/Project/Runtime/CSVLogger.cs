using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CSVLogger : MonoBehaviour
{
    public static CSVLogger instance { get; private set; }

     List<string> logHeaders;
     Dictionary<string, dynamic> logPairs;

    string date = DateTime.Today.ToString();
    public string participantID;
    Func<string> scene;
    Func<string> time;
    Func<string> FPS;

    TextWriter tw;

    string CSVName = "";

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        logHeaders = new List<string>();
        logPairs = new Dictionary<string, dynamic>();
        scene = () => { return SceneManager.GetActiveScene().name; };
        time = () => { return Time.time.ToString(); };
        FPS = () => { return"" + (int)(1f / Time.unscaledDeltaTime); };
        //CSVName = Application.dataPath +"/" +"_ID_" + participantID+".csv";
        CSVName = Application.dataPath + "/test.csv";
    }

    void Start()
    {
        addMetaData();
        initCSV();
    }

    void LateUpdate()
    {
        StartCoroutine("writeToCSV");
        
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

         tw = new StreamWriter(CSVName,false);
        string headerString  ="";
        foreach(string name in logHeaders)
        {
            headerString += name + ",";
        }
        tw.WriteLine(headerString);
        tw.Close();
    }

    IEnumerator writeToCSV()
    {
        yield return new WaitForSeconds(1.5f);
        string input = "";
         tw = new StreamWriter(CSVName, true);
        foreach (string name in logHeaders)
        {
            if (logPairs[name] is string)
            {
                input += logPairs[name]+",";
            }
            else
            {
                input +=    logPairs[name].Invoke()+",";
            }
        }
        
        print(input);tw.WriteLine(input);
        tw.Close();
    }

    public void addNewData(string header, dynamic data)
    {

        logHeaders.Add(header);
        logPairs.Add(header, data);
    }
    
}
