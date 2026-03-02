using System;
using System.Collections.Generic;
using UnityEngine;

public enum BeatEventType { Beat, Kick, Snare, Hat, Section }

[Serializable]
public class BeatEvent
{
    public float time;
    public BeatEventType type;
    public int value; // optional (e.g., section id/intensity)
}

[CreateAssetMenu(menuName="TimeOff3/BeatMap")]
public class BeatMap : ScriptableObject
{
    public AudioClip clip;
    public List<BeatEvent> events = new();
}
