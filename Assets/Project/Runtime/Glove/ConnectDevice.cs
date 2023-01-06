using System;
using UnityEngine;
using System.IO.Ports;
using System.Threading;
using TMPro;
public class ConnectDevice : MonoBehaviour
{
    public TMP_InputField ComPortBox;
    public TMP_Text FailConnectionText, ConnectingText;
    public GameObject ConnectDevicePanel;
    public UiManager ButtonManager;
    public static SerialPort glovePort;
    public static PadScript.Pad[] Remap2;
    public static string batteryLife;
    public static string connectedMessage;
    private int[] Remap;

    [SerializeField] private CalibrationScriptableObject cd;

    public static SerialController gloveSerialController;

    private void Awake()
    {
        Remap = new int[32] { 30, 27, 29, 28, 25, 31, 32, 26, 17, 18, 20, 1, 2, 22, 19, 3, 23, 21, 24, 4, 5, 8, 9, 6, 7, 10, 13, 14, 11, 12, 15, 16 };
        Remap2 = new PadScript.Pad[32];

        for (int i = 0; i < Remap2.Length; i++) {
            Remap2[i] = new PadScript.Pad(i + 1, Remap[i], 0.5f, 100, 50);
            //print("Remap: " + Remap2[i].Remap + "PulseWidth: " + Remap2[i].PulseWidth);
        }
    }
    
    void Start()
    {
        ComPortBox.characterValidation = TMP_InputField.CharacterValidation.Integer;
    }

    public static PadScript.Pad GetPadsInfo(int i) {
        return Remap2[i];
    }

    public void MakeConnection()
    {
        // NOTE: previous coroutine structure was removed from here
        Connect();
    }

    private void Connect()
    {
        ConnectingText.alpha = 1;
        FailConnectionText.alpha = 0;

        try
        {
            print("COM" + ComPortBox.text);
            print("Hi");
            
            // Define SerialController and make sure it persist between scene loads
            var serialControllerGameObject = GameObject.Find("SerialController");
            gloveSerialController = serialControllerGameObject.GetComponent<SerialController>();
            gloveSerialController.portName = "COM" + ComPortBox.text;
            gloveSerialController.enabled = true;
            DontDestroyOnLoad(serialControllerGameObject);
            
            // SerialController messages are buffered and thus we can spam it without issues
            print("Hello");
            Thread.Sleep(200);
            gloveSerialController.SendSerialMessage("iam TACTILITY\r");
            gloveSerialController.SendSerialMessage("elec 1 *pads_qty 32\r");
            gloveSerialController.SendSerialMessage("battery ?\r");
            gloveSerialController.SendSerialMessage("freq 50\r");

            // === The results are printed in a separate GameObject ===
            
            cd.port = ComPortBox.text;
        }
        catch(Exception e)
        {
            Thread.Sleep(200);  // This is probably not needed anymore...

            ConnectingText.alpha = 0;
            FailConnectionText.alpha = 1;
        }
    }
}
