using UnityEngine;

public class LevelTrigger : MonoBehaviour
{
    public enum TriggerKind { Coin, Hazard, Checkpoint, Finish }
    public TriggerKind kind;
    public Vector3 checkpointPosition;
    private bool triggered;

    void OnTriggerEnter(Collider other)
    {
        PlayerPlatformer player = other.GetComponent<PlayerPlatformer>();
        if (player == null || GameFlow.Instance == null) return;
        switch (kind)
        {
            case TriggerKind.Coin:
                if (triggered) return;
                triggered = true;
                GameFlow.Instance.CollectCoin();
                Destroy(gameObject);
                break;
            case TriggerKind.Hazard:
                player.Respawn();
                break;
            case TriggerKind.Checkpoint:
                if (triggered) return;
                triggered = true;
                GameFlow.Instance.SetCheckpoint(checkpointPosition);
                break;
            case TriggerKind.Finish:
                if (!GameFlow.Instance.HasWon) GameFlow.Instance.Win();
                break;
        }
    }
}
