using MoreMountains.Feedbacks;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class PointUI : MonoBehaviour
{
    public static PointUI Instance;
    [SerializeField] private TextMeshProUGUI pointText;
    [SerializeField] private MMF_Player pointFeedback;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        PhotonManager.s.onCallAnyCmdFunction += OnCallAnyCmdFunction;
    }
    void OnDestroy()
    {
        PhotonManager.s.onCallAnyCmdFunction -= OnCallAnyCmdFunction;
    }

    private void OnCallAnyCmdFunction(CmdOrder order)
    {
        if (order.receiver == nameof(PointUI) && order.functionName == nameof(RpcUpdateUI))
        {
            RpcUpdateUI(order.data[0].ToString());
        }
    }

    public void UpdateUI()
    {
        CmdUpdateUI(DeliveryManager.Instance.GetSuccessfulRecipePoint().ToString());
    }

    private void CmdUpdateUI(string point)
    {
        PhotonManager.s.CmdCallFunction(new CmdOrder(nameof(PointUI), nameof(RpcUpdateUI), point));
    }

    private void RpcUpdateUI(string point)
    {
        pointText.text = point;
        pointFeedback?.PlayFeedbacks();
    }
}
