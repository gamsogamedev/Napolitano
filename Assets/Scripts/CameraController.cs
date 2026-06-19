using System.Collections.Generic;
using Player;
using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineTargetGroup targetGroup;

    private readonly HashSet<ulong> _currentMembers = new();
    private readonly HashSet<ulong> _activePlayers = new();
    private readonly List<ulong> _toRemove = new();

    private void LateUpdate()
    {
        SyncTargetGroup();
    }

    private void SyncTargetGroup()
    {
        if (targetGroup == null) return;

        _activePlayers.Clear();

        foreach (var kvp in PlayerController.AllPlayers)
        {
            var player = kvp.Value;
            if (player == null) continue;
            if (player.NetworkedStateType == PlayerController.PlayerStateType.Spoon) continue;

            _activePlayers.Add(kvp.Key);

            if (!_currentMembers.Contains(kvp.Key))
            {
                targetGroup.AddMember(player.transform, 1f, 1f);
                _currentMembers.Add(kvp.Key);
            }
        }

        _toRemove.Clear();
        foreach (var id in _currentMembers)
        {
            if (!_activePlayers.Contains(id))
                _toRemove.Add(id);
        }

        foreach (var id in _toRemove)
        {
            if (PlayerController.AllPlayers.TryGetValue(id, out var player) && player != null)
                targetGroup.RemoveMember(player.transform);
            _currentMembers.Remove(id);
        }
    }
}
