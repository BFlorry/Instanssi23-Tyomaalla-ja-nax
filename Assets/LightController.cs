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
    string USER = "BFlorryNot3";

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

        if(Input.GetKeyDown(KeyCode.Q))
        {
            StartCoroutine(FlashLights(FlashType.CLOCKWISE, new Color32(255, 255, 255, 0)));
            
        }

        if(Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(FlashLights(FlashType.COUNTERCLOCKWISE, new Color32(255, 255, 255, 0)));
            
        }
    }

    void SendPacket(){
        Debug.Log($"[{string.Join(",", packet.ToArray())}]");
        if (enableSending && packet != defaultPacket){
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

        byte[] name = Encoding.ASCII.GetBytes(USER);
        
        packet.AddRange(name);

        packet.Add(0); // End name tag

        //Debug.Log($"Packet resetted: [{string.Join(",", packet.ToArray())}]");
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
        COUNTERCLOCKWISE
    }

    

    private IEnumerator FlashLights(FlashType flashType, Color32 color){
        switch (flashType)
        {
            case FlashType.ALL:
                    for (int i = 0; i <= flashUpSpeed; i++)
                    {
                        float brightness = i * 0.1f;
                        for (int u = 0; u < NUM_LIGHTS - 2; u++)
                        {
                            SetPacket(u, Mathf.RoundToInt(color.r * brightness), Mathf.RoundToInt(color.g * brightness), Mathf.RoundToInt(color.b * brightness));
                            
                        }
                        SendPacket();
                        yield return new WaitForSeconds(minWaitTime);
                    }

                    for (int i = flashDownSpeed; i >= 0; i--)
                    {
                        //TODO: laita flashDownWait odotusaika kaikkiin flashdownspeed kohtiin ja laita tilale flashupspeed
                        float brightness = i * 0.01f;
                        for (int u = 0; u < NUM_LIGHTS - 2; u++)
                        {
                            SetPacket(u, Mathf.RoundToInt(color.r * brightness), Mathf.RoundToInt(color.g * brightness), Mathf.RoundToInt(color.b * brightness));
                            
                        }
                        SendPacket();
                        yield return new WaitForSeconds(minWaitTime);
                    }
                    break;
            case FlashType.LEFT:
                    for (int i = 0; i <= flashUpSpeed; i++)
                    {
                        float brightness = i * 0.1f;
                        for (int u = 0; u < NUM_LIGHTS/2 - 1; u++)
                        {
                            SetPacket(u, Mathf.RoundToInt(color.r * brightness), Mathf.RoundToInt(color.g * brightness), Mathf.RoundToInt(color.b * brightness));
                            
                        }
                        SendPacket();
                        yield return new WaitForSeconds(minWaitTime);
                    }

                    for (int i = NUM_LIGHTS/2 - 1; i >= 0; i--)
                    {
                        float brightness = i * 0.01f;
                        for (int u = 0; u < NUM_LIGHTS/2 - 1; u++)
                        {
                            SetPacket(u, Mathf.RoundToInt(color.r * brightness), Mathf.RoundToInt(color.g * brightness), Mathf.RoundToInt(color.b * brightness));
                            
                        }
                        SendPacket();
                        yield return new WaitForSeconds(minWaitTime);
                    }
                    break;
            case FlashType.RIGHT:
                    for (int i = 0; i <= flashUpSpeed; i++)
                    {
                        float brightness = i * 0.1f;
                        for (int u = NUM_LIGHTS/2 - 1; u < NUM_LIGHTS - 2; u++)
                        {
                            SetPacket(u, Mathf.RoundToInt(color.r * brightness), Mathf.RoundToInt(color.g * brightness), Mathf.RoundToInt(color.b * brightness));
                            
                        }
                        SendPacket();
                        yield return new WaitForSeconds(minWaitTime);
                    }

                    for (int i = flashDownSpeed; i >= 0; i--)
                    {
                        float brightness = i * 0.01f;
                        for (int u = NUM_LIGHTS/2 - 1; u < NUM_LIGHTS - 2; u++)
                        {
                            SetPacket(u, Mathf.RoundToInt(color.r * brightness), Mathf.RoundToInt(color.g * brightness), Mathf.RoundToInt(color.b * brightness));
                            
                        }
                        SendPacket();
                        yield return new WaitForSeconds(minWaitTime);
                    }
                    break;
            case FlashType.CLOCKWISE:
                    for (int i = NUM_LIGHTS - 1; i > 0; i--)
                    {
                        StartCoroutine(FlashSingleLight(i, color));
                        yield return new WaitForSeconds(circleWaitTime);
                        
                    }
                    break;
            case FlashType.COUNTERCLOCKWISE:
                    for (int i = 0; i < NUM_LIGHTS; i++)
                    {
                        StartCoroutine(FlashSingleLight(i, color));
                        yield return new WaitForSeconds(circleWaitTime);
                        
                    }
                    break;
            default:
                    yield return new WaitForSeconds(0);
                    break;
        } 
    }
    private IEnumerator FlashSingleLight(int index, Color32 color){
        for (int i = 0; i <= flashUpSpeed; i++)
        {
            float brightness = i * 0.1f;
            SetPacket(index, Mathf.RoundToInt(color.r * brightness), Mathf.RoundToInt(color.g * brightness), Mathf.RoundToInt(color.b * brightness));
                
            SendPacket();
            yield return new WaitForSeconds(minWaitTime);
        }

        for (int i = flashDownSpeed; i >= 0; i--)
        {
            float brightness = i * 0.01f;
            SetPacket(index, Mathf.RoundToInt(color.r * brightness), Mathf.RoundToInt(color.g * brightness), Mathf.RoundToInt(color.b * brightness));
                
            SendPacket();
            yield return new WaitForSeconds(minWaitTime);
        }
    }
}
