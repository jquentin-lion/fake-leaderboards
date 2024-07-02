using LionStudios.Suite.Core;
using LionStudios.Suite.Leaderboards.Fake;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestRace : MonoBehaviour
{
    public RaceConfig raceConfig;

    [SerializeField] private int increaseAmount = 5;

    [SerializeField] private TextMeshProUGUI txtIncrease;

    [SerializeField] private Button testBtn;
    [SerializeField] private Button playButton;


    private void Awake()
    {
        playButton.gameObject.SetActive(false);
        testBtn.gameObject.SetActive(false);

        txtIncrease.text = "Increase " + increaseAmount;
        testBtn.onClick.AddListener(IncreasePlayerScore);
    }

    void Start()
    {
        LionGameInterfaces.SetAdsInterface(new MyAdsInterface());
        raceConfig.RaceDescription =
            $"Beat <color=yellow>{raceConfig.Leaderboard.GetLeaderboardData().totalScore} levels</color> before others\nto win amazing rewards!";
        RaceManager.Instance.OverrideConfig(raceConfig);
        RaceManager.Instance.OnMainRacePanelStateChanged += OnMainPanelStateChanged;
        RaceManager.Instance.OnRaceJoined += OnRaceJoined;
        RaceManager.Instance.OnRaceContinue += OnRaceJoined;
        RaceManager.Instance.OnRaceFinished += OnRaceFinished;
        RaceManager.Instance.Initialize();
    }

    private void OnRaceJoined()
    {
        playButton.gameObject.SetActive(true);
        testBtn.gameObject.SetActive(true);
    }

    private void OnRaceFinished()
    {
        playButton.gameObject.SetActive(false);
        testBtn.gameObject.SetActive(false);
    }

    private void OnMainPanelStateChanged(bool status)
    {
        Debug.Log(status ? "Pause Gameplay Show Race Panel" : "Resume Gameplay on closing the panel");
    }

    public void IncreasePlayerScore()
    {
        RaceManager.Instance.IncreasePlayerScore(increaseAmount);
    }
}

public class MyAdsInterface : ILionAdsInterface
{
    public bool IsRewardedReady()
    {
        Debug.Log("Check to see if rewarded ad is ready or not");
        //return MaxSdk.IsRewardedAdReady(AD_UNIT_ID);
        return true;
    }

    public void ShowRewardedAd(string placement, ILionGameInterface.OnCompleteHandler onComplete)
    {
        //Request Reward video show here
        Debug.Log("Show rewarded ad.");
        onComplete?.Invoke(true);
    }
}