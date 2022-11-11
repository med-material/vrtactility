using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading;
using UnityEngine.UI;
using TMPro;
public class ConnectDevice : MonoBehaviour
{
    //public string ComPort;
    public TMP_InputField ComPortBox;
    public TMP_Text FailConnectionText, ConnectingText;
    public GameObject ConnectDevicePanel;
    //public GameObject LoadingCircle;
    public UiManager ButtonManager;
    public static SerialPort glovePort;
    //[HideInInspector]
    public static PadScript.Pad[] Remap2;
    public static string batteryLife;
    public static string connectedMessage;
    private int[] Remap;
    //private float time;
    //private int index = 0;
    //private bool done = false;

    private void Awake()
    {
        
    }
    // Start is called before the first frame update
    void Start()
    {
        ComPortBox.characterValidation = TMP_InputField.CharacterValidation.Integer;
        glovePort = new SerialPort();
        //glovePort.PortName = "COM5";
        //glovePort.BaudRate = 115200;
        //glovePort.DataBits = 8;
        //glovePort.Parity = Parity.None;
        //glovePort.StopBits = StopBits.One;
        //glovePort.Handshake = Handshake.None;

        Remap = new int[32] { 30, 27, 29, 28, 25, 31, 32, 26, 17, 18, 20, 1, 2, 22, 19, 3, 23, 21, 24, 4, 5, 8, 9, 6, 7, 10, 13, 14, 11, 12, 15, 16 };
        Remap2 = new PadScript.Pad[32];

        for (int i = 0; i < Remap2.Length; i++) {
            Remap2[i] = new PadScript.Pad(i + 1, Remap[i], 0.5f, 100, 50);
            //print("Remap: " + Remap2[i].Remap + "PulseWidth: " + Remap2[i].PulseWidth);
        }
        

    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.anyKeyDown)
        //{
        //    glovePort.Write("stim off\r\n");
        //}
        //time = time + Time.deltaTime;

        //if ((time < 1) && (!done))
        //{
        //    glovePort.Write("velec 11 *special_anodes 1 *name test *elec 1 *pads " + Remap[index] + "=C, *amp " + Remap[index] + "=3, *width " + Remap[index] + "=200, *selected 1 *sync 0");
        //    done = true;
        //}
        //else
        //{
        //    if (time > 1)
        //    {
        //        if (index < 9)
        //            index++;
        //        else
        //            index = 0;
        //        glovePort.Write("stim off\r\n");
        //        time = 0;

        //    }

        //}

    }

    public PadScript.Pad GetPadsInfo(int i) {
        return Remap2[i];
    }

    public void MakeConnection()
    {
        StopAllCoroutines();
        StartCoroutine(Connect());
    }

    IEnumerator Connect()
    {
        ConnectingText.alpha = 1;
        FailConnectionText.alpha = 0;
        //string[] ports;

        yield return null;

        try
        {
            glovePort.PortName = "COM" + ComPortBox.text;
            print(glovePort.PortName);
            glovePort.BaudRate = 115200;
            glovePort.DataBits = 8;
            glovePort.Parity = Parity.None;
            glovePort.StopBits = StopBits.One;
            glovePort.Handshake = Handshake.None;
            print("Hi");
            glovePort.Open();
            print("Hello");
            Thread.Sleep(200);
            glovePort.Write("iam TACTILITY\r\n");
            connectedMessage = glovePort.ReadLine();
            print(connectedMessage);

            Thread.Sleep(200);

            glovePort.Write("elec 1 *pads_qty 32\r\n");
            print(glovePort.ReadLine());

            Thread.Sleep(200);

            glovePort.Write("battery ?\r\n");
            batteryLife = glovePort.ReadLine();
            print(batteryLife);

            Thread.Sleep(200);

            glovePort.Write("freq 50\r\n");
            print(glovePort.ReadLine());

            Thread.Sleep(200);

            ButtonManager.SetBatteryLevel(batteryLife);

            if (connectedMessage == "Re:[] new connection" || connectedMessage == "Re:[] re-connection")
            {
                ConnectDevicePanel.SetActive(false);
            }
            else
            {
                ConnectingText.alpha = 0;
            }
        }
        catch
        {
            //glovePort.DiscardInBuffer();
            //glovePort.DiscardOutBuffer();
            glovePort.Close();
            Thread.Sleep(200);

            //ports = SerialPort.GetPortNames();

            //// Display each port name to the console.
            //foreach (string port in ports)
            //{
            //    glovePort.
            //}
            ConnectingText.alpha = 0;
            FailConnectionText.alpha = 1;
        }
        yield return null;

        //string[] ports = SerialPort.GetPortNames();

        //print("The following serial ports were found:");

        //// Display each port name to the console.
        //foreach (string port in ports)
        //{
        //    print(port);
        //}

    }


    //public void WriteComPort()
    //{
    //    FailConnectionText.alpha = 0;
    //}

    //public PadScript.Pad GetPads()
    //{
    //    return Remap2;
    //}
}
