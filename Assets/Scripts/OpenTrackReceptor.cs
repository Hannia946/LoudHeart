using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
public class OpenTrackReceptor : ProveedorHeadTracking
{
    public float rawYaw { get; private set; }
    public float rawPitch { get; private set; }

    private UdpClient udpClient;
    private Thread receiveThread;
    private bool isRunning;
    private int listenPort = 5555; // Puerto de OpenTrack
    
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
        try
        {
            // Permite reutilizar el puerto sin que Windows lance la SocketException
            udpClient = new UdpClient();
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, listenPort));

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
            Debug.LogWarning("Estado del puerto UDP: " + e.Message);
        }
    }



    void OnApplicationQuit()
    {
        isRunning = false;
        if (receiveThread != null) receiveThread.Abort();
        if (udpClient != null) udpClient.Close();
    }

    public override float ObtenerYaw() { return rawYaw; }
    public override float ObtenerPitch() { return rawPitch; }

}
