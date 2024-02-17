using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MorphTempSO : ScriptableObject
{
    public List<VMDSingleFrame> frames;

    public List<VMDMorphMap> morphMap;
    public void SetMorphSO(List<VMDSingleFrame> frames)
    {
        this.frames = frames;
    }
    public void SetMorphMap(List<VMDMorphMap> morphMap)
    {
        this.morphMap = morphMap;
    }
}
[Serializable]
public class VMDMorphMap
{
    public string morphName;

    public int morphIndex;
}
