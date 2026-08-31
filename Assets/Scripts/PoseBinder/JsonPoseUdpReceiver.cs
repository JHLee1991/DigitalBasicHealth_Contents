using System;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

[DefaultExecutionOrder(-200)]
[DisallowMultipleComponent]
public sealed class JsonPoseUdpReceiver : MonoBehaviour
{
    private const int MAX_DATAGRAM_SIZE = 4096;
    private const int REQUIRED_FIELD_MASK = (1 << 11) - 1;

    public static JsonPoseUdpReceiver Instance { get; private set; }

    [Header("UDP")]
    [SerializeField] private int _receivePort = 4000;
    [SerializeField] private bool _dontDestroyOnLoad = true;

    public JsonPoseDataDto LatestPose { get; } = new JsonPoseDataDto();
    public bool HasPose { get; private set; }
    public uint FrameVersion { get; private set; }

    private readonly byte[] _receiveBuffer = new byte[MAX_DATAGRAM_SIZE];
    private readonly byte[] _pendingBuffer = new byte[MAX_DATAGRAM_SIZE];
    private readonly byte[] _parseBuffer = new byte[MAX_DATAGRAM_SIZE];
    private readonly object _bufferLock = new object();

    private UdpClient _udpClient;
    private Thread _receiveThread;
    private volatile bool _isReceiving;
    private int _pendingLength;
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

        int jsonLength;
        lock (_bufferLock)
        {
            jsonLength = _pendingLength;
            if (jsonLength <= 0)
            {
                return;
            }

            Buffer.BlockCopy(_pendingBuffer, 0, _parseBuffer, 0, jsonLength);
            _pendingLength = 0;
        }

        if (!TryParsePose(_parseBuffer, jsonLength, LatestPose))
        {
            Debug.LogWarning( $"[{nameof(JsonPoseUdpReceiver)}] JSON 형식 또는 필드가 올바르지 않습니다.",  this);
            return;
        }

        HasPose = true;
        FrameVersion++;
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

