using System;
using UnityEngine;

[Serializable]
public class StalkerRoamingState : State<StalkerBehaviour>
{
    [SerializeField] private float _RoamingSpeed;
}
