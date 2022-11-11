using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class ButtonsManager : MonoBehaviour
{
    public TMP_Dropdown OrderSequenceDropdown;
    public TMP_Dropdown StartingPositionDropdown;
    public Button NextButton;
    public GameObject panel;

    private string SaveDocumentPath = Application.streamingAssetsPath + "/Calibration_Saved_Data/" + "CalibrationSave" + ".txt";
    private string separation = "/";
    private BrushMovementBackAndForth brushMovement;
    private int PathIndex = -1;
    private int SequenceIndex;
    private int[][] randomSequences = new int[12][];
    private ConnectDevice StimManager;


    // Start is called before the first frame update
    void Start()
    {
        StimManager = FindObjectOfType<ConnectDevice>();
        Time.timeScale = 0;
        //create the folder
        Directory.CreateDirectory(Application.streamingAssetsPath + "/Calibration_Saved_Data/");

        brushMovement = FindObjectOfType<BrushMovementBackAndForth>();

        randomSequences[0] = new int[] { 1, 2, 3, 4, 5, 6 };
        randomSequences[1] = new int[] { 4, 5, 6, 1, 2, 3 };
        randomSequences[2] = new int[] { 1, 3, 2, 4, 6, 5 };
        randomSequences[3] = new int[] { 4, 6, 5, 1, 3, 2 };
        randomSequences[4] = new int[] { 2, 1, 3, 5, 4, 6 };
        randomSequences[5] = new int[] { 5, 4, 6, 2, 1, 3 };
        randomSequences[6] = new int[] { 2, 3, 1, 5, 6, 4 };
        randomSequences[7] = new int[] { 5, 6, 4, 2, 3, 1 };
        randomSequences[8] = new int[] { 3, 1, 2, 6, 4, 5 };
        randomSequences[9] = new int[] { 6, 4, 5, 3, 1, 2 };
        randomSequences[10] = new int[] { 3, 2, 1, 6, 5, 4 };
        randomSequences[11] = new int[] { 6, 5, 4, 3, 2, 1 };
    }

    public void ReadSaveFile()
    {

        if (File.Exists(SaveDocumentPath))
        {
            List<string> fileLines = File.ReadAllLines(SaveDocumentPath).ToList();

            for (int i = 0; i < fileLines.Count; i++)
            {

                string amplitude = "";
                string pulseWidth = "";
                string frequency = "";
                int separationCount = 0;
                PadScript.Pad currentPad = ConnectDevice.Remap2[i];

                foreach (char c in fileLines[i])
                {
                    if (c.ToString() == separation)
                        separationCount++;
                    else
                    {
                        switch (separationCount)
                        {
                            case 0:
                                amplitude += c;
                                break;
                            case 1:
                                pulseWidth += c;
                                break;
                            case 2:
                                frequency += c;
                                break;
                        }
                    }

                }

                currentPad.SetAmplitude(float.Parse(amplitude));
                currentPad.SetPulseWidth(int.Parse(pulseWidth));
                currentPad.SetFrequency(int.Parse(frequency));

            }
        }

    }

    public void PlayNextPattern() {
        if (PathIndex < randomSequences[SequenceIndex].Length - 1)
        {
            PathIndex += 1;
            NextButton.interactable = false;
            //print(SequenceIndex);
            //print(PathIndex);
            brushMovement.playPath(randomSequences[SequenceIndex][PathIndex]);
            //NextButton.interactable = true;
        }

        else
        {
            NextButton.interactable = false;
            ConnectDevice.glovePort.Write("stim off\r\n");
        }

    }


    public void StartButton()
    {
        ConnectDevice.glovePort.Write("velec 11 *selected 0\r\n");
        brushMovement.Stop();
        NextButton.interactable = true;
        SequenceIndex = OrderSequenceDropdown.value;
        PathIndex = StartingPositionDropdown.value - 1;
        panel.SetActive(false);
        Time.timeScale = 1;
    }

    public int getActualCondition()
    {
        return randomSequences[SequenceIndex][PathIndex];
    }

    public void ReturnToCalibration()
    {
        ConnectDevice.glovePort.Write("stim off\r\n");
        Thread.Sleep(150);
        SceneManager.LoadScene(0);
        DontDestroyOnLoad(StimManager.gameObject);
    }

    public void showMenu()
    {
        panel.SetActive(true);
    }

    public void Close()
    {
        if (ConnectDevice.connectedMessage == "Re:[] new connection" || ConnectDevice.connectedMessage == "Re:[] re-connection")
        {
            ConnectDevice.glovePort.Write("stim off\r\n");
            ConnectDevice.glovePort.Close();
        }
        Application.Quit();
    }

}