            Debug.Log($"[{nameof(JsonPoseUdpReceiver)}] UDP 수신 시작: {_receivePort}", this);
        }
        catch (Exception exception)
        {
            Debug.LogError( $"[{nameof(JsonPoseUdpReceiver)}] UDP {_receivePort} 포트를 열지 못했습니다: {exception.Message}", this);
            StopReceiving();
        }
    }

    private void ReceiveLoop()
    {
        while (_isReceiving)
        {
            try
            {
                int receivedLength = _udpClient.Client.Receive(_receiveBuffer, 0, _receiveBuffer.Length, SocketFlags.None);

                if (receivedLength <= 0)
                {
                    continue;
                }

                lock (_bufferLock)
                {
                    Buffer.BlockCopy(_receiveBuffer, 0, _pendingBuffer, 0, receivedLength);
                    _pendingLength = receivedLength;
                }
            }
            catch (SocketException exception)
            {
                if (_isReceiving)
                {
                    Interlocked.Exchange(ref _pendingError, $"UDP 수신 오류: {exception.Message}");
                }

                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception exception)
            {
                Interlocked.Exchange(ref _pendingError, $"UDP 수신 오류: {exception.Message}");
                break;
            }
        }
        _isReceiving = false;
    }

    private static bool TryParsePose(byte[] json, int length, JsonPoseDataDto pose)
    {
        int index = 0;
        int parsedMask = 0;

        SkipWhiteSpace(json, length, ref index);
        if (!Consume(json, length, ref index, (byte)'{'))
        {
            return false;
        }

        while (index < length)
        {
            SkipWhiteSpace(json, length, ref index);
            if (Consume(json, length, ref index, (byte)'}'))
            {
                return parsedMask == REQUIRED_FIELD_MASK;
            }

            int keyStart;
            int keyLength;
            if (!TryReadKey(json, length, ref index, out keyStart, out keyLength))
            {
                return false;
            }

            SkipWhiteSpace(json, length, ref index);
            if (!Consume(json, length, ref index, (byte)':'))
            {
                return false;
            }

            SkipWhiteSpace(json, length, ref index);

            float[] target;
            int fieldBit;
            if (!TryGetField(json, keyStart, keyLength, pose, out target, out fieldBit) ||
                !TryReadFloatArray(json, length, ref index, target))
            {
                return false;
            }

            parsedMask |= fieldBit;
            SkipWhiteSpace(json, length, ref index);

            if (Consume(json, length, ref index, (byte)','))
            {
                continue;
            }

            if (Consume(json, length, ref index, (byte)'}'))
            {
                return parsedMask == REQUIRED_FIELD_MASK;
            }

            return false;
        }

        return false;
    }

    private static bool TryGetField(
        byte[] json,
        int start,
        int length,
        JsonPoseDataDto pose,
        out float[] target,
        out int bit)
    {
        if (KeyEquals(json, start, length, "position"))
        {
            target = pose.position; bit = 1 << 0;
        }
        else if (KeyEquals(json, start, length, "Hips"))
        {
            target = pose.Hips; bit = 1 << 1;
        }
        else if (KeyEquals(json, start, length, "RightUpLeg"))
        {
            target = pose.RightUpLeg; bit = 1 << 2;
        }
        else if (KeyEquals(json, start, length, "LeftUpLeg"))
        {
            target = pose.LeftUpLeg; bit = 1 << 3;
        }
        else if (KeyEquals(json, start, length, "RightLeg"))
        {
            target = pose.RightLeg; bit = 1 << 4;
        }
        else if (KeyEquals(json, start, length, "LeftLeg"))
        {
            target = pose.LeftLeg; bit = 1 << 5;
        }
        else if (KeyEquals(json, start, length, "RightArm"))
        {
            target = pose.RightArm; bit = 1 << 6;
        }
        else if (KeyEquals(json, start, length, "LeftArm"))
        {
            target = pose.LeftArm; bit = 1 << 7;
        }
        else if (KeyEquals(json, start, length, "RightForeArm"))
        {
            target = pose.RightForeArm; bit = 1 << 8;
        }
        else if (KeyEquals(json, start, length, "LeftForeArm"))
        {
            target = pose.LeftForeArm; bit = 1 << 9;
        }
        else if (KeyEquals(json, start, length, "Spine2"))
        {
            target = pose.Spine2; bit = 1 << 10;
        }
        else
        {
            target = null; bit = 0; return false;
        }

        return true;
    }

    private static bool TryReadKey(
        byte[] json,
        int length,
        ref int index,
        out int start,
        out int keyLength)
    {
        start = 0;
        keyLength = 0;

        if (!Consume(json, length, ref index, (byte)'\"'))
        {
            return false;
        }

        start = index;
        while (index < length && json[index] != (byte)'\"')
        {
            index++;
        }

        if (index >= length)
        {
            return false;
        }

        keyLength = index - start;
        index++;
        return true;
    }

    private static bool TryReadFloatArray(
        byte[] json,
        int length,
        ref int index,
        float[] destination)
    {
        if (!Consume(json, length, ref index, (byte)'['))
        {
            return false;
        }

        for (int i = 0; i < destination.Length; i++)
        {
            SkipWhiteSpace(json, length, ref index);

            float value;
            if (!TryReadFloat(json, length, ref index, out value))
            {
                return false;
            }

            destination[i] = value;
            SkipWhiteSpace(json, length, ref index);

            if (i < destination.Length - 1 &&
                !Consume(json, length, ref index, (byte)','))
            {
                return false;
            }
        }

        SkipWhiteSpace(json, length, ref index);
        return Consume(json, length, ref index, (byte)']');
    }

    private static bool TryReadFloat(
        byte[] json,
        int length,
        ref int index,
        out float result)
    {
        result = 0f;
        bool negative = false;

        if (index < length && (json[index] == (byte)'-' || json[index] == (byte)'+'))
        {
            negative = json[index] == (byte)'-';
            index++;
        }

        bool hasDigit = false;
        double value = 0d;
        while (index < length && IsDigit(json[index]))
        {
            hasDigit = true;
            value = value * 10d + json[index++] - (byte)'0';
        }

        if (index < length && json[index] == (byte)'.')
        {
            index++;
            double scale = 0.1d;
            while (index < length && IsDigit(json[index]))
            {
                hasDigit = true;
                value += (json[index++] - (byte)'0') * scale;
                scale *= 0.1d;
            }
        }

        if (!hasDigit)
        {
            return false;
        }

        if (index < length && (json[index] == (byte)'e' || json[index] == (byte)'E'))
        {
            index++;
            bool exponentNegative = false;
            if (index < length && (json[index] == (byte)'-' || json[index] == (byte)'+'))
            {
                exponentNegative = json[index] == (byte)'-';
                index++;
            }

            if (index >= length || !IsDigit(json[index]))
            {
                return false;
            }

            int exponent = 0;
            while (index < length && IsDigit(json[index]))
            {
                exponent = exponent * 10 + json[index++] - (byte)'0';
            }

            double scale = 1d;
            for (int i = 0; i < exponent; i++)
            {
                scale *= 10d;
            }

            value = exponentNegative ? value / scale : value * scale;
        }

        result = (float)(negative ? -value : value);
        return true;
    }

    private static bool KeyEquals(byte[] json, int start, int length, string expected)
    {
        if (length != expected.Length)
        {
            return false;
        }

        for (int i = 0; i < length; i++)
        {
            if (json[start + i] != (byte)expected[i])
            {
                return false;
            }
        }

        return true;
    }

    private static void SkipWhiteSpace(byte[] json, int length, ref int index)
    {
        while (index < length)
        {
            byte value = json[index];
            if (value != (byte)' ' && value != (byte)'\t' &&
                value != (byte)'\r' && value != (byte)'\n')
            {
                return;
            }

            index++;
        }
    }

    private static bool Consume(byte[] json, int length, ref int index, byte expected)
    {
        if (index >= length || json[index] != expected)
        {
            return false;
        }

        index++;
        return true;
    }

    private static bool IsDigit(byte value)
    {
        return value >= (byte)'0' && value <= (byte)'9';
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
