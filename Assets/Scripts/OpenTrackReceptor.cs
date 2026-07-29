using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
public class OpenTrackReceptor : MonoBehaviour
{
    public float rawYaw { get; private set; }
    public float rawPitch { get; private set; }

    private UdpClient udpClient;
    private Thread receiveThread;
    private bool isRunning;
    private int listenPort = 5050; // Puerto de OpenTrack
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isRunning = true;
        receiveThread = new Thread(ReceiveData);
        receiveThread.IsBackground = true;
        receiveThread.Start();

    }

    private void ReceiveData()
    {
        udpClient = new UdpClient(listenPort);
        IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, listenPort);

        while (isRunning)
        {
            try
            {
                byte[] data = udpClient.Receive(ref anyIP);
                if (data.Length >= 48)
                {
                    // OpenTrack envía double de 8 bytes. Yaw = byte 24, Pitch = byte 32.
                    rawYaw = (float)System.BitConverter.ToDouble(data, 24);
                    rawPitch = (float)System.BitConverter.ToDouble(data, 32);
                }
            }
            catch { /* Ignorar errores de red temporales */ }
        }
    }

    void OnApplicationQuit()
    {
        isRunning = false;
        if (receiveThread != null) receiveThread.Abort();
        if (udpClient != null) udpClient.Close();
    }

}
