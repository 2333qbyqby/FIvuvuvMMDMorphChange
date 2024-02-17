using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace FivuvuvMMD
{
    internal class FivuvuvMorph
    {
        [MenuItem("Assets/FivuvuvMMD/Convert MorphVMD TO MorphTempSO")]
        internal static void CreateMorphToMorphTempSO()
        {
            System.GC.Collect();
            string path =
                AssetDatabase.GetAssetPath(Selection.GetFiltered<DefaultAsset>(SelectionMode.Assets).FirstOrDefault());

            if (Path.GetExtension(path).ToUpper().Contains("VMD"))
            {
                var stream = File.Open(path, FileMode.Open);

                var vmd = VMDParser.ParseVMD(stream);

                //创建一个数据中转
                MorphTempSO morphTempSO = ScriptableObject.CreateInstance<MorphTempSO>();
                List<VMDSingleFrame> vMDSingleFrames = new List<VMDSingleFrame>();
                Dictionary<string, int> morphMap = new Dictionary<string, int>();

                foreach (var vmdMorphFrame in vmd.Morphs)
                {
                    VMDSingleFrame vMDSingleFrame = new VMDSingleFrame();
                    vMDSingleFrame.MorphName = vmdMorphFrame.MorphName;
                    vMDSingleFrame.FrameIndex = vmdMorphFrame.FrameIndex;
                    vMDSingleFrame.Weight = vmdMorphFrame.Weight;
                    vMDSingleFrames.Add(vMDSingleFrame);
                    if (!morphMap.ContainsKey(vmdMorphFrame.MorphName))
                    {
                        morphMap.Add(vmdMorphFrame.MorphName, 0);
                    }
                }
                morphTempSO.SetMorphSO(vMDSingleFrames);
                //添加映射初始值
                List<VMDMorphMap> vMDMorphMaps = new List<VMDMorphMap>();
                foreach (var item in morphMap)
                {
                    VMDMorphMap vMDMorphMap = new VMDMorphMap();
                    vMDMorphMap.morphName = item.Key;
                    vMDMorphMap.morphIndex = 0;
                    vMDMorphMaps.Add(vMDMorphMap);
                }
                morphTempSO.SetMorphMap(vMDMorphMaps);
                AssetDatabase.CreateAsset(morphTempSO, path.Replace("vmd", "asset"));
                //设置MorphTemp中的映射

                stream.Close();
            }
        }
        [MenuItem("Assets/FivuvuvMMD/Convert MorphTempSO TO AnimationClip")]
        internal static void ConvertMorphTempSO()
        {
            System.GC.Collect();
            //if (Path.GetExtension(path).ToUpper().Contains("ASSET"))
            //{
            MorphTempSO morphTempSO = Selection.activeObject as MorphTempSO;
            string path = AssetDatabase.GetAssetPath(morphTempSO);
            Debug.Log(path);
            //创建一个全新的动画片段
            var animationClip = new AnimationClip() { frameRate = 30 };
            animationClip.legacy = false;
            var delta = 1 / animationClip.frameRate;
            Dictionary<string, List<Keyframe>> keyframes = new Dictionary<string, List<Keyframe>>();
            //将同一个表情的帧数据放在一起
            foreach (var vmdMorphFrame in morphTempSO.frames)
            {
                if (keyframes.ContainsKey(vmdMorphFrame.MorphName))
                {
                    keyframes[vmdMorphFrame.MorphName].Add(new Keyframe(vmdMorphFrame.FrameIndex * delta, vmdMorphFrame.Weight * 100));
                }
                else
                {
                    keyframes.Add(vmdMorphFrame.MorphName, new List<Keyframe>() { new Keyframe(vmdMorphFrame.FrameIndex * delta, vmdMorphFrame.Weight * 100) });
                }
            }
            GameObject gameObject = GameObject.Find("U_Char_2");
            Dictionary<int, string> blendShapeNames = GetblendShapeNames(gameObject.GetComponent<SkinnedMeshRenderer>());

            foreach (var item in keyframes)
            {
                AnimationCurve animationCurve = new AnimationCurve(item.Value.ToArray());
                string gameObjectName = gameObject.name;
                string parentName = gameObject.transform.parent.name;
                try
                {
                    //通过tempSO中的映射，找到对应的blendShape的index,再用index找到对应的blendShape的名字,获取property
                    if (GetMapIdFromMorphTempSO(morphTempSO, item.Key) != -1)
                    {
                        string registerName = blendShapeNames[GetMapIdFromMorphTempSO(morphTempSO, item.Key)];
                        animationClip.SetCurve(parentName + "/" + gameObjectName, typeof(SkinnedMeshRenderer), "blendShape." + registerName, animationCurve);
                    }
                }
                catch (Exception e)
                {

                    Debug.LogError($"Error:{e.Message}");
                }
            }
            AssetDatabase.CreateAsset(animationClip, path.Replace("asset", "anim"));
            //}
        }
        private static int GetMapIdFromMorphTempSO(MorphTempSO morphTempSO, string morphName)
        {
            for (int i = 0; i < morphTempSO.morphMap.Count; i++)
            {
                if (morphTempSO.morphMap[i].morphName == morphName)
                {
                    return morphTempSO.morphMap[i].morphIndex;
                }
            }
            return -1;
        }

        private static Dictionary<int, string> GetblendShapeNames(SkinnedMeshRenderer skinnedMeshRenderer)
        {
            var mesh = skinnedMeshRenderer.sharedMesh;
            var bsCounts = mesh.blendShapeCount;
            var blendShapeNames = Enumerable.Range(0, bsCounts).ToList()
                .ConvertAll(index => mesh.GetBlendShapeName(index));
            Dictionary<int, string> blendShapeNameDict = new Dictionary<int, string>();
            for (int i = 0; i < blendShapeNames.Count; i++)
            {
                blendShapeNameDict.Add(i, blendShapeNames[i]);
            }
            return blendShapeNameDict;
        }
    }
}



