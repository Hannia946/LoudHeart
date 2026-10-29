using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
public class OpenTrackReceptor : ProveedorHeadTracking
{
    public float rawYaw { get; private set; }
    public float rawPitch { get; private set; }

    [SerializeField] private int listenPort = 5555;

    private UdpClient udpClient;
    private Thread receiveThread;
    private volatile bool isRunning;
   
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        isRunning = true;
        receiveThread = new Thread(ReceiveData) { IsBackground = true};
        receiveThread.Start();

    }

    private void OnDisable()
    {
        Detener();
    }

    void OnApplicationQuit() { Detener(); }

    private void ReceiveData()
    {
        try
        {
            var client = new UdpClient();
            udpClient = client;
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.Bind(new IPEndPoint(IPAddress.Any, listenPort));

            IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, listenPort);

            while (isRunning)
            {
                byte[] data = udpClient.Receive(ref anyIP);
                if (data.Length >= 48)
                {
                    rawYaw = (float)System.BitConverter.ToDouble(data, 24);
                    rawPitch = (float)System.BitConverter.ToDouble(data, 32);
                }
            }
        }
        catch (System.Exception e)
        {
            if (isRunning) Debug.LogWarning("Estado del puerto UDP: " + e.Message);
        }
    }



    private void Detener()
    {
        isRunning = false;
        if (udpClient != null) { udpClient.Close(); udpClient = null; }
        if (receiveThread != null && receiveThread.IsAlive) receiveThread.Join(200);
        receiveThread = null;
    }

    public override float ObtenerYaw() { return rawYaw; }
    public override float ObtenerPitch() { return rawPitch; }

}
