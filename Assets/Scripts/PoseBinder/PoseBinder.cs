using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class PoseBinder : MonoBehaviour
{
    private enum BodyPart
    {
        Hips, Spine1, Head,
        LeftArm, LeftForeArm, LeftHand, LeftShoulder,
        RightArm, RightForeArm, RightHand, RightShoulder,
        LeftUpLeg, LeftLeg, LeftFoot,
        RightUpLeg, RightLeg, RightFoot,
        Count
    }

    private static readonly HumanBodyBones[] HumanBones =
    {
        HumanBodyBones.Hips,
        HumanBodyBones.Spine,
        HumanBodyBones.Head,
        HumanBodyBones.LeftUpperArm,
        HumanBodyBones.LeftLowerArm,
        HumanBodyBones.LeftHand,
        HumanBodyBones.LeftShoulder,
        HumanBodyBones.RightUpperArm,
        HumanBodyBones.RightLowerArm,
        HumanBodyBones.RightHand,
        HumanBodyBones.RightShoulder,
        HumanBodyBones.LeftUpperLeg,
        HumanBodyBones.LeftLowerLeg,
        HumanBodyBones.LeftFoot,
        HumanBodyBones.RightUpperLeg,
        HumanBodyBones.RightLowerLeg,
        HumanBodyBones.RightFoot
    };

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

    [Header("Model")]
    [SerializeField] private Animator _animator;

    [Tooltip("SensorBaseProj에서 별도 장치로 제어하던 머리, 어깨, 손, 발에도 센서 회전을 적용합니다.")]
    [SerializeField] private bool _applyOptionalBones;

    [Tooltip("0이면 수신 회전을 즉시 적용합니다.")]
    [Min(0f)]
    [SerializeField] private float _rotationLerpSpeed;

    [Header("Pose Sensor UDP")]
    [SerializeField] private string _senderIp = "192.168.201.199";
    [SerializeField] private int _senderPort = 53000;
    [SerializeField] private int _receivePort = 55000;
    [Min(0)]
    [SerializeField] private int _playerIndex;

    private readonly Transform[] _bones = new Transform[(int)BodyPart.Count];
    private readonly Quaternion[] _sourceRotations = new Quaternion[(int)BodyPart.Count];
    private readonly Quaternion[] _receivedRotations = new Quaternion[(int)BodyPart.Count];
    private readonly byte[] _floatBytes = new byte[sizeof(float)];

    private UdpClient _udpClient;
    private Thread _receiveThread;
    private volatile bool _threadRunning;
    private byte[] _latestPacket;
    private bool _isInitialized;
    private bool _hasReceivedPose;

    private void Awake()
    {
        if (_animator == null)
        {
            _animator = GetComponentInParent<Animator>();
        }
    }

    private void Start()
    {
        InitializeBones();
        StartConnection();
    }

    private void LateUpdate()
    {
        byte[] packet = Interlocked.Exchange(ref _latestPacket, null);
        if (packet != null && TryUnpackBodyRotations(packet))
        {
            _hasReceivedPose = true;
        }

        if (_hasReceivedPose)
        {
            ApplyBodyRotations();
        }
    }

    private void InitializeBones()
    {
        if (_animator == null || !_animator.isHuman)
        {
            Debug.LogError(
                $"[{nameof(PoseBinder)}] Humanoid Animator를 찾을 수 없습니다.",
                this);
            enabled = false;
            return;
        }

        for (int i = 0; i < (int)BodyPart.Count; i++)
        {
            Transform bone = _animator.GetBoneTransform(HumanBones[i]);
            _bones[i] = bone;
            _receivedRotations[i] = Quaternion.identity;

            if (bone != null)
            {
                _sourceRotations[i] = bone.rotation;
            }
            else
            {
                Debug.LogWarning(
                    $"[{nameof(PoseBinder)}] {HumanBones[i]} 본을 찾지 못했습니다.",
                    this);
            }
        }

        _isInitialized = true;
    }

    private void StartConnection()
    {
        if (!_isInitialized || _threadRunning)
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
                Name = $"PoseBinder UDP {port}"
            };
            _receiveThread.Start();

            Debug.Log($"[{nameof(PoseBinder)}] UDP 수신 시작: {port}", this);
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[{nameof(PoseBinder)}] UDP {port} 포트를 열지 못했습니다: " +
                exception.Message,
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
            (((int)BodyPart.Count - 1) * PACKET_INTERVAL) +
            QUATERNION_SIZE - 1;

        int lastSourceByte = CompactIndexToSourceIndex(lastCompactByte);

        return packet != null &&
               packet.Length > lastSourceByte &&
               packet[0] == 0xfa &&
               packet[1] == 0xef;
    }

    private bool TryUnpackBodyRotations(byte[] packet)
    {
        if (!IsPosePacket(packet))
        {
            return false;
        }

        for (int i = 0; i < (int)BodyPart.Count; i++)
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

            if (sqrMagnitude > 0.000001f)
            {
                float inverseMagnitude = 1f / Mathf.Sqrt(sqrMagnitude);
                rotation.x *= inverseMagnitude;
                rotation.y *= inverseMagnitude;
                rotation.z *= inverseMagnitude;
                rotation.w *= inverseMagnitude;
                _receivedRotations[i] = rotation;
            }
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
        // SensorBaseProj의 PlayerScript는 오른손, 왼손 압력 데이터 20바이트를
        // 차례대로 제거한 다음 신체 Quaternion을 해석한다.
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

    private void ApplyBodyRotations()
    {
        for (int i = 0; i < (int)BodyPart.Count; i++)
        {
            BodyPart bodyPart = (BodyPart)i;

            if (!_applyOptionalBones && IsOptionalBone(bodyPart))
            {
                continue;
            }

            Transform bone = _bones[i];
            if (bone == null)
            {
                continue;
            }

            Quaternion targetRotation =
                _receivedRotations[i] * _sourceRotations[i];

            if (_rotationLerpSpeed <= 0f)
            {
                bone.rotation = targetRotation;
            }
            else
            {
                float lerpAmount =
                    1f - Mathf.Exp(-_rotationLerpSpeed * Time.deltaTime);
                bone.rotation = Quaternion.Slerp(
                    bone.rotation,
                    targetRotation,
                    lerpAmount);
            }
        }
    }

    private static bool IsOptionalBone(BodyPart bodyPart)
    {
        return bodyPart == BodyPart.Head ||
               bodyPart == BodyPart.LeftShoulder ||
               bodyPart == BodyPart.RightShoulder ||
               bodyPart == BodyPart.LeftHand ||
               bodyPart == BodyPart.RightHand ||
               bodyPart == BodyPart.LeftFoot ||
               bodyPart == BodyPart.RightFoot;
    }

    [ContextMenu("Send T-Pose")]
    public void SendTPose()
    {
        if (_udpClient == null)
        {
            Debug.LogWarning(
                $"[{nameof(PoseBinder)}] UDP 연결이 시작되지 않았습니다.",
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
                $"[{nameof(PoseBinder)}] T-Pose 전송 실패: " +
                exception.Message,
                this);
        }
    }

    private void OnDestroy()
    {
        StopConnection();
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
