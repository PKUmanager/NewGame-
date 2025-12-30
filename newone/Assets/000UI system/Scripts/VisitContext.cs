using UnityEngine;

public enum CampusVisitMode
{
    Self,
    Friend
}

public class VisitContext : MonoBehaviour
{
    public static VisitContext Instance { get; private set; }

    public CampusVisitMode Mode { get; private set; }
    public string TargetPlayerId { get; private set; } // ×Ô¼º or ºÃÓÑID

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void VisitSelf(string myPlayerId)
    {
        Mode = CampusVisitMode.Self;
        TargetPlayerId = myPlayerId;
    }

    public void VisitFriend(string friendPlayerId)
    {
        Mode = CampusVisitMode.Friend;
        TargetPlayerId = friendPlayerId;
    }
}
