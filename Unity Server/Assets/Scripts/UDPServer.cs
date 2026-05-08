using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;

public class UDPServer : MonoBehaviour
{
    public OVRCameraRig cameraRig;
    public string clientIP = "192.168.137.237";
    public int receivePort = 50000; // Port for receiving head tracking data
    public int sendPort = 50001; // Port for sending stereo images
    private UdpClient receiveClient;
    private UdpClient sendClient;
    private IPEndPoint receiveEndPoint;
    private IPEndPoint sendEndPoint;
    private Thread receiveThread;
    private bool isRunning = true;
    private ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();
    private uint lastProcessedSequenceNumber = 0;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct HeadTrackingData
    {
        public long timestamp;
        public float posX, posY, posZ;
        public float rotQx, rotQy, rotQz, rotQw;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct UDPPacketHeader
    {
        public uint sequenceNumber;
        public uint timestamp;
        public uint payloadSize;
    }

    private void Start()
    {
        receiveClient = new UdpClient(receivePort);
        receiveEndPoint = new IPEndPoint(IPAddress.Any, receivePort);
        
        sendClient = new UdpClient();
        sendEndPoint = new IPEndPoint(IPAddress.Parse(clientIP), 53770); // Client port for receiving stereo images

        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.Start();

        Debug.Log($"UDP Server started. Listening on port {receivePort}, sending to {clientIP}:{sendEndPoint.Port}");
    }

    private void OnDisable()
    {
        isRunning = false;
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Join();
        }
        if (receiveClient != null)
        {
            receiveClient.Close();
        }
        if (sendClient != null)
        {
            sendClient.Close();
        }
        Debug.Log("UDP Server stopped.");
    }

    private void Update()
    {
        while (mainThreadActions.TryDequeue(out Action action))
        {
            action.Invoke();
        }
    }

    private void ReceiveData()
    {
        while (isRunning)
        {
            try
            {
                byte[] data = receiveClient.Receive(ref receiveEndPoint);
                
                // Check if this is a connection verification message
                if (data.Length == 7 && Encoding.ASCII.GetString(data) == "CONNECT")
                {
                    HandleConnectionVerification(data, receiveEndPoint);
                }
                else
                {
                    ProcessHeadTrackingData(data);
                }
            }
            catch (SocketException e)
            {
                if (!isRunning) // Socket was closed intentionally
                {
                    break;
                }
                Debug.LogError($"Socket error: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error receiving UDP data: {e.Message}");
            }
        }
    }

    private void HandleConnectionVerification(byte[] data, IPEndPoint clientEndPoint)
    {
        string message = Encoding.ASCII.GetString(data);
        if (message == "CONNECT")
        {
            byte[] response = Encoding.ASCII.GetBytes("CONNECTED");
            sendClient.Send(response, response.Length, clientEndPoint);
            Debug.Log($"Sent CONNECTED response to {clientEndPoint}");
        }
    }

    private void ProcessHeadTrackingData(byte[] data)
    {
        int headerSize = Marshal.SizeOf<UDPPacketHeader>();
        int dataSize = Marshal.SizeOf<HeadTrackingData>();

        if (data.Length < headerSize + dataSize)
        {
            Debug.LogError("Received data is too short");
            return;
        }

        UDPPacketHeader header = ByteArrayToStructure<UDPPacketHeader>(data);
        HeadTrackingData htData = ByteArrayToStructure<HeadTrackingData>(data, headerSize);

        if (header.sequenceNumber > lastProcessedSequenceNumber)
        {
            lastProcessedSequenceNumber = header.sequenceNumber;
            mainThreadActions.Enqueue(() =>
            {
                cameraRig.transform.position = new Vector3(htData.posX, htData.posY, htData.posZ);
                cameraRig.transform.rotation = new Quaternion(htData.rotQx, htData.rotQy, htData.rotQz, htData.rotQw);
                CaptureAndSendStereoImage();
            });
        }
        else
        {
            Debug.Log($"Skipped out-of-order packet. Received: {header.sequenceNumber}, Last processed: {lastProcessedSequenceNumber}");
        }
    }

    private void CaptureAndSendStereoImage()
    {   
        //RenderTexture leftEyeTexture = new RenderTexture(4096, 4096, 24);
        //RenderTexture leftEyeTexture = new RenderTexture(2048, 2048, 24);
        
        RenderTexture leftEyeTexture = new RenderTexture(1024, 1024, 24);
        RenderTexture rightEyeTexture = new RenderTexture(1024, 1024, 24);

        cameraRig.leftEyeAnchor.GetComponent<Camera>().targetTexture = leftEyeTexture;
        cameraRig.rightEyeAnchor.GetComponent<Camera>().targetTexture = rightEyeTexture;

        cameraRig.leftEyeAnchor.GetComponent<Camera>().Render();
        cameraRig.rightEyeAnchor.GetComponent<Camera>().Render();

        Texture2D stereoImage = CombineTextures(leftEyeTexture, rightEyeTexture);

        byte[] imageData = stereoImage.EncodeToJPG(75);
        SendStereoImage(imageData);

        RenderTexture.ReleaseTemporary(leftEyeTexture);
        RenderTexture.ReleaseTemporary(rightEyeTexture);
        Destroy(stereoImage);
    }

    private Texture2D CombineTextures(RenderTexture left, RenderTexture right)
    {
        Texture2D combined = new Texture2D(left.width * 2, left.height);
        RenderTexture.active = left;
        combined.ReadPixels(new Rect(0, 0, left.width, left.height), 0, 0);
        RenderTexture.active = right;
        combined.ReadPixels(new Rect(0, 0, right.width, right.height), left.width, 0);
        combined.Apply();
        return combined;
    }

    private void SendStereoImage(byte[] imageData)
    {
        try
        {
            UDPPacketHeader header = new UDPPacketHeader
            {
                sequenceNumber = lastProcessedSequenceNumber,
                timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                payloadSize = (uint)imageData.Length
            };

            byte[] headerBytes = StructureToByteArray(header);
            byte[] packet = new byte[headerBytes.Length + sizeof(int) * 2 + imageData.Length];
            Buffer.BlockCopy(headerBytes, 0, packet, 0, headerBytes.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(1024), 0, packet, headerBytes.Length, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(1024), 0, packet, headerBytes.Length + sizeof(int), sizeof(int));
            Buffer.BlockCopy(imageData, 0, packet, headerBytes.Length + sizeof(int) * 2, imageData.Length);

            sendClient.Send(packet, packet.Length, sendEndPoint);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error sending stereo image: {e.Message}");
        }
    }

    private static T ByteArrayToStructure<T>(byte[] bytes, int offset = 0) where T : struct
    {
        int size = Marshal.SizeOf<T>();
        byte[] arr = new byte[size];
        Buffer.BlockCopy(bytes, offset, arr, 0, size);
        GCHandle handle = GCHandle.Alloc(arr, GCHandleType.Pinned);
        T result = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
        handle.Free();
        return result;
    }

    private static byte[] StructureToByteArray(object obj)
    {
        int size = Marshal.SizeOf(obj);
        byte[] arr = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(obj, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);
        return arr;
    }
}