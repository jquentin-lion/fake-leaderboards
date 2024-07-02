using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LionStudios.Suite.Core;

namespace LionStudios.Suite.Leaderboards.Fake
{
    [RequireComponent(typeof(Button))]
    public class LeagueShowButton : MonoBehaviour
    {
        [SerializeField] private bool useLeaguesManagerSingleton = true;
        [SerializeField] internal LeaguesManager leaguesManager;

        [SerializeField] private GameObject joinState;
        [SerializeField] private GameObject progressState;
        [SerializeField] private GameObject finishedState;

        [SerializeField] private TMP_Text progressRankText;
        [SerializeField] private TMP_Text finishedRankText;
        [SerializeField] private GameObject upGO;
        [SerializeField] private GameObject downGO;
        [SerializeField] private Image ProgressHaloImg;

        [SerializeField] [Tooltip("In seconds")]
        private float autoUpdateInterval = 1f;

        private int lastRank;

        private CancellationTokenSource cancellerTokenSource = new CancellationTokenSource();

        private bool hasStarted = false;

        private void Awake()
        {
            if (useLeaguesManagerSingleton)
                leaguesManager = LeaguesManager.Instance;
            GetComponent<Button>().onClick.AddListener(OnClick);

            leaguesManager.OnConfigOverridden += HandleButtonActiveState;
        }

        private void OnEnable()
        {
            // We don't call UpdateButton on 1st OnEnable because some things are initialized in Awake so we call the 1st one on Start instead.
            if (hasStarted)
                AutoUpdateData();
        }

        private void OnDisable()
        {
            StopAutoUpdateData();
        }

        private void OnDestroy()
        {
            leaguesManager.OnConfigOverridden -= HandleButtonActiveState;
        }

        private void Start()
        {
            // This gets checked before the leagues remote config variables have been processed, so check to see if dev by default has leaguesManager disabled
            if (!leaguesManager.isEnabled)
            {
                HandleButtonActiveState();
            }

            AutoUpdateData();
            hasStarted = true;
        }

        private void UpdateButton()
        {
            if (!leaguesManager.HasJoined)
            {
                joinState.SetActive(true);
                progressState.SetActive(false);
                finishedState.SetActive(false);
            }
            else if (leaguesManager.HasOutdatedScores())
            {
                joinState.SetActive(false);
                progressState.SetActive(false);
                finishedState.SetActive(true);
                int rank = leaguesManager.GetStoredScores().playerIndex;
                finishedRankText.text = (rank + 1).ToString();
                lastRank = -1;
            }
            else
            {
                joinState.SetActive(false);
                progressState.SetActive(true);
                finishedState.SetActive(false);
                int rank = leaguesManager.GetCurrentScores().playerIndex;
                if (lastRank >= 0 && rank < lastRank)
                {
                    upGO.SetActive(true);
                    downGO.SetActive(false);
                    ProgressHaloImg.gameObject.SetActive(true);
                    ProgressHaloImg.color = Color.green;
                    progressRankText.color = Color.green;
                    progressRankText.text = (rank + 1).ToString();
                }
                else if (lastRank >= 0 && rank > lastRank)
                {
                    upGO.SetActive(false);
                    downGO.SetActive(true);
                    ProgressHaloImg.gameObject.SetActive(true);
                    progressRankText.color = Color.red;
                    ProgressHaloImg.color = Color.red;
                    progressRankText.text = (rank + 1).ToString();
                }
                else
                {
                    upGO.SetActive(false);
                    downGO.SetActive(false);
                    ProgressHaloImg.gameObject.SetActive(false);
                    progressRankText.color = Color.white;
                    progressRankText.text = (rank + 1).ToString();
                }
            }
        }

        private void OnClick()
        {
            if (!LeaguesManager.Instance.IsInitialized)
            {
                return;
            }
            
            leaguesManager.Show();
            lastRank = leaguesManager.HasJoined ? leaguesManager.GetCurrentScores().playerIndex : 0;
        }


        async void AutoUpdateData()
        {
            try
            {
                cancellerTokenSource.Dispose();
                cancellerTokenSource = new CancellationTokenSource();
                await Task.Delay(100, cancellerTokenSource.Token);

                await TaskWaiter.WaitUntil(() => leaguesManager.IsInitialized);
            
                while (!cancellerTokenSource.IsCancellationRequested)
                {
                    UpdateButton();
                    await Task.Delay((int)(autoUpdateInterval * 1000), cancellerTokenSource.Token);
                }
            }
            catch (TaskCanceledException)
            {
            }
        }

        void StopAutoUpdateData()
        {
            cancellerTokenSource.Cancel(false);
        }

        private void HandleButtonActiveState()
        {
            if (!leaguesManager.isEnabled)
            {
                gameObject.SetActive(false);
            }
        }
    }
}