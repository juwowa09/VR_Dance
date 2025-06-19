// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.InputSystem;
// using System.IO;
//
// public class Mocap : MonoBehaviour
// {
//     public Animator animator;
//     private float time;
//     private bool isRecording;
//     private List<FrameData> allFrames;
//     private float recordInterval = 0.3f; // 0.1초마다 기록
//     private float recordTimer = 0f;
//     HumanBodyBones[] bones = new HumanBodyBones[]
//     {
//         HumanBodyBones.Head, HumanBodyBones.Neck, HumanBodyBones.Hips, HumanBodyBones.Spine, HumanBodyBones.Chest,
//         HumanBodyBones.LeftShoulder, HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand,
//         HumanBodyBones.RightShoulder, HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand,
//         HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot, HumanBodyBones.LeftToes,
//         HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot, HumanBodyBones.RightToes
//     };
//     string[] boneNames = new string[]
//     {
//         "Head", "Neck", "Hips", "Spine", "Chest",
//         "L_Shoulder", "L_UpperArm", "L_LowerArm", "L_hand",
//         "R_Shoulder", "R_UpperArm", "R_LowerArm", "R_hand",
//         "L_UpperLeg", "L_LowerLeg", "L_Foot", "L_Toe",
//         "R_UpperLeg", "R_LowerLeg", "R_Foot", "R_Toe"
//     };
//     
//     private Dictionary<string, Quaternion> tPoseRotations;
//     [System.Serializable]
//     public class FrameDataWrapper
//     {
//         public List<FrameData> frames = new List<FrameData>();
//     }
//     [System.Serializable]
//     public struct BoneTransform
//     {
//         public string name;
//         public Quaternion rotation;
//     }
//     [System.Serializable]
//     public class FrameData
//     {
//         public string time;
//         public List<BoneTransform> bones = new List<BoneTransform>();
//     }
//     // Start is called before the first frame update
//     public void setRecord()
//     {
//         Debug.Log("Start Record");
//         isRecording = true;
//         allFrames.Clear();
//     }
//     public void StopAndSave()
//     {
//         Debug.Log("End Record");
//         isRecording = false;
//         string path = Application.dataPath + "/mocap_data.json";
//         FrameDataWrapper wrapper = new FrameDataWrapper();
//         wrapper.frames = allFrames;
//         string json = JsonUtility.ToJson(wrapper, true);
//         File.WriteAllText(path, json);
//         Debug.Log($"Mocap data saved to: {path}");
//     }
//     void Start()
//     {
//         time = 0;
//         isRecording = false;
//         allFrames = new List<FrameData>();
//         tPoseRotations = new Dictionary<string, Quaternion>();
//         for (int i = 0; i < bones.Length; i++)
//         {
//             Transform boneTransform = animator.GetBoneTransform(bones[i]);
//             if (boneTransform != null)
//             {
//                 tPoseRotations[boneNames[i]] = boneTransform.localRotation;
//             }
//         }
//     }
//
//     // Update is called once per frame
//     void Update()
//     {
//         if (isRecording && Keyboard.current.eKey.wasPressedThisFrame)
//         {
//             StopAndSave();
//         }
//         if (!isRecording && Keyboard.current.sKey.wasPressedThisFrame)
//         {
//             setRecord();
//         }
//         
//         if (!isRecording) return;
//         time += Time.deltaTime;
//         recordTimer += Time.deltaTime;
//         if (recordTimer < recordInterval) return;
//         recordTimer = 0;
//         
//         FrameData frame = new FrameData();
//         
//         frame.time = time.ToString("F2");
//         
//         // 기록 대상 본 이름 리스트
//         for (int i = 0; i < bones.Length; i++)
//         {
//             Transform boneTransform = animator.GetBoneTransform(bones[i]);
//
//             if (boneTransform != null)
//             {
//                 Quaternion rawRotation = boneTransform.localRotation;
//                 Quaternion offset = tPoseRotations[boneNames[i]];
//                 Quaternion relativeRotation = rawRotation * Quaternion.Inverse(offset);
//
//                 // 소수점 둘째 자리까지 반올림
//                 float rx = Mathf.Round(relativeRotation.x * 100f) / 100f;
//                 float ry = Mathf.Round(relativeRotation.y * 100f) / 100f;
//                 float rz = Mathf.Round(relativeRotation.z * 100f) / 100f;
//                 float rw = Mathf.Round(relativeRotation.w * 100f) / 100f;
//
//                 BoneTransform bt = new BoneTransform
//                 {
//                     name = boneNames[i],
//                     rotation = new Quaternion(rx, ry, rz, rw)
//                 };
//                 frame.bones.Add(bt);
//             }
//         }
//
//         allFrames.Add(frame);
//     }
// }
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor; // Editor API 사용

public class Mocap : MonoBehaviour
{
    public Animator animator;
    public Animator target;
    private float time;
    private bool isRecording;
    private float recordInterval = 0.0335f; // 60fps
    private float recordTimer = 0f;

    HumanBodyBones[] bones = new HumanBodyBones[]
    {
        HumanBodyBones.Head, HumanBodyBones.Neck, HumanBodyBones.Hips, HumanBodyBones.Spine, HumanBodyBones.Chest,
        HumanBodyBones.LeftShoulder, HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand,
        HumanBodyBones.RightShoulder, HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand,
        HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot, HumanBodyBones.LeftToes,
        HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot, HumanBodyBones.RightToes
    };

