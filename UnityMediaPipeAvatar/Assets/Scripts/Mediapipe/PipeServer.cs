// REVIEWS

using System;
using System.Collections;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

/* Currently very messy because both the server code and hand-drawn code is all in the same file here.
 * But it is still fairly straightforward to use as a reference/base.
 */

[DefaultExecutionOrder(-1)]
public class PipeServer : MonoBehaviour
{
    public bool useLegacyPipes = false; // True to use NamedPipes for interprocess communication (not supported on Linux)
    // private string host = "192.168.0.16"; // This machines host.
    private string host = "127.0.0.1"; // This machines host.
    public int port = 52733; // Must match the Python side.
    public Transform bodyParent;
    public GameObject landmarkPrefab;
    public GameObject linePrefab;
    public GameObject headPrefab;
    public bool enableHead = true;
    public float multiplier = 10f;
    public float landmarkScale = 1f;
    private float maxSpeed = 120f;
    public float debug_samplespersecond;
    public int samplesForPose = 1;
    public bool active;

    private NamedPipeServerStream serverNP;
    private BinaryReader reader;
    private ServerUDP server;

    private Body body;

    // these virtual transforms are not actually provided by mediapipe pose, but are required for avatars.
    // so I just manually compute them
    private Transform virtualNeck;
    private Transform virtualHip;

    public Transform GetLandmark(Landmark mark)
    {
        return body.instances[(int)mark].transform ;
    }
    public Transform GetVirtualNeck()
    {
        return virtualNeck;
    }
    public Transform GetVirtualHip()
    {
        return virtualHip;
    }

