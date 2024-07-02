using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionStudios.Suite.Core.LeanTween;
using UnityEngine;
using UnityEngine.UI;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class LeaderboardEntriesDisplay : MonoBehaviour
    {
        
        [SerializeField] private Transform leaderboardTopThreeContentTransform;
        [SerializeField] private Transform leaderboardScrollContentTransform;
        [SerializeField] private LeaderboardEntryDisplay prefab;
        [SerializeField] private GameObject promotionPrefab;
        [SerializeField] private GameObject demotionPrefab;

        private Dictionary<Transform, Vector3> scrollRanksPreviousSyncedPositions = new Dictionary<Transform, Vector3>();
        private Dictionary<Transform, Vector3> topRanksPreviousSyncedPositions = new Dictionary<Transform, Vector3>();
        private List<EntryData<LeaderboardEntryDisplay>> topThreeRankEntries;
        private List<EntryData<LeaderboardEntryDisplay>> scrollRankEntries;
        private Transform playerEntry;
        private GameObject promotionSeparator;
        private GameObject demotionSeparator;
        
        private int promoteCount;

        private ContentSizeFitter sizeFitter;
        private HorizontalOrVerticalLayoutGroup layoutGroup;
        private ScrollRect scrollRect;

        private const int TopRanksCount = 3;

        private bool _isDataAlreadyUpdating = false;
        private bool _animatePlayerOnly;
        private Vector3 _startSizeOfEntryDisplay;
        
        private class EntryData<T>
        {
            public string participantName;
            public T leaderboardEntryDisplay;
        }

        public void Init(LeaderboardCalculatedData data, int promoteCount, bool hasPromotionZone, bool hasDemotionZone, bool animatePlayerOnly)
        {
            layoutGroup = leaderboardScrollContentTransform.GetComponent<HorizontalOrVerticalLayoutGroup>();
            sizeFitter = leaderboardScrollContentTransform.GetComponent<ContentSizeFitter>();
            scrollRect = leaderboardScrollContentTransform.GetComponentInParent<ScrollRect>();
            this.promoteCount = promoteCount;
            _animatePlayerOnly = animatePlayerOnly;
            leaderboardScrollContentTransform.DestroyChildrenImmediate();

            topThreeRankEntries = new List<EntryData<LeaderboardEntryDisplay>>();
            scrollRankEntries = new List<EntryData<LeaderboardEntryDisplay>>();

            var displays = leaderboardTopThreeContentTransform.GetComponentsInChildren<LeaderboardEntryDisplay>();
            for(int i = 0; i < TopRanksCount; i++)
            {
                var display = displays[i];
                if (i > data.participantDatas.Count)
                {
                    display.gameObject.SetActive(false);
                    return;
                }
                
                ParticipantData participantData = data.participantDatas[i];
                bool isPlayer = i == data.playerIndex;
                display.Init(i, participantData, isPlayer);
                topThreeRankEntries.Add(new EntryData<LeaderboardEntryDisplay>()
                {
                    participantName = participantData.name,
                    leaderboardEntryDisplay = display
                });
            }
           
            for (var i = 0; i < data.participantDatas.Count; i++)
            {
                if (hasPromotionZone && i == promoteCount)
                    promotionSeparator = Instantiate(promotionPrefab, leaderboardScrollContentTransform);
                if (hasDemotionZone && i == data.participantDatas.Count - promoteCount)
                    demotionSeparator = Instantiate(demotionPrefab, leaderboardScrollContentTransform);
                ParticipantData participantData = data.participantDatas[i];
                LeaderboardEntryDisplay instance = Instantiate(prefab, leaderboardScrollContentTransform);
                bool isPlayer = i == data.playerIndex;
                instance.Init(i, participantData, isPlayer);
                scrollRankEntries.Add(new EntryData<LeaderboardEntryDisplay>()
                {
                    participantName = data.participantDatas[i].name,
                    leaderboardEntryDisplay = instance
                });

                _startSizeOfEntryDisplay = instance.transform.localScale;
            }

            _isDataAlreadyUpdating = false;
            
            Canvas.ForceUpdateCanvases();
        }

        public void UpdateData(LeaderboardCalculatedData data, bool focusOnPlayer, bool animated)
        {
            if(_isDataAlreadyUpdating)
                return;
            
            _isDataAlreadyUpdating = true;
            
            LeanTween.cancelAll(true);
            scrollRanksPreviousSyncedPositions.Clear();
            topRanksPreviousSyncedPositions.Clear();

            var previousPlayerEntryData = scrollRankEntries.FirstOrDefault(x => x.leaderboardEntryDisplay._isPlayer);
            int previousPlayerRank = -1;

            if (previousPlayerEntryData != null)
            {
                previousPlayerRank = previousPlayerEntryData.leaderboardEntryDisplay._rank;
            }

            var participantList = data.GetParticipantList();

            //Set old initial position list first
            for (int i = 0; i < participantList.Count; i++)
            {
                var participant = participantList[i];
                
                //For all ranks
                EntryData<LeaderboardEntryDisplay> previousEntryDisplayData = scrollRankEntries.Find(p => p.participantName == participant.name);
                
                //If previous entry was also in scroll rect content ranks
                if (previousEntryDisplayData != null)
                {
                    scrollRanksPreviousSyncedPositions[scrollRankEntries[i].leaderboardEntryDisplay.transform] =
                        previousEntryDisplayData.leaderboardEntryDisplay.transform.localPosition;
                } 
            }

            //Update UI data
            for (int i = 0; i < participantList.Count ; i++)
            {
                var participant = participantList[i];
                int rank = i;
                bool isPlayer = i == data.playerIndex;
                
                if (rank < TopRanksCount)
                {
                    topThreeRankEntries[i].participantName = participant.name;
                    topThreeRankEntries[i].leaderboardEntryDisplay.UpdateData(rank, participant, isPlayer);
                }
                
                int scrollRankIndex = rank;
                scrollRankEntries[scrollRankIndex].participantName = participant.name;
                scrollRankEntries[scrollRankIndex].leaderboardEntryDisplay.UpdateData(rank, participant, isPlayer);
                
                if (isPlayer)
                    playerEntry = scrollRankEntries[scrollRankIndex].leaderboardEntryDisplay.transform;
            }
            
            for (int i = 0; i < scrollRankEntries.Count; i++)
            {
                scrollRankEntries[i].leaderboardEntryDisplay.transform.SetSiblingIndex(i);
            }

            if (demotionSeparator != null)
            {
                int demotionIndex = scrollRankEntries.Count - promoteCount;
                demotionIndex = Mathf.Clamp(demotionIndex, 0, data.participantDatas.Count());
                demotionSeparator.transform.SetSiblingIndex(demotionIndex);
            }

            if (promotionSeparator != null)
            {
                // int promotionIndex = promoteCount - TopRanksCount;
                int promotionIndex = promoteCount;
                promotionIndex = Mathf.Clamp(promotionIndex, 0, data.participantDatas.Count());
                promotionSeparator.transform.SetSiblingIndex(promotionIndex);
            }

            Canvas.ForceUpdateCanvases();

            if (focusOnPlayer)
            {
                FocusOnPlayer();
            }
            
            if (animated)
            {
                if (_animatePlayerOnly)
                {
                    AnimateOnlyPlayerEntry(focusOnPlayer, previousPlayerRank);
                }
                else
                {
                    AnimateAllEntries(focusOnPlayer);
                }
            }
            else
            {
                _isDataAlreadyUpdating = false;
            }
        }
        
        async void AnimateOnlyPlayerEntry(bool focusOnPlayer, int previousPlayerRank)
        {
            if (focusOnPlayer)
            {
                FocusOnPlayer();
            }
            
            await Task.Yield();

            layoutGroup.enabled = false;
            sizeFitter.enabled = false;
            
            //To calculate difference of previous and new player rank 
            const float oneRankChangeAnimationTime = 0.2f;
            const float playerOnlyDelay = 0.25f;
            float extraRankChangeDelayTime = 0;

            //For all rows animation
            for (int i = 0; i < scrollRanksPreviousSyncedPositions.Count; i++)
            {
                var kvp = scrollRanksPreviousSyncedPositions.ElementAt(i);

                Vector3 targetNewPosition = kvp.Key.localPosition;
                Vector3 previousPosition = kvp.Value;

                kvp.Key.localPosition = previousPosition;

                if (focusOnPlayer && playerEntry != null && kvp.Key == playerEntry.transform)
                {
                    LeaderboardEntryDisplay entryDisplay = kvp.Key.GetComponent<LeaderboardEntryDisplay>();

                    //Means player score is increased, so show animation
                    if (!IsDecreasePosition(previousPosition, targetNewPosition))
                    {
                        int numberOfPlayerRankChanged = 0;
                        //Just a precaution check that previous player rank is set, otherwise just assign any default value
                        //Here it is 3
                        if (previousPlayerRank == -1)
                        {
                            extraRankChangeDelayTime = 2;
                        }
                        else
                        {
                            numberOfPlayerRankChanged =
                                Mathf.Abs(previousPlayerRank) - Mathf.Abs(entryDisplay._rank);
                            numberOfPlayerRankChanged = Mathf.Abs(numberOfPlayerRankChanged);

                            extraRankChangeDelayTime = numberOfPlayerRankChanged * oneRankChangeAnimationTime;
                        }

                        if (numberOfPlayerRankChanged > 0)
                        {
                            sizeFitter.enabled = false;
                            layoutGroup.enabled = false;

                            await Task.Yield();
                            
                            entryDisplay.PutThisOnTopOfSortingOrder();

                            LeanTween.scale(kvp.Key.gameObject, _startSizeOfEntryDisplay + Vector3.one * 0.1f,
                                    playerOnlyDelay).setOnUpdate(FocusOnPlayer)
                                .setOnComplete(() =>
                                {
                                    LeanTween.moveLocal(kvp.Key.gameObject, targetNewPosition,
                                            extraRankChangeDelayTime)
                                        .setOnUpdate(FocusOnPlayer).setEase(LeanTweenType.easeInOutCubic)
                                        .setOnComplete(
                                            () =>
                                            {
                                                kvp.Key.gameObject.transform.localPosition = targetNewPosition;

                                                LeanTween.scale(kvp.Key.gameObject, _startSizeOfEntryDisplay,
                                                        playerOnlyDelay)
                                                    .setOnUpdate(FocusOnPlayer).setOnComplete(
                                                        () => { entryDisplay.ResetSortingOrder(); });
                                            });
                                });
                        }
                        else
                        {
                            entryDisplay.ResetSortingOrder();

                            //No need to disable these two if player score is decreased as no animation will be played.
                            sizeFitter.enabled = true;
                            layoutGroup.enabled = true;
                        }
                    }
                    //If player score decreased from previous time
                    else
                    {
                        entryDisplay.ResetSortingOrder();

                        //No need to disable these two if player score is decreased as no animation will be played.
                        sizeFitter.enabled = true;
                        layoutGroup.enabled = true;
                    }
                }
                else
                {
                    //All other Npc's
                    kvp.Key.gameObject.transform.localPosition = targetNewPosition;
                }
            }

            await Task.Delay((int)(playerOnlyDelay * 1000f + extraRankChangeDelayTime * 1000f));
            
            if (layoutGroup != null)
                layoutGroup.enabled = true;
            if (sizeFitter != null)
                sizeFitter.enabled = true;

            _isDataAlreadyUpdating = false;
        }

        async void AnimateAllEntries(bool focusOnPlayer)
        {
            if (layoutGroup != null)
                layoutGroup.enabled = false;
            if (sizeFitter != null)
                sizeFitter.enabled = false;
            
            //For all rows animation
            for (int i = 0; i < scrollRanksPreviousSyncedPositions.Count; i++)
            {
                var kvp = scrollRanksPreviousSyncedPositions.ElementAt(i);
                
                Vector3 targetNewPosition = kvp.Key.localPosition;
                Vector3 previousPosition = kvp.Value;
                kvp.Key.localPosition = previousPosition;

                if (focusOnPlayer && playerEntry != null && kvp.Key == playerEntry.transform)
                {
                    LeaderboardEntryDisplay entryDisplay = kvp.Key.GetComponent<LeaderboardEntryDisplay>();

                    entryDisplay.PutThisOnTopOfSortingOrder();

                    LeanTween.moveLocal(kvp.Key.gameObject, targetNewPosition, 1f).setOnUpdate(FocusOnPlayer)
                        .setOnComplete(
                            () =>
                            {
                                entryDisplay.ResetSortingOrder();
                                kvp.Key.gameObject.transform.localPosition = targetNewPosition;
                            });
                }
                else
                {
                    LeaderboardEntryDisplay entryDisplay = kvp.Key.GetComponent<LeaderboardEntryDisplay>();
                    entryDisplay.ResetSortingOrder();
                    LeanTween.moveLocal(kvp.Key.gameObject, targetNewPosition, 0.7f).setOnComplete(() =>
                    {
                        kvp.Key.gameObject.transform.localPosition = targetNewPosition;
                    });
                }
            }

            await Task.Delay(1100);
            
            if (layoutGroup != null)
                layoutGroup.enabled = true;
            if (sizeFitter != null)
                sizeFitter.enabled = true;

            _isDataAlreadyUpdating = false;
        }

        public void FocusOnPlayer()
        {
            if (playerEntry == null && scrollRankEntries!=null)
            {
                if (scrollRankEntries.Count > 0)
                {
                    scrollRect.FocusOnItem(scrollRankEntries[0].leaderboardEntryDisplay.GetComponent<RectTransform>());
                }
            }
            else
            {
                scrollRect.FocusOnItem(playerEntry.GetComponent<RectTransform>());
            }
        }

        void FocusOnPlayer(float v)
        {
            FocusOnPlayer();
        }

        private bool IsDecreasePosition(Vector3 oldPosition, Vector3 newPosition)
        {
            return newPosition.y < oldPosition.y - 1;
        }
    }
}
