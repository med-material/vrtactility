using System.Collections;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Linq;

public class UiManager : MonoBehaviour
{
    public GameObject Panel;
    //public GameObject ConnectionPanel;
    public TextMeshProUGUI textTitle;
    public TMP_InputField AmplitudeBox;
    public TMP_InputField PulseWidthBox;
    public TMP_InputField FrequencyBox;
    public TMP_Text BatteryLifeText;
    public Button IncreaseButton;
    public Button DecreaseButton;
    public Button InitialCalibrationButton;
    public Button SaveCalibrationButton;
    public Button RestoreCalibationButton;
    public Toggle StimulationToggle;
    public ConnectDevice StimManager;
    private int currentPad = 0;
    private string stringStart = "Re:[] battery *capacity=";
    private string SaveDocumentPath = Application.streamingAssetsPath + "/Calibration_Saved_Data/" + "CalibrationSave" + ".txt";
    private string separation = "/";

    private void Awake()
    {
        if (ConnectDevice.connectedMessage == "Re:[] new connection" || ConnectDevice.connectedMessage == "Re:[] re-connection") {
            Panel.SetActive(false);
            Destroy(StimManager.gameObject);
            StimManager = FindObjectOfType<ConnectDevice>();
        }
    }
        

    // Start is called before the first frame update
    void Start()
    {
        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
        AmplitudeBox.characterValidation = TMP_InputField.CharacterValidation.Decimal;
        PulseWidthBox.characterValidation = TMP_InputField.CharacterValidation.Integer;
        FrequencyBox.characterValidation = TMP_InputField.CharacterValidation.Integer;

        //create the folder
        Directory.CreateDirectory(Application.streamingAssetsPath + "/Calibration_Saved_Data/");

        //textTitle = panel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        //for (int i = 0; i < 32; i++)
        //{
        //    print(StimManager.GetPadsInfo(i).GetRemap());
        //}
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SelectPad()
    {
        string buttonTag = EventSystem.current.currentSelectedGameObject.tag;
        currentPad = Convert.ToInt32(buttonTag) - 1;
        AmplitudeBox.interactable = true;
        PulseWidthBox.interactable = true;
        FrequencyBox.interactable = true;
        IncreaseButton.interactable = true;
        DecreaseButton.interactable = true;
        InitialCalibrationButton.interactable = true;
        SaveCalibrationButton.interactable = true;
        RestoreCalibationButton.interactable = true;
        StimulationToggle.interactable = true;
        textTitle.text = "Pad " + buttonTag + " Selected";
        AmplitudeBox.text = Convert.ToString(StimManager.GetPadsInfo(currentPad).GetAmplitude());
        PulseWidthBox.text = Convert.ToString(StimManager.GetPadsInfo(currentPad).GetPulseWidth());
        FrequencyBox.text = Convert.ToString(StimManager.GetPadsInfo(currentPad).GetFrequency());
        //StimManager.glovePort.Write("velec 11 *special_anodes 1 *name test *elec 1 *pads " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=C, *amp " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=" + StimManager.GetPadsInfo(currentPad).GetAmplitude() + ", *width " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=" + StimManager.GetPadsInfo(currentPad).GetPulseWidth() + ", *selected 1 *sync 0\r\n");
        ConnectDevice.glovePort.Write("velec 11 *special_anodes 1 *name test *elec 1 *pads " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=C, *amp " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=" + StimManager.GetPadsInfo(currentPad).GetAmplitude() + ", *width " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=" + StimManager.GetPadsInfo(currentPad).GetPulseWidth() + ", *selected 1 *sync 0\r\n");
        Thread.Sleep(100);
    }

    public void SetAmplitude()
    {
        float amplitude = (float) Decimal.Round(Convert.ToDecimal(AmplitudeBox.text), 1);
        amplitude =  Mathf.Clamp(amplitude, 0.5f, 7.5f);
        AmplitudeBox.text = Convert.ToString(amplitude);
        StimManager.GetPadsInfo(currentPad).SetAmplitude(amplitude);
        //StimManager.glovePort.Write("velec 11 *special_anodes 1 *name test *elec 1 *pads " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=C, *amp " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=" + StimManager.GetPadsInfo(currentPad).GetAmplitude() + ", *width " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=" + StimManager.GetPadsInfo(currentPad).GetPulseWidth() + ", *selected 1 *sync 0\r\n");
        ConnectDevice.glovePort.Write("velec 11 *special_anodes 1 *name test *elec 1 *pads " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=C, *amp " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=" + StimManager.GetPadsInfo(currentPad).GetAmplitude() + ", *width " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=" + StimManager.GetPadsInfo(currentPad).GetPulseWidth() + ", *selected 1 *sync 0\r\n");
        Thread.Sleep(100);
    }

    public void IncreaseAmplitude()
    {
        float amplitude = (float)Decimal.Round(Convert.ToDecimal(AmplitudeBox.text), 1);
        amplitude = Mathf.Clamp(amplitude + 0.1f, 0.5f, 7.5f);
        AmplitudeBox.text = Convert.ToString(amplitude);
        StimManager.GetPadsInfo(currentPad).SetAmplitude(amplitude);
        //StimManager.glovePort.Write("velec 11 *special_anodes 1 *name test *elec 1 *pads " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=C, *amp " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=" + StimManager.GetPadsInfo(currentPad).GetAmplitude() + ", *width " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=" + StimManager.GetPadsInfo(currentPad).GetPulseWidth() + ", *selected 1 *sync 0\r\n");
        ConnectDevice.glovePort.Write("velec 11 *special_anodes 1 *name test *elec 1 *pads " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=C, *amp " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=" + StimManager.GetPadsInfo(currentPad).GetAmplitude() + ", *width " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=" + StimManager.GetPadsInfo(currentPad).GetPulseWidth() + ", *selected 1 *sync 0\r\n");
        //print("velec 11 *special_anodes 1 *name test *elec 1 *pads " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=C, *amp " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=" + StimManager.GetPadsInfo(currentPad).GetAmplitude() + ", *width " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=" + StimManager.GetPadsInfo(currentPad).GetPulseWidth() + ", *selected 1 *sync 0\r\n");
        EventSystem.current.currentSelectedGameObject.SetActive(false);
        Thread.Sleep(150);
        EventSystem.current.currentSelectedGameObject.SetActive(true);
    }

    public void DecreaseAmplitude()
    {
        float amplitude = (float)Decimal.Round(Convert.ToDecimal(AmplitudeBox.text), 1);
        amplitude = Mathf.Clamp(amplitude - 0.1f, 0.5f, 7.5f);
        AmplitudeBox.text = Convert.ToString(amplitude);
        StimManager.GetPadsInfo(currentPad).SetAmplitude(amplitude);
        //StimManager.glovePort.Write("velec 11 *special_anodes 1 *name test *elec 1 *pads " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=C, *amp " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=" + StimManager.GetPadsInfo(currentPad).GetAmplitude() + ", *width " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=" + StimManager.GetPadsInfo(currentPad).GetPulseWidth() + ", *selected 1 *sync 0\r\n");
        ConnectDevice.glovePort.Write("velec 11 *special_anodes 1 *name test *elec 1 *pads " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=C, *amp " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=" + StimManager.GetPadsInfo(currentPad).GetAmplitude() + ", *width " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=" + StimManager.GetPadsInfo(currentPad).GetPulseWidth() + ", *selected 1 *sync 0\r\n");
        //print("velec 11 *special_anodes 1 *name test *elec 1 *pads " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=C, *amp " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=" + StimManager.GetPadsInfo(currentPad).GetAmplitude() + ", *width " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=" + StimManager.GetPadsInfo(currentPad).GetPulseWidth() + ", *selected 1 *sync 0\r\n");
        EventSystem.current.currentSelectedGameObject.SetActive(false);
        Thread.Sleep(150);
        EventSystem.current.currentSelectedGameObject.SetActive(true);
    }

    public void SetPulseWidth()
    {
        int pulseWidth = (int)Mathf.Round((float)(Convert.ToDouble(PulseWidthBox.text) / 10.0)) * 10;
        pulseWidth = Mathf.Clamp(pulseWidth, 30, 500);
        PulseWidthBox.text = Convert.ToString(pulseWidth);
        StimManager.GetPadsInfo(currentPad).SetPulseWidth(pulseWidth);
        //StimManager.glovePort.Write("velec 11 *special_anodes 1 *name test *elec 1 *pads " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=C, *amp " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=" + StimManager.GetPadsInfo(currentPad).GetAmplitude() + ", *width " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=" + StimManager.GetPadsInfo(currentPad).GetPulseWidth() + ", *selected 1 *sync 0\r\n");
        ConnectDevice.glovePort.Write("velec 11 *special_anodes 1 *name test *elec 1 *pads " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=C, *amp " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=" + StimManager.GetPadsInfo(currentPad).GetAmplitude() + ", *width " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=" + StimManager.GetPadsInfo(currentPad).GetPulseWidth() + ", *selected 1 *sync 0\r\n");
        Thread.Sleep(150);
    }

    public void SetFrequency()
    {
        int frequency = Convert.ToInt32(FrequencyBox.text);
        frequency = Mathf.Clamp(frequency, 1, 200);
        FrequencyBox.text = Convert.ToString(frequency);
        StimManager.GetPadsInfo(currentPad).SetFrequency(frequency);
        //StimManager.glovePort.Write("velec 11 *special_anodes 1 *name test *elec 1 *pads " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=C, *amp " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=" + StimManager.GetPadsInfo(currentPad).GetAmplitude() + ", *width " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=" + StimManager.GetPadsInfo(currentPad).GetPulseWidth() + ", *selected 1 *sync 0\r\n");
        ConnectDevice.glovePort.Write("velec 11 *special_anodes 1 *name test *elec 1 *pads " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=C, *amp " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=" + StimManager.GetPadsInfo(currentPad).GetAmplitude() + ", *width " + StimManager.GetPadsInfo(currentPad).GetRemap() + "=" + StimManager.GetPadsInfo(currentPad).GetPulseWidth() + ", *selected 1 *sync 0\r\n");
        Thread.Sleep(150);
        //StimManager.glovePort.Write("freq " + StimManager.GetPadsInfo(currentPad).GetFrequency() + "\r\n");
        ConnectDevice.glovePort.Write("freq " + StimManager.GetPadsInfo(currentPad).GetFrequency() + "\r\n");
        Thread.Sleep(150);
    }

    public void InitialCalibration()
    {
        float amplitude = StimManager.GetPadsInfo(3).GetAmplitude();
        for (int i = 0; i < 8; i++)
        {
            StimManager.GetPadsInfo(i).SetAmplitude(amplitude);
        }
        amplitude = StimManager.GetPadsInfo(11).GetAmplitude();
        for (int i = 8; i < 21; i++)
        {
            StimManager.GetPadsInfo(i).SetAmplitude(amplitude);
        }
        amplitude = StimManager.GetPadsInfo(22).GetAmplitude();
        for (int i = 21; i < 26; i++)
        {
            StimManager.GetPadsInfo(i).SetAmplitude(amplitude);
        }
        amplitude = StimManager.GetPadsInfo(27).GetAmplitude();
        for (int i = 26; i < 31; i++)
        {
            StimManager.GetPadsInfo(i).SetAmplitude(amplitude);
        }
    }

    public void EnableStimulation() {
        if (EventSystem.current.currentSelectedGameObject.GetComponent<Toggle>().isOn)
            //StimManager.glovePort.Write("stim on\r\n");
            ConnectDevice.glovePort.Write("stim on\r\n");
        else
            //StimManager.glovePort.Write("stim off\r\n");
            ConnectDevice.glovePort.Write("stim off\r\n");
    }

    public void ChangeScene()
    {
        ConnectDevice.glovePort.Write("stim on\r\n");
        Thread.Sleep(150);
        ConnectDevice.glovePort.Write("velec 11 *selected 0\r\n");
        SceneManager.LoadScene(1);
        DontDestroyOnLoad(StimManager.gameObject);
    }

    public void CreateSaveFile()
    {
        //string txtDocumentName = Application.streamingAssetsPath + "/Calibration_Saved_Data/" + "CalibrationSave" + ".txt";

        File.WriteAllText(SaveDocumentPath, "");

        foreach (PadScript.Pad pad in ConnectDevice.Remap2)
        {
            string text = pad.GetAmplitude() + separation + pad.GetPulseWidth() + separation + pad.GetFrequency();
            File.AppendAllText(SaveDocumentPath, text + "\n");
        }

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

            AmplitudeBox.text = Convert.ToString(StimManager.GetPadsInfo(currentPad).GetAmplitude());
            PulseWidthBox.text = Convert.ToString(StimManager.GetPadsInfo(currentPad).GetPulseWidth());
            FrequencyBox.text = Convert.ToString(StimManager.GetPadsInfo(currentPad).GetFrequency());
        }

    }

    public void SetBatteryLevel(string batteryLife)
    {
        int startPos = batteryLife.LastIndexOf(stringStart) + stringStart.Length;
        int length = batteryLife.IndexOf("%") - startPos;
        batteryLife = batteryLife.Substring(startPos, length);
        BatteryLifeText.text = BatteryLifeText.text + " " + batteryLife + " %";

        if (int.Parse(batteryLife) < 30)
        {
            BatteryLifeText.color = Color.red;
        }
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
