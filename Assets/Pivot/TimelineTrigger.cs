using UnityEngine;
using UnityEngine.Playables; // is needed to find timeline functions
using UnityEngine.Timeline; 

public class TimelineTrigger : MonoBehaviour
{
    [SerializeField] private PlayableDirector _timeline; //if [SerializeField] private it is still shown in the inspector but no other scripts that read it
    [SerializeField] private string _playerTag = "Player"; //if only public, other scripts could read it ([SerializeField] private is generally preferred) 
    [SerializeField] TimelineAsset newTimeline;

    private void OnTriggerEnter(Collider other) //when any collider enters this trigger, call this automauticcly. "other" = any collider that just entered
    {
        if (!other.CompareTag(_playerTag)) return; //checks if the entered collider has the playerTag, if else (!) end everything here
        if (_timeline != null)
            _timeline.playableAsset = newTimeline; //safety check that a timeline is assigned, then play it. without null it would become errors if you didnt assign anything
        _timeline.Play(); 
    }
} 