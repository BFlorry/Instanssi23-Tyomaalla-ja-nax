using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class LightController : MonoBehaviour
{

    // DMX-valojen määrä
    int NUM_LIGHTS = 24;

    // Valopalvelimen IP-osoite
    string HOSTNAME = "valot.instanssi";

    int PORTNUM = 9909;

    // Paketin lähettäjä
    string USER = "BFlor";

    Socket sock;

    IPEndPoint endPoint;

    List<byte> packet;

    [SerializeField]
    private FakeLightController fakeLightController;

    [SerializeField]
    private bool enableSending = false;

    List<byte> defaultPacket;

    [SerializeField]
    private float minWaitTime = 0.025f;

    [SerializeField]
    private float circleWaitTime = 0.050f;

    [SerializeField]
    private int flashUpSpeed = 10;

    [SerializeField]
    private int flashDownSpeed = 25;

    float angle = 0;

    [SerializeField]
    private float flashDownWait = 0.05f;

    private int curLight = 0;

    private int lightNumber = 0;

    [SerializeField]
    private DemoController demoController;

    private int coneCount;

    // Start is called before the first frame update
    void Start()
    {
        sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
            ProtocolType.Udp);
        IPHostEntry hostEntry = Dns.GetHostEntry(HOSTNAME);
        IPAddress serverAddr = hostEntry.AddressList[0];

        endPoint = new IPEndPoint(serverAddr, PORTNUM);
        ResetPacket();
        defaultPacket = packet;
    }

    

    // Update is called once per frame
    void Update()
    {
        coneCount = demoController.GetConeCount();
        
        if(Input.GetKeyDown(KeyCode.Space))
        {
            int intensr = Intensity(angle);
            int intensg = Intensity(angle + 90);
            int intensb = Intensity(angle + 180);
            
            for (int i = 0; i < NUM_LIGHTS; i++)
            {
                //SetPacket((byte)i, 0, 0, 255);
                SetPacket((byte)i, (byte)intensr, (byte)intensg, (byte)intensb);
            }


            Debug.Log($"[{string.Join(",", packet.ToArray())}]");
            
            
            
            angle += 0.01f;
            angle = angle % 360;

            SendPacket();
            
        }
        if(Input.GetKeyDown(KeyCode.Delete))
        {
            StartCoroutine(FlashLights(FlashType.ALL, new Color32(0, 0, 0, 0)));
            
        }

        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            StartCoroutine(FlashLights(FlashType.ALL, new Color32(255, 255, 255, 0)));
            
        }

        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            StartCoroutine(FlashLights(FlashType.LEFT, new Color32(255, 255, 255, 0)));
            
        }

        if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            StartCoroutine(FlashLights(FlashType.RIGHT, new Color32(255, 255, 255, 0)));
            
        }

        if(Input.GetKeyDown(KeyCode.Keypad6))
        {
            StartCoroutine(FlashLights(FlashType.ALTRIGHT, new Color32(255, 255, 255, 0)));
            
        }

        if(Input.GetKeyDown(KeyCode.Q))
        {
            StartCoroutine(FlashLights(FlashType.CLOCKWISE, new Color32(255, 255, 255, 0)));
            
        }

        if(Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(FlashLights(FlashType.COUNTERCLOCKWISE, new Color32(255, 255, 255, 0)));
            
        }

        if(Input.GetKeyDown(KeyCode.KeypadEnter)){
            StartCoroutine(FlashSingleLight(curLight, new Color32(255, 255, 255, 0)));
            curLight = curLight + 1;
            if(curLight > NUM_LIGHTS) curLight = 0;
        }
    }

    void SendPacket(){
        Debug.Log($"[{string.Join(",", packet.ToArray())}]");
        if (enableSending/* && packet != defaultPacket*/){
            sock.SendTo(packet.ToArray(), endPoint);
        }
        ResetPacket();
    }

    int Intensity(float angle){
    // Siirtää käyrän Y-koordinaatiston välille 0-1 ja kertoo väriavaruuden maksimilla
    int intensity = (int)(((Mathf.Sin(angle) * 0.5 + 0.5)* 255)* 0.5) % 256;
    return intensity;
    }

    private void ResetPacket()
    {
        packet = new List<byte>();
        packet.Add(1); // Spex version is always 1
        packet.Add(0); // Begin name tag

        byte[] name = Encoding.ASCII.GetBytes(USER + lightNumber);
        lightNumber = lightNumber + 1;
        packet.AddRange(name);

        packet.Add(0); // End name tag

        //Debug.Log($"Packet resetted: [{string.Join(",", packet.ToArray())}]");

        if(lightNumber > 999) lightNumber = 0;
    }

    void SetPacket(int i, int r, int g, int b){
        byte[] light = {
            1, // Tehosteen tyyppi on yksi eli valo
            (byte)i, // Valon indeksi
            0, // Laajennustavu. Aina nolla. Älä välitä tästä
            (byte)r, // Punaisuus
            (byte)g, // Vihreys
            (byte)b  // Sinisyys
        };
        packet.AddRange(light);
        if (fakeLightController != null){
            fakeLightController.ChangeLight(i, new Color32((byte)r, (byte)g, (byte)b, 255));
        }
    }

    enum FlashType{
        ALL,
        LEFT,
        RIGHT,
        CLOCKWISE,
        COUNTERCLOCKWISE,
        ALTRIGHT,
        PINGPONG
    }

    

    private IEnumerator FlashLights(FlashType flashType, Color32 color){
        switch (flashType)
        {
            case FlashType.ALL:
                    demoController.PulseSizeAll(3);
                    //StopAllCoroutines();
                    for (int i = 0; i <= flashUpSpeed/2; i++)
                    {
                        float brightness = i * 0.1f;
                        for (int u = 0; u < NUM_LIGHTS - 2; u++)
                        {
                            SetPacket(u, Mathf.RoundToInt(color.r * brightness), Mathf.RoundToInt(color.g * brightness), Mathf.RoundToInt(color.b * brightness));
                            
                        }
                        SendPacket();
                        yield return new WaitForSeconds(minWaitTime);
                    }

                    for (int i = flashUpSpeed/2; i >= 0; i--)
                    {
                        //TODO: laita flashDownWait odotusaika kaikkiin flashdownspeed kohtiin ja laita tilale flashupspeed
                        float brightness = i * 0.01f;
                        for (int u = 0; u < NUM_LIGHTS - 2; u++)
                        {
                            SetPacket(u, Mathf.RoundToInt(color.r * brightness), Mathf.RoundToInt(color.g * brightness), Mathf.RoundToInt(color.b * brightness));
                            
                        }
                        SendPacket();
                        yield return new WaitForSeconds(flashDownWait);
                    }
                    break;
            case FlashType.LEFT:
                    demoController.PulseSizeHalf(3, false);
                    for (int i = 0; i <= flashUpSpeed; i++)
                    {
                        float brightness = i * 0.1f;
                        for (int u = 0; u < NUM_LIGHTS - 2; u++)
                        {
                            if(u > NUM_LIGHTS/2 - 1) SetPacket(u, Mathf.RoundToInt(0), Mathf.RoundToInt(0), Mathf.RoundToInt(0));
                            else SetPacket(u, Mathf.RoundToInt(color.r * brightness), Mathf.RoundToInt(color.g * brightness), Mathf.RoundToInt(color.b * brightness));
                            
                        }
                        SendPacket();
                        yield return new WaitForSeconds(minWaitTime);
                    }

                    for (int i = NUM_LIGHTS - 2; i >= 0; i--)
                    {
                        float brightness = i * 0.01f;
                        for (int u = 0; u < NUM_LIGHTS/2 - 1; u++)
                        {
                            if(u > NUM_LIGHTS/2 - 1) SetPacket(u, Mathf.RoundToInt(0), Mathf.RoundToInt(0), Mathf.RoundToInt(0));
                            else SetPacket(u, Mathf.RoundToInt(color.r * brightness), Mathf.RoundToInt(color.g * brightness), Mathf.RoundToInt(color.b * brightness));
                            
                        }
                        SendPacket();
                        yield return new WaitForSeconds(minWaitTime);
                    }
                    break;
            case FlashType.RIGHT:
                    demoController.PulseSizeHalf(3, true);
                    for (int i = 0; i <= flashUpSpeed; i++)
                    {
                        float brightness = i * 0.1f;
                        for (int u = 0; u < NUM_LIGHTS - 2; u++)
                        {
                            if(u < NUM_LIGHTS/2 - 1) SetPacket(u, Mathf.RoundToInt(0), Mathf.RoundToInt(0), Mathf.RoundToInt(0));
                            else SetPacket(u, Mathf.RoundToInt(color.r * brightness), Mathf.RoundToInt(color.g * brightness), Mathf.RoundToInt(color.b * brightness));
                            
                        }
                        SendPacket();
                        yield return new WaitForSeconds(minWaitTime);
                    }

                    for (int i = flashUpSpeed; i >= 0; i--)
                    {
                        float brightness = i * 0.01f;
                        for (int u = 0; u < NUM_LIGHTS - 2; u++)
                        {
                            if(u < NUM_LIGHTS/2 - 1) SetPacket(u, Mathf.RoundToInt(0), Mathf.RoundToInt(0), Mathf.RoundToInt(0));
                            else SetPacket(u, Mathf.RoundToInt(color.r * brightness), Mathf.RoundToInt(color.g * brightness), Mathf.RoundToInt(color.b * brightness));
                            
                        }
                        SendPacket();
                        yield return new WaitForSeconds(flashDownWait);
                    }
                    break;
            case FlashType.CLOCKWISE:
                    for (int i = NUM_LIGHTS - 1; i > 0; i--)
                    {
                        StartCoroutine(FlashSingleLight(i, color));
                        demoController.PulseSize(new int[]{i % coneCount}, 3);
                        yield return new WaitForSeconds(circleWaitTime);
                        
                    }
                    break;
            case FlashType.COUNTERCLOCKWISE:
                    for (int i = 0; i < NUM_LIGHTS; i++)
                    {
                        StartCoroutine(FlashSingleLight(i, color));
                        demoController.PulseSize(new int[]{i % coneCount}, 3);
                        yield return new WaitForSeconds(circleWaitTime);
                        
                    }
                    break;
            case FlashType.ALTRIGHT:
                    for (int i = 0; i <= flashUpSpeed; i++)
                    {
                        float brightness = i * 0.1f;
                        for (int u = 0; u < NUM_LIGHTS - 2; u++)
                        {
                            if(u > 10) SetPacket(u, Mathf.RoundToInt(color.r * brightness), Mathf.RoundToInt(color.g * brightness), Mathf.RoundToInt(color.b * brightness));
                            else SetPacket(u, Mathf.RoundToInt(0), Mathf.RoundToInt(0), Mathf.RoundToInt(0));
                            
                        }
                        SendPacket();
                        yield return new WaitForSeconds(minWaitTime);
                    }

                    for (int i = flashUpSpeed; i >= 0; i--)
                    {
                        //TODO: laita flashDownWait odotusaika kaikkiin flashdownspeed kohtiin ja laita tilale flashupspeed
                        float brightness = i * 0.01f;
                        for (int u = 0; u < NUM_LIGHTS - 2; u++)
                        {
                            if(u > 10) SetPacket(u, Mathf.RoundToInt(color.r * brightness), Mathf.RoundToInt(color.g * brightness), Mathf.RoundToInt(color.b * brightness));
                            else SetPacket(u, Mathf.RoundToInt(0), Mathf.RoundToInt(0), Mathf.RoundToInt(0));
                            
                        }
                        SendPacket();
                        yield return new WaitForSeconds(flashDownWait);
                    }
                    break;
            /*case FlashType.PINGPONG:
                    SetPacket(u, Mathf.RoundToInt(color.r * brightness), Mathf.RoundToInt(color.g * brightness), Mathf.RoundToInt(color.b * brightness));
                            
                        
                    SendPacket();
                    yield return new WaitForSeconds(minWaitTime);
                    break;*/
            default:
                    yield return new WaitForSeconds(0);
                    break;
        } 
    }
    private IEnumerator FlashSingleLight(int index, Color32 color){
        for (int i = 0; i <= flashUpSpeed; i++)
        {
            float brightness = i * 0.1f;
            for (int u = 0; u < NUM_LIGHTS; u++)
            {
                if(u == index) SetPacket(index, Mathf.RoundToInt(color.r * brightness), Mathf.RoundToInt(color.g * brightness), Mathf.RoundToInt(color.b * brightness));
                else SetLightOff(u);
            }
            SendPacket();
            demoController.PulseSize(new int[]{i % coneCount}, 3);
            yield return new WaitForSeconds(minWaitTime);
        }

        for (int i = flashUpSpeed; i >= 0; i--)
        {
            float brightness = i * 0.01f;
            for (int u = 0; u < NUM_LIGHTS; u++)
            {
                if(u == index) SetPacket(index, Mathf.RoundToInt(color.r * brightness), Mathf.RoundToInt(color.g * brightness), Mathf.RoundToInt(color.b * brightness));
                else SetLightOff(u);
            }
            SendPacket();
            yield return new WaitForSeconds(flashDownWait);
        }
    }

    private IEnumerator SimpleFlashSingle(int index, Color32 color){
        SetPacket(index, color.r, color.g, color.b);
        SendPacket();
        yield return new WaitForSeconds(flashDownWait);
        SetPacket(index, 0, 0, 0);
        SendPacket();
    }

    public void ClearAllLights()
        {
            StartCoroutine(FlashLights(FlashType.ALL, new Color32(0, 0, 0, 0)));
            
        }

    public void FlashAllLights()
        {
            StartCoroutine(FlashLights(FlashType.ALL, new Color32(255, 255, 255, 0)));
            
        }

    public void FlashLeftLights()
        {
            StartCoroutine(FlashLights(FlashType.LEFT, new Color32(255, 255, 255, 0)));
            
        }

    public void FlashRightLights()
        {
            StartCoroutine(FlashLights(FlashType.RIGHT, new Color32(255, 255, 255, 0)));
            
        }

    public void FlashAllClockwise()
        {
            StartCoroutine(FlashLights(FlashType.CLOCKWISE, new Color32(255, 255, 255, 0)));
            
        }

    public void FlashAllCounterclockwise()
        {
            StartCoroutine(FlashLights(FlashType.COUNTERCLOCKWISE, new Color32(255, 255, 255, 0)));
            
        }

    public void SimpleFlashSingle(int index)
        {
            //StartCoroutine(SimpleFlashSingle(index, new Color32(255, 255, 255, 0)));
            StartCoroutine(FlashSingleLight(index, new Color32(255, 255, 255, 0)));
        }

    private void SetLightOff(int index){
        SetPacket(index, Mathf.RoundToInt(0), Mathf.RoundToInt(0), Mathf.RoundToInt(0));
    }
}
