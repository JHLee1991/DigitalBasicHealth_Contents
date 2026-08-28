using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

[DefaultExecutionOrder(-200)]
[DisallowMultipleComponent]
public sealed class JsonPoseUdpReceiver : MonoBehaviour
{
    public static JsonPoseUdpReceiver Instance { get; private set; }

    [Header("UDP")]
    [SerializeField] private int _receivePort = 4000;
    [SerializeField] private bool _dontDestroyOnLoad = true;

    public JsonPoseDataDto LatestPose { get; private set; }
    public bool HasPose => LatestPose != null;
    public uint FrameVersion { get; private set; }

    private UdpClient _udpClient;
    private Thread _receiveThread;
    private volatile bool _isReceiving;
    private string _pendingJson;
    private string _pendingError;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        //if (_dontDestroyOnLoad)
        //{
        //    DontDestroyOnLoad(gameObject);
        //}
    }

    private void Start()
    {
        if (Instance == this)
        {
            StartReceiving();
        }
    }

    private void Update()
    {
        string error = Interlocked.Exchange(ref _pendingError, null);
        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogError($"[{nameof(JsonPoseUdpReceiver)}] {error}", this);
        }

        string json = Interlocked.Exchange(ref _pendingJson, null);
        if (string.IsNullOrWhiteSpace(json))
        {
            return;
        }

        try
        {
            JsonPoseDataDto pose = JsonUtility.FromJson<JsonPoseDataDto>(json);
            //if (pose == null || !pose.HasValidLengths())
            if (pose == null)
            {
                Debug.LogWarning(
                    $"[{nameof(JsonPoseUdpReceiver)}] JSON 필드가 없거나 배열 길이가 올바르지 않습니다.",
                    this);
                return;
            }

            LatestPose = pose;
            FrameVersion++;
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                $"[{nameof(JsonPoseUdpReceiver)}] JSON 파싱 실패: {exception.Message}",
                this);
        }
    }

    private void StartReceiving()
    {
        if (_isReceiving)
        {
            return;
        }

        try
        {
            _udpClient = new UdpClient(_receivePort);
            _isReceiving = true;
            _receiveThread = new Thread(ReceiveLoop)
            {
                IsBackground = true,
                Name = $"JSON Pose UDP {_receivePort}"
            };
            _receiveThread.Start();

            Debug.Log(
                $"[{nameof(JsonPoseUdpReceiver)}] UDP 수신 시작: {_receivePort}",
                this);
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[{nameof(JsonPoseUdpReceiver)}] UDP {_receivePort} 포트를 열지 못했습니다: " +
                exception.Message,
                this);
            StopReceiving();
        }
    }

    private void ReceiveLoop()
    {
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        while (_isReceiving)
        {
            try
            {
                byte[] datagram = _udpClient.Receive(ref remoteEndPoint);
                string json = Encoding.UTF8.GetString(datagram);
                Debug.Log($"Json Reveived!!! : {json}");
                Interlocked.Exchange(ref _pendingJson, json);
            }
            catch (SocketException exception)
            {
                if (_isReceiving)
                {
                    Interlocked.Exchange(
                        ref _pendingError,
                        $"UDP 수신 오류: {exception.Message}");
                }

                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception exception)
            {
                Interlocked.Exchange(
                    ref _pendingError,
                    $"UDP 수신 오류: {exception.Message}");
                break;
            }
        }

        _isReceiving = false;
    }

    private void OnDestroy()
    {
        StopReceiving();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnApplicationQuit()
    {
        StopReceiving();
    }

    private void StopReceiving()
    {
        _isReceiving = false;

        UdpClient client = _udpClient;
        if (client != null)
        {
            client.Close();
        }

        if (_receiveThread != null && _receiveThread.IsAlive)
        {
            _receiveThread.Join(500);
        }

        _udpClient = null;
        _receiveThread = null;
    }
}
