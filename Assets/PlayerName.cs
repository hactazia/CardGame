using CardGameVR.Controllers;
using CardGameVR.Players;
using CardGameVR.UI;
using UnityEngine;

public class PlayerName : MonoBehaviour
{
    public Transform head;
    public TMPro.TextMeshProUGUI textMesh;
    public float distance = .2f;
    public PlayerNetwork player;

    private void Start()
    {
        if (!player) return;
        SetName(player.PlayerName);
        player.onPlayerNameChanged.AddListener(SetName);
    }

    private void OnDestroy()
    {
        if (!player) return;
        player.onPlayerNameChanged.RemoveListener(SetName);
    }

    private void SetName(string playerName)
    {
        textMesh.text = playerName;
        ForceUpdateLayout.UpdateManually(gameObject);
    }

    private void LateUpdate()
    {
        if (!head) return;
        if (!ControllerManager.Controller.TryGetTransform(HumanBodyBones.Head, out var cameraPos)) return;
        transform.position = head.position + Vector3.up * distance;
        transform.LookAt(cameraPos.position);
        transform.Rotate(0, 180, 0);
    }
}