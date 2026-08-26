using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PoseDataReceiver : MonoBehaviour
{
    public static PoseDataReceiver Instance { get; private set; }

    private const int PACKET_OFFSET = 3;
    private const int PACKET_INTERVAL = 17;
    private const int QUATERNION_SIZE = 16;
    private const int LEFT_FINGER_START_INDEX = 104;
    private const int RIGHT_FINGER_START_INDEX = 192;
    private const int FINGER_DATA_SIZE = sizeof(float) * 5;

    private static readonly byte[] TPoseDatagram =
    {
        0xfa, 0xef, 0x30, 0x30, 0x30,
        0x30, 0xa1, 0x03, 0xfb, 0xff
    };

    [Header("Pose Data UDP")]
    [SerializeField] private string _senderIp = "192.168.201.199";
    [SerializeField] private int _senderPort = 53000;
    [SerializeField] private int _receivePort = 55000;
    [Min(0)]
    [SerializeField] private int _playerIndex;

    public PoseFrame LatestPose { get; } = new PoseFrame();
    public bool HasPose { get; private set; }

    private readonly byte[] _floatBytes = new byte[sizeof(float)];

    private UdpClient _udpClient;
    private Thread _receiveThread;
    private volatile bool _threadRunning;
    private byte[] _latestPacket;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning(
                $"[{nameof(PoseDataReceiver)}] 중복 인스턴스를 제거합니다.",
                this);
            Destroy(gameObject); 
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (Instance != this)
        {
            return;
        }

        StartConnection();
    }

    private void Update()
    {
        byte[] packet = Interlocked.Exchange(ref _latestPacket, null);
        if (packet != null && TryUnpackPose(packet))
        {
            HasPose = true;
        }
    }

    private void StartConnection()
    {
        if (_threadRunning)
        {
            return;
        }

        int port = _receivePort + _playerIndex;

        try
        {
            _udpClient = new UdpClient(port);
            _threadRunning = true;
            _receiveThread = new Thread(ReceiveLoop)
            {
                IsBackground = true,
                Name = $"PoseDataReceiver UDP {port}"
            };
            _receiveThread.Start();

            Debug.Log(
                $"[{nameof(PoseDataReceiver)}] UDP 수신 시작: {port}",
                this);
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[{nameof(PoseDataReceiver)}] UDP {port} 포트를 " +
                $"열지 못했습니다: {exception.Message}",
                this);
            StopConnection();
        }
    }

    private void ReceiveLoop()
    {
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        while (_threadRunning)
        {
            try
            {
                byte[] packet = _udpClient.Receive(ref remoteEndPoint);

                if (IsPosePacket(packet))
                {
                    Interlocked.Exchange(ref _latestPacket, packet);
                }
            }
            catch (SocketException)
            {
                if (_threadRunning)
                {
                    _threadRunning = false;
                }
            }
            catch (ObjectDisposedException)
            {
                break;
            }
        }
    }

    private static bool IsPosePacket(byte[] packet)
    {
        int lastCompactByte =
            PACKET_OFFSET +
            (((int)PoseBodyPart.Count - 1) * PACKET_INTERVAL) +
            QUATERNION_SIZE - 1;

        int lastSourceByte = CompactIndexToSourceIndex(lastCompactByte);

        return packet != null &&
               packet.Length > lastSourceByte &&
               packet[0] == 0xfa &&
               packet[1] == 0xef;
    }

    private bool TryUnpackPose(byte[] packet)
    {
        if (!IsPosePacket(packet))
        {
            return false;
        }

        for (int i = 0; i < (int)PoseBodyPart.Count; i++)
        {
            int compactIndex = (i * PACKET_INTERVAL) + PACKET_OFFSET;

            Quaternion rotation = new Quaternion
            {
                w = ReadPacketFloat(packet, compactIndex),
                z = -ReadPacketFloat(packet, compactIndex + 4),
                x = ReadPacketFloat(packet, compactIndex + 8),
                y = -ReadPacketFloat(packet, compactIndex + 12)
            };

            float sqrMagnitude =
                rotation.x * rotation.x +
                rotation.y * rotation.y +
                rotation.z * rotation.z +
                rotation.w * rotation.w;

            if (sqrMagnitude <= 0.000001f)
            {
                continue;
            }

            float inverseMagnitude = 1f / Mathf.Sqrt(sqrMagnitude);
            rotation.x *= inverseMagnitude;
            rotation.y *= inverseMagnitude;
            rotation.z *= inverseMagnitude;
            rotation.w *= inverseMagnitude;

            LatestPose[(PoseBodyPart)i] = rotation;
        }

        return true;
    }

    private float ReadPacketFloat(byte[] packet, int compactIndex)
    {
        for (int i = 0; i < sizeof(float); i++)
        {
            int sourceIndex = CompactIndexToSourceIndex(compactIndex + i);
            _floatBytes[sizeof(float) - 1 - i] = packet[sourceIndex];
        }

        return BitConverter.ToSingle(_floatBytes, 0);
    }

    private static int CompactIndexToSourceIndex(int compactIndex)
    {
        // 기존 SensorBaseProj는 오른손, 왼손 압력 데이터를 제거한 뒤
        // 신체 Quaternion을 해석한다. 원본 배열을 복사하지 않고 동일한
        // 위치를 읽도록 인덱스를 보정한다.
        if (compactIndex >= RIGHT_FINGER_START_INDEX - FINGER_DATA_SIZE)
        {
            return compactIndex + (FINGER_DATA_SIZE * 2);
        }

        if (compactIndex >= LEFT_FINGER_START_INDEX)
        {
            return compactIndex + FINGER_DATA_SIZE;
        }

        return compactIndex;
    }

    [ContextMenu("Send T-Pose")]
    public void SendTPose()
    {
        if (_udpClient == null)
        {
            Debug.LogWarning(
                $"[{nameof(PoseDataReceiver)}] UDP 연결이 시작되지 않았습니다.",
                this);
            return;
        }

        try
        {
            IPEndPoint serverEndPoint = new IPEndPoint(
                IPAddress.Parse(_senderIp),
                _senderPort + _playerIndex);

            _udpClient.Send(
                TPoseDatagram,
                TPoseDatagram.Length,
                serverEndPoint);
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[{nameof(PoseDataReceiver)}] T-Pose 전송 실패: " +
                exception.Message,
                this);
        }
    }

    private void OnDestroy()
    {
        StopConnection();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnApplicationQuit()
    {
        StopConnection();
    }

    private void StopConnection()
    {
        _threadRunning = false;

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
