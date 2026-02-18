using UnityEngine;

public class PlayerEquipmentHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _playerEquipmentDisplay;
    private PlayerStats _playerStats;

    void Awake()
    {
        _playerStats = GetComponent<PlayerStats>();
    }
}