    string[] boneNames = new string[]
    {
        "Head", "Neck", "Hips", "Spine", "Chest",
        "L_Shoulder", "L_UpperArm", "L_LowerArm", "L_Hand",
        "R_Shoulder", "R_UpperArm", "R_LowerArm", "R_Hand",
        "L_UpperLeg", "L_LowerLeg", "L_Foot", "L_Toes",
        "R_UpperLeg", "R_LowerLeg", "R_Foot", "R_Toes"
    };

    private Dictionary<string, List<Quaternion>> recordedRotations;
    private Dictionary<string, Quaternion> tPoseRotations;

    void Start()
    {
        time = 0f;
        isRecording = false;
        recordedRotations = new Dictionary<string, List<Quaternion>>();
        tPoseRotations = new Dictionary<string, Quaternion>();

        for (int i = 0; i < bones.Length; i++)
        {
            Transform boneTransform = animator.GetBoneTransform(bones[i]);
            if (boneTransform != null)
            {
                tPoseRotations[boneNames[i]] = boneTransform.localRotation;
                recordedRotations[boneNames[i]] = new List<Quaternion>();
            }
        }
    }

    void Update()
    {
        if (isRecording && UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
        {
            SaveAnimationClip();
            isRecording = false;
        }
        if (!isRecording && UnityEngine.InputSystem.Keyboard.current.sKey.wasPressedThisFrame)
        {
            target.enabled = false;
            Debug.Log("Start Recording");
            isRecording = true;
            time = 0f;
            recordTimer = 0f;
            foreach (var key in recordedRotations.Keys)
                recordedRotations[key].Clear();
        }

        if (!isRecording) return;

        time += Time.deltaTime;
        recordTimer += Time.deltaTime;
        if (recordTimer < recordInterval) return;
        recordTimer = 0f;


        for (int i = 0; i < bones.Length; i++)
        {
            // 원본 아바타의 본
            Transform sourceBone = animator.GetBoneTransform(bones[i]);
            // 타겟 아바타의 본
            
            Transform targetBone = FindTransformRecursive(target.transform, boneNames[i]);

            if (sourceBone != null && targetBone != null)
            {
                // 회전 보정 없이 그대로 복사
                targetBone.localRotation = sourceBone.localRotation;
                // ✅ 기록
                Quaternion current = targetBone.localRotation;
                if (!recordedRotations.ContainsKey(boneNames[i]))
                {
                    recordedRotations[boneNames[i]] = new List<Quaternion>();
                }
                var list = recordedRotations[boneNames[i]];
                if (list.Count > 0 && Quaternion.Dot(list[list.Count - 1], current) < 0f)
                {
                    current = new Quaternion(-current.x, -current.y, -current.z, -current.w);
                }

                list.Add(current);
                // 혹은 T-Pose offset을 적용한 상대 회전 복사 (기록 기준 동일하게 맞추려면 아래 방식)
                // Quaternion offset = tPoseRotations[boneNames[i]];
                // Quaternion relativeRotation = sourceBone.localRotation * Quaternion.Inverse(offset);
                // targetBone.localRotation = relativeRotation;
            }
        }
    }

    void SmoothCurve(AnimationCurve curve)
    {
        for (int i = 0; i < curve.length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
            AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
        }
    }
    void SaveAnimationClip()
    {
        AnimationClip clip = new AnimationClip();
        clip.frameRate = 1f / recordInterval;

        foreach (var kvp in recordedRotations)
        {
            string boneName = kvp.Key;

            // ⬇️ target.transform 기준으로 본 탐색 (캐시 사용 안 함)
            Transform boneTransform = FindTransformRecursive(target.transform, boneName);
            if (boneTransform == null)
            {
                Debug.LogWarning($"❌ 저장 실패: {boneName} 본 없음");
                continue;
            }

            string path = GetRelativePath(target.transform, boneTransform);

            AnimationCurve curveX = new AnimationCurve();
            AnimationCurve curveY = new AnimationCurve();
            AnimationCurve curveZ = new AnimationCurve();
            AnimationCurve curveW = new AnimationCurve();
            


            List<Quaternion> rotations = kvp.Value;
            for (int i = 0; i < rotations.Count; i++)
            {
                float t = i * recordInterval;
                Quaternion q = rotations[i];
                curveX.AddKey(t, q.x);
                curveY.AddKey(t, q.y);
                curveZ.AddKey(t, q.z);
                curveW.AddKey(t, q.w);
            }
            
            SmoothCurve(curveX);
            SmoothCurve(curveY);
            SmoothCurve(curveZ);
            SmoothCurve(curveW);

            clip.SetCurve(path, typeof(Transform), "localRotation.x", curveX);
            clip.SetCurve(path, typeof(Transform), "localRotation.y", curveY);
            clip.SetCurve(path, typeof(Transform), "localRotation.z", curveZ);
            clip.SetCurve(path, typeof(Transform), "localRotation.w", curveW);
        }

        AssetDatabase.CreateAsset(clip, "Assets/Mocap_Recorded.anim");
        AssetDatabase.SaveAssets();
        Debug.Log("✅ AnimationClip 저장 완료: Assets/Mocap_Recorded.anim");
    }

    string GetRelativePath(Transform root, Transform target)
    {
        if (target == root) return "";
        string path = target.name;
        while (target.parent != null && target.parent != root)
        {
            target = target.parent;
            path = target.name + "/" + path;
        }
        return path;
    }

    Transform FindTransformRecursive(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        foreach (Transform child in parent)
        {
            Transform found = FindTransformRecursive(child, name);
            if (found != null) return found;
        }
        return null;
    }
}
