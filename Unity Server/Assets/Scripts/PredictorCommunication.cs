using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

using System.Runtime.InteropServices;

public class PredictorCommunication : MonoBehaviour
{
    [DllImport("kernel32.dll")]
    private static extern ulong GetTickCount64();
    private const string PredictorIP = "192.168.137.1";
    private const int RequestPort = 23002;
    private const int ResponsePort = 23004;
    private const int ClockSyncPort = 23006;
    private const int ClockSyncResponsePort = 23007;

    private UdpClient requestClient;
    private UdpClient responseClient;
    private UdpClient clockSyncClient;
    private IPEndPoint requestEndPoint;
    private IPEndPoint responseEndPoint;
    private IPEndPoint clockSyncEndPoint;

    private double clockOffset = 0.0;
    public float predictionTimeOffset = 0.05f;
    private Vector3 predictedPosition;
    private Quaternion predictedRotation;
    private bool newDataAvailable = false;
    private Thread receiveThread;
    private bool isRunning = true;
    private Stopwatch systemClock;

    private void Start()
    {
        systemClock = Stopwatch.StartNew();
        InitializeUDP();
        SynchronizeClock();
        InitializeReceiveThread();
    }

    private void InitializeUDP()
    {
        requestClient = new UdpClient();
        requestEndPoint = new IPEndPoint(IPAddress.Parse(PredictorIP), RequestPort);
        
        responseClient = new UdpClient(ResponsePort);
        responseEndPoint = new IPEndPoint(IPAddress.Any, ResponsePort);
        
        clockSyncClient = new UdpClient(ClockSyncResponsePort);
        clockSyncEndPoint = new IPEndPoint(IPAddress.Parse(PredictorIP), ClockSyncPort);
    }

    private void InitializeReceiveThread()
    {
        receiveThread = new Thread(ReceiveLoop);
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void SynchronizeClock()
    {
        // byte[] syncResponse = new byte[16];
        // long currentTime = systemClock.ElapsedTicks / (TimeSpan.TicksPerMillisecond / 1000); // Convert to microseconds
        
        // BitConverter.GetBytes(currentTime).CopyTo(syncResponse, 0);
        // clockSyncClient.Send(syncResponse, syncResponse.Length, clockSyncEndPoint);

        // byte[] receivedData = clockSyncClient.Receive(ref clockSyncEndPoint);
        // long serverTime = BitConverter.ToInt64(receivedData, 0);
        // clockOffset = (double)(serverTime - currentTime) / 1_000_000.0; // Convert to seconds
    }

    private void Update()
    {
        SendPredictionRequest();
        UpdateCameraRig();
    }

    private void SendPredictionRequest()
    {
        double currentTime = (double)systemClock.ElapsedMilliseconds / 1000.0 + clockOffset;
        ulong uptimeMilliseconds = GetTickCount64();
        double uptimeSeconds = uptimeMilliseconds / 1000.0;
        currentTime = uptimeSeconds;
        double requestTime = currentTime + predictionTimeOffset;
        
        byte[] requestData = BitConverter.GetBytes(requestTime);
        requestClient.Send(requestData, requestData.Length, requestEndPoint);
    }

    private void ReceiveLoop()
    {
        while (isRunning)
        {
            try
            {
                byte[] receivedData = responseClient.Receive(ref responseEndPoint);
                ProcessReceivedData(receivedData);
            }
            catch (SocketException e)
            {
                Debug.LogError($"SocketException: {e.Message}");
            }
        }
    }

    private void ProcessReceivedData(byte[] data)
    {
        if (data.Length != 36)
        {
            Debug.LogError("Received data has unexpected length");
            return;
        }

        double timestamp = BitConverter.ToDouble(data, 0);
        Vector3 position = new Vector3(
            BitConverter.ToSingle(data, 8),
            BitConverter.ToSingle(data, 12),
            -BitConverter.ToSingle(data, 16)
        );
        
        Quaternion orientation = new Quaternion(
            BitConverter.ToSingle(data, 24),
            BitConverter.ToSingle(data, 28),
            BitConverter.ToSingle(data, 32),
            BitConverter.ToSingle(data, 20)
        );

        predictedPosition = position;
        predictedRotation = ConvertToUnityRotation(orientation);
        newDataAvailable = true;
    }

    private Quaternion ConvertToUnityRotation(Quaternion input)
    {
        return new Quaternion(-input.x, -input.y, input.z, input.w);
    }

    private void UpdateCameraRig()
    {
        if (newDataAvailable)
        {
            transform.localPosition = predictedPosition;
            transform.localRotation = predictedRotation;
            newDataAvailable = false;
        }
    }

    private void OnDestroy()
    {
        isRunning = false;
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Join();
        }
        
        requestClient?.Close();
        responseClient?.Close();
        clockSyncClient?.Close();
    }
}