using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelineSwapper : MonoBehaviour
{
    [SerializeField] TimelineAsset newTimeline;
    [SerializeField] PlayableDirector director;

    public void Swap()
    {
        director.playableAsset = newTimeline;
        director.Play();
    }
}