    private void Start()
    {
        // float 문자열에서 파싱할 때 , . 차이 지정하는 구문
        System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

        body = new Body(bodyParent,landmarkPrefab,linePrefab,landmarkScale,enableHead?headPrefab:null);
        // 목과 hip이 존재하지 않음, mediapipe에 없으니까 이걸 retargeting 
        virtualNeck = new GameObject("VirtualNeck").transform;
        virtualHip = new GameObject("VirtualHip").transform;

        // T - Run 함수를 통해서 server 관절 위치 받아오는 thread => 퀘스트 장치에서는 병목현상 가능, thread 삭제함
        if (useLegacyPipes)
        {
            // Open the named pipe.
            serverNP = new NamedPipeServerStream("UnityMediaPipeBody1", PipeDirection.InOut, 99, PipeTransmissionMode.Message);

            print("Waiting for connection...");
            serverNP.WaitForConnection();

            print("Connected.");
            reader = new BinaryReader(serverNP, Encoding.UTF8);
        }
        else
        {
            // server = new ServerUDP(host, port);
            server = FindObjectOfType<ServerUDP>();
            server.Init(host, port);
            server.Connect();
            server.StartListeningAsync();
            print("Listening @"+host+":"+port);
        }

        SetVisible(true);
    }
    private void Update()
    {
        
        // 문자열 파싱하는 구문
        try
        {
            Body h = body;
            var len = 0;
            var str = "";

            if (useLegacyPipes)
            {
                len = (int)reader.ReadUInt32();
                str = new string(reader.ReadChars(len));
            }
            else
            {
                if(server.HasMessage())
                    str = server.GetMessage();
                len = str.Length;
            }

            string[] lines = str.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                    continue;
                string[] s = lines[i].Split('|');
                if (s.Length < 4) continue;
                int j;
                if (!int.TryParse(s[0], out j)) continue;
                
                // Debug.Log("new " + incoming * multiplier);
                // Debug.Log("old " + last);
                // float threshold = 4.0f; // 예시값
                //
                // if (Vector3.Distance(last, incoming * multiplier) < threshold)
                // {
                //     h.positionsBuffer[i].value += incoming;
                //     h.positionsBuffer[i].accumulatedValuesCount += 1;
                //     h.active = true;
                // }
                h.positionsBuffer[j].value += new Vector3(-float.Parse(s[1]), float.Parse(s[2]), float.Parse(s[3]));
                h.positionsBuffer[j].accumulatedValuesCount += 1;
                h.active = true;
            }
        }
        catch (EndOfStreamException)
        {
            print("Client Disconnected");
        }
        // 바디 좌표 갱신
        // UpdateBody(body);
    }

    private void LateUpdate()
    {
        UpdateBody(body);
    }

    private void UpdateBody(Body b)
    {
        for (int i = 0; i < LANDMARK_COUNT; ++i)
        {
            if (b.positionsBuffer[i].accumulatedValuesCount < samplesForPose)
                continue;
            
            b.localPositionTargets[i] = b.positionsBuffer[i].value / (float)b.positionsBuffer[i].accumulatedValuesCount * multiplier;
            b.positionsBuffer[i] = new AccumulatedBuffer(Vector3.zero,0);
        }

        for (int i = 0; i < LANDMARK_COUNT; ++i)
        {
            // 현재위치 -> localPositionTargets[i] 위치로 일정속도만큼 이동
            b.instances[i].transform.localPosition = Vector3.MoveTowards(
                b.instances[i].transform.localPosition, 
                b.localPositionTargets[i],
                Time.deltaTime * maxSpeed);
        }
        // mediapipe에서 제공 X, 임의로 neck, hip을 만들기
        virtualNeck.transform.position = (b.instances[(int)Landmark.RIGHT_SHOULDER].transform.position + b.instances[(int)Landmark.LEFT_SHOULDER].transform.position) / 2f;
        virtualHip.transform.position = (b.instances[(int)Landmark.RIGHT_HIP].transform.position + b.instances[(int)Landmark.LEFT_HIP].transform.position) / 2f;
        
        b.UpdateLines();
    }
    public void SetVisible(bool visible)
    {
        bodyParent.gameObject.SetActive(visible);
    }

    // private void Run()
    // {
    //     System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
    //
    //     if (useLegacyPipes)
    //     {
    //         // Open the named pipe.
    //         serverNP = new NamedPipeServerStream("UnityMediaPipeBody1", PipeDirection.InOut, 99, PipeTransmissionMode.Message);
    //
    //         print("Waiting for connection...");
    //         serverNP.WaitForConnection();
    //
    //         print("Connected.");
    //         reader = new BinaryReader(serverNP, Encoding.UTF8);
    //     }
    //     else
    //     {
    //         // server = new ServerUDP(host, port);
    //         server.Connect();
    //         server.StartListeningAsync();
    //         print("Listening @"+host+":"+port);
    //     }
    //
    //     while (true)
    //     {
    //         try
    //         {
    //             Body h = body;
    //             var len = 0;
    //             var str = "";
    //
    //             if (useLegacyPipes)
    //             {
    //                 len = (int)reader.ReadUInt32();
    //                 str = new string(reader.ReadChars(len));
    //             }
    //             else
    //             {
    //                 if(server.HasMessage())
    //                     str = server.GetMessage();
    //                 len = str.Length;
    //             }
    //
    //             string[] lines = str.Split('\n');
    //             foreach (string l in lines)
    //             {
    //                 if (string.IsNullOrWhiteSpace(l))
    //                     continue;
    //                 string[] s = l.Split('|');
    //                 if (s.Length < 4) continue;
    //                 int i;
    //                 if (!int.TryParse(s[0], out i)) continue;
    //                 h.positionsBuffer[i].value += new Vector3(float.Parse(s[1]), float.Parse(s[2]), float.Parse(s[3]));
    //                 h.positionsBuffer[i].accumulatedValuesCount += 1;
    //                 h.active = true;
    //             }
    //         }
    //         catch (EndOfStreamException)
    //         {
    //             print("Client Disconnected");
    //             break;
    //         }
    //     }
    //
    // }

    private void OnDisable()
    {
        print("Client disconnected.");
        if (useLegacyPipes)
        {
            serverNP.Close();
            serverNP.Dispose();
        }
        else
        {
            server.Disconnect();
        }
    }

    const int LANDMARK_COUNT = 33;
    const int LINES_COUNT = 11;

    public struct AccumulatedBuffer
    {
        public Vector3 value;       // 누적 위치 값
        public int accumulatedValuesCount;  // 누적횟수
        public AccumulatedBuffer(Vector3 v, int ac)
        {
            value = v;
            accumulatedValuesCount = ac;
        }
    }

    public class Body
    {
        public Transform parent;
        public AccumulatedBuffer[] positionsBuffer = new AccumulatedBuffer[LANDMARK_COUNT];
        public Vector3[] localPositionTargets = new Vector3[LANDMARK_COUNT];
        // 랜드마크 = 인스턴스
        public GameObject[] instances = new GameObject[LANDMARK_COUNT];
        public LineRenderer[] lines = new LineRenderer[LINES_COUNT];

        public bool active;

        public Body(Transform parent, GameObject landmarkPrefab, GameObject linePrefab, float s, GameObject headPrefab)
        {
            this.parent = parent;
            for (int i = 0; i < instances.Length; ++i)
            {
                instances[i] = Instantiate(landmarkPrefab);// GameObject.CreatePrimitive(PrimitiveType.Sphere);
                instances[i].transform.localScale = Vector3.one * s;
                instances[i].transform.parent = parent;
                instances[i].name = ((Landmark)i).ToString();
            }
            for (int i = 0; i < lines.Length; ++i)
            {
                lines[i] = Instantiate(linePrefab).GetComponent<LineRenderer>();
                lines[i].transform.parent = parent;
            }

            if (headPrefab)
            {
                GameObject head = Instantiate(headPrefab);
                head.transform.parent = instances[(int)Landmark.NOSE].transform;
                head.transform.localPosition = Vector3.zero;
                head.transform.localRotation = Quaternion.identity;
                head.transform.localScale = headPrefab.transform.localScale;
            }
        }
        public void UpdateLines()
        {
            lines[0].positionCount = 4;
            lines[0].SetPosition(0, Position((Landmark)32));
            lines[0].SetPosition(1, Position((Landmark)30));
            lines[0].SetPosition(2, Position((Landmark)28));
            lines[0].SetPosition(3, Position((Landmark)32));
            lines[1].positionCount = 4;
            lines[1].SetPosition(0, Position((Landmark)31));
            lines[1].SetPosition(1, Position((Landmark)29));
            lines[1].SetPosition(2, Position((Landmark)27));
            lines[1].SetPosition(3, Position((Landmark)31));

            lines[2].positionCount = 3;
            lines[2].SetPosition(0, Position((Landmark)28));
            lines[2].SetPosition(1, Position((Landmark)26));
            lines[2].SetPosition(2, Position((Landmark)24));
            lines[3].positionCount = 3;
            lines[3].SetPosition(0, Position((Landmark)27));
            lines[3].SetPosition(1, Position((Landmark)25));
            lines[3].SetPosition(2, Position((Landmark)23));

            lines[4].positionCount = 5;
            lines[4].SetPosition(0, Position((Landmark)24));
            lines[4].SetPosition(1, Position((Landmark)23));
            lines[4].SetPosition(2, Position((Landmark)11));
            lines[4].SetPosition(3, Position((Landmark)12));
            lines[4].SetPosition(4, Position((Landmark)24));

            lines[5].positionCount = 4;
            lines[5].SetPosition(0, Position((Landmark)12));
            lines[5].SetPosition(1, Position((Landmark)14));
            lines[5].SetPosition(2, Position((Landmark)16));
            lines[5].SetPosition(3, Position((Landmark)22));
            lines[6].positionCount = 4;
            lines[6].SetPosition(0, Position((Landmark)11));
            lines[6].SetPosition(1, Position((Landmark)13));
            lines[6].SetPosition(2, Position((Landmark)15));
            lines[6].SetPosition(3, Position((Landmark)21));

            lines[7].positionCount = 4;
            lines[7].SetPosition(0, Position((Landmark)16));
            lines[7].SetPosition(1, Position((Landmark)18));
            lines[7].SetPosition(2, Position((Landmark)20));
            lines[7].SetPosition(3, Position((Landmark)16));
            lines[8].positionCount = 4;
            lines[8].SetPosition(0, Position((Landmark)15));
            lines[8].SetPosition(1, Position((Landmark)17));
            lines[8].SetPosition(2, Position((Landmark)19));
            lines[8].SetPosition(3, Position((Landmark)15));

            lines[9].positionCount = 2;
            lines[9].SetPosition(0, Position((Landmark)10));
            lines[9].SetPosition(1, Position((Landmark)9));


            lines[10].positionCount = 9;
            lines[10].SetPosition(0, Position((Landmark)8));
            // lines[10].SetPosition(1, Position((Landmark)6));
            lines[10].SetPosition(2, Position((Landmark)5));
            // lines[10].SetPosition(3, Position((Landmark)4));
            lines[10].SetPosition(4, Position((Landmark)0));
            // lines[10].SetPosition(5, Position((Landmark)1));
            lines[10].SetPosition(6, Position((Landmark)2));
            // lines[10].SetPosition(7, Position((Landmark)3));
            lines[10].SetPosition(8, Position((Landmark)7));
        }

        public Vector3 Direction(Landmark from,Landmark to)
        {
            return (instances[(int)to].transform.position - instances[(int)from].transform.position).normalized;
        }
        public float Distance(Landmark from, Landmark to)
        {
            return (instances[(int)from].transform.position - instances[(int)to].transform.position).magnitude;
        }
        public Vector3 LocalPosition(Landmark Mark)
        {
            return instances[(int)Mark].transform.localPosition;
        }
        public Vector3 Position(Landmark Mark)
        {
            return instances[(int)Mark].transform.position;
        }

    }
}
