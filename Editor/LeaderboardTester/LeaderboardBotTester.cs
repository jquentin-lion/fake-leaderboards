using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LionStudios.Suite.Leaderboards.Fake.Editor
{
    public class LeaderboardBotTester : EditorWindow
    {
        private const string PACKAGE_NAME = "com.lionstudios.release.fakeleaderboards";

        private readonly string PATH_TO_LEADERBOARD_TEST_OBJECT =
            $"Packages/{PACKAGE_NAME}/Editor/LeaderboardTester/Data/LeaderboardCurveTester.prefab";

        private Animator _animator;
        AnimationClip _animationClip;
        private Animation _animation;
        private List<string> _animationClipNames = new List<string>();

        private LeaderboardType _leaderboardType;
        private int _playerMaximumScore = 100;
        private LeaderboardInstanceHolder _leaderboardInstanceHolder;
        private List<GameObject> allChildParticipantObjects = new List<GameObject>();
        
        //For PseudoLeadaerboard
        private int _minimumBotScore = 10;
        private int _maximumBotScore = 100;
        private float _targetWinRatio = 0.9f;

        //For time
        private int _competitionTotalTimeInSeconds = 120;
        private DateTime _realStartTime;
        private DateTime _realEndTime;

        private AnimationWindow _animationWindow;
        private static LeaderboardBotTester LeaderboardBotTesterWindow;
        Vector2 _scrollPosition = Vector2.zero;

        private SerializedObject _selectedLeaderboardSerializedObject;
        private LeaderboardType _lastLeaderboardType;
        

        [MenuItem("Tools/LeaderboardCurve Tester")]
        public static void Open()
        {
            LeaderboardBotTesterWindow = GetWindow<LeaderboardBotTester>();
            LeaderboardBotTesterWindow.minSize = new Vector2(600, 400);
        }

        private void OnEnable()
        {
            _leaderboardType = LeaderboardType.ProbabilityDistribution;
        }

        void OnGUI()
        {
            if (LeaderboardBotTesterWindow == null)
            {
                return;
            }
            
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition,
                false,
                true,
                GUILayout.Width(LeaderboardBotTesterWindow.position.width),
                GUILayout.Height(LeaderboardBotTesterWindow.position.height)
            );

            if (_leaderboardInstanceHolder == null)
            {
                GameObject leaderGameObject = GameObject.Find("LeaderboardCurveTester")?.gameObject;

                if (leaderGameObject == null)
                {
                    GameObject leaderPrefab =
                        (GameObject)AssetDatabase.LoadAssetAtPath(PATH_TO_LEADERBOARD_TEST_OBJECT, typeof(GameObject));
                    leaderGameObject = Instantiate(leaderPrefab);
                    leaderGameObject.name = leaderGameObject.name.Replace("(Clone)", "");
                }

                _leaderboardInstanceHolder = leaderGameObject.GetComponent<LeaderboardInstanceHolder>();
                _animation = leaderGameObject.GetComponent<Animation>();
            }
            else
            {
                _leaderboardType =
                    (LeaderboardType)EditorGUILayout.EnumPopup("Select leaderboard type:", _leaderboardType);

                if (_selectedLeaderboardSerializedObject == null || _leaderboardType != _lastLeaderboardType)
                {
                    _selectedLeaderboardSerializedObject = new SerializedObject(_leaderboardInstanceHolder);
                }
                
                string leaderType = "probabilityDistributionLeaderboard";

                switch (_leaderboardType)
                {
                    case LeaderboardType.DynamicScore:
                        leaderType = "dynamicScoreLeaderboard";
                        break;
                    case LeaderboardType.ProbabilityDistribution:
                        leaderType = "probabilityDistributionLeaderboard";
                        _playerMaximumScore = EditorGUILayout.IntField("Player Maximum Score: ", _playerMaximumScore);
                        _minimumBotScore = EditorGUILayout.IntField("Bot minimum Score: ", _minimumBotScore);
                        _maximumBotScore = EditorGUILayout.IntField("Bot Maximum Score: ", _maximumBotScore);
                        _targetWinRatio = EditorGUILayout.Slider("Target Win Ratio", _targetWinRatio, 0f, 1f);
                        break;
                }

                SerializedProperty sp = _selectedLeaderboardSerializedObject.FindProperty(leaderType);
                EditorGUILayout.PropertyField(sp);

                SerializedProperty playerCurveProperty =
                    _selectedLeaderboardSerializedObject.FindProperty("playerAnimationCurve");
                EditorGUILayout.PropertyField(playerCurveProperty);

                for (int i = 0; i < 3; i++)
                {
                    EditorGUILayout.Space();
                }

                _lastLeaderboardType = _leaderboardType;
                _selectedLeaderboardSerializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.BeginVertical();
            {
                _leaderboardInstanceHolder = EditorGUILayout.ObjectField(
                    "Probability Data",
                    _leaderboardInstanceHolder,
                    typeof(LeaderboardInstanceHolder),
                    true
                ) as LeaderboardInstanceHolder;

                _competitionTotalTimeInSeconds =
                    EditorGUILayout.IntField("Competition time (seconds)", _competitionTotalTimeInSeconds);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            
            
            
            //Here the lick code starts
            if (_leaderboardInstanceHolder != null)
            {
                GUI.enabled = true;
                
                SetTime();

                if (GUILayout.Button("Generate Curves"))
                {
                    List<string> allParticipantNames = new List<string>();
                    string playerName = string.Empty;

                    switch (_leaderboardType)
                    {
                        case LeaderboardType.DynamicScore:
                            List<DynamicScoreBotData> allScorebots = _leaderboardInstanceHolder
                                .dynamicScoreLeaderboard
                                .GetLeaderboardData().bots;

                            playerName = _leaderboardInstanceHolder.dynamicScoreLeaderboard
                                .GetLeaderboardData().playerProfile.name;
                            allParticipantNames.Add(playerName);
                            for (int i = 0; i < allScorebots.Count; i++)
                            {
                                allParticipantNames.Add(allScorebots[i].profile.name);
                            }

                            break;
                        case LeaderboardType.ProbabilityDistribution:
                            _leaderboardInstanceHolder.probabilityDistributionLeaderboard.Init(_realStartTime, _minimumBotScore, _maximumBotScore, _targetWinRatio);

                            playerName = _leaderboardInstanceHolder.probabilityDistributionLeaderboard
                                .GetLeaderboardData().playerProfile.name;
                            allParticipantNames.Add(playerName);

                            PseudoBetaDistBotData[] allProbabilityBots =
                                _leaderboardInstanceHolder.probabilityDistributionLeaderboard.GetAllBots;
                            
                            for (int i = 0; i < allProbabilityBots.Length; i++)
                            {
                                allParticipantNames.Add(allProbabilityBots[i].profile.name);
                            }

                            break;
                    }
                    
                    ResetData(allParticipantNames);

                    for (int curveIterator = 0;
                         curveIterator < _leaderboardInstanceHolder.playerAnimationCurve.Count;
                         curveIterator++)
                    {
                        
                        ResetDataBeforeNextPlayerCurveCalculation();

                        AnimationClip animationClip = new AnimationClip
                        {
                            legacy = true
                        };
                        

                        string clipName = $"animationClip_{curveIterator}";
                        animationClip.name = clipName;
                        _animation.AddClip(animationClip, clipName);
                        _animationClipNames.Add(animationClip.name);
                        
                        int totalPlayerCurvePoints = _leaderboardInstanceHolder.playerAnimationCurve[curveIterator].keys.Length;

                        //Run for every point
                        for (int i = 0; i < totalPlayerCurvePoints; i++)
                        {
                            Keyframe playerKeyFrame = _leaderboardInstanceHolder.playerAnimationCurve[curveIterator].keys[i];

                            LeaderboardCalculatedData calculatedData =
                                GenerateCalculatedDataDependingUponPlayerCurve(playerKeyFrame);

                            List<ParticipantData> participantDatas = calculatedData.GetParticipantList();

                            for (int j = 0; j < participantDatas.Count; j++)
                            {
                                ParticipantData participantData = calculatedData.GetParticipantList()[j];

                                Transform selectedBot =
                                    allChildParticipantObjects.Where(t => t.name.Contains(participantData.profile.name))
                                        .ToList()[0].transform;
                                //Create gameobject and set float value
                                GenerateCurves(selectedBot, playerKeyFrame, participantData, animationClip, playerName);
                            }
                        }

                        Selection.activeGameObject = _leaderboardInstanceHolder.gameObject;

                        _animationWindow = GetWindow<AnimationWindow>();
                        _animationWindow.minSize = new Vector2(500, 200);
                    }
                }
            }

            GUILayout.EndScrollView();
        }

        private void ResetDataBeforeNextPlayerCurveCalculation()
        {
            switch (_leaderboardType)
            {
                case LeaderboardType.ProbabilityDistribution:
                    
                    //Use both of these as sometimes Round is using somewhere
                    _leaderboardInstanceHolder.probabilityDistributionLeaderboard.scoresStorage.tournamentsProgresses
                        .RemoveTournament(_realStartTime.ToUnixTime());
                    _leaderboardInstanceHolder.probabilityDistributionLeaderboard.scoresStorage.tournamentsProgresses
                        .RemoveTournament(_realStartTime.Round().ToUnixTime());
                    
                    _leaderboardInstanceHolder.probabilityDistributionLeaderboard.scoresStorage.tournamentsProgresses.Save();

                    break;
            }
        }

        private void ResetData(List<string> allParticipants)
        {
            _leaderboardInstanceHolder.probabilityDistributionLeaderboard.DeleteData();
            _leaderboardInstanceHolder.dynamicScoreLeaderboard.DeleteData();
            
            foreach (string clip in _animationClipNames)
            {
                _animation.RemoveClip(clip);
            }
            
            _animationClipNames.Clear();
            DeleteAnimatorChildGameObjects();
            CreateAnimatorChildGameObjects(allParticipants);
        }

        private void DeleteAnimatorChildGameObjects()
        {
            int animatorChilds = _leaderboardInstanceHolder.transform.childCount;

            for (int i = animatorChilds - 1; i >= 0; i--)
            {
                DestroyImmediate(_leaderboardInstanceHolder.transform.GetChild(i).gameObject);
            }
        }

        private void CreateAnimatorChildGameObjects(List<string> allParticipants)
        {
            allChildParticipantObjects.Clear();

            //Spawn all
            for (int i = 0; i < allParticipants.Count; i++)
            {
                GameObject obj = new GameObject($"{allParticipants[i]}");
                obj.transform.parent = _leaderboardInstanceHolder.transform;
                obj.AddComponent<CurveData>();

                allChildParticipantObjects.Add(obj);
            }
        }

        private void SetTime()
        {
            _realStartTime = DateTime.Now;
            _realEndTime = DateTime.Now.AddSeconds(_competitionTotalTimeInSeconds);
        }

        private LeaderboardCalculatedData GenerateCalculatedDataDependingUponPlayerCurve(Keyframe playerKeyFrame)
        {
            int timeInSeconds = (int)(_competitionTotalTimeInSeconds * playerKeyFrame.time);

            // DateTime calculatedStartTime = _realStartTime.AddSeconds(-moveBackTimeInSeconds);
            // DateTime calculatedEndTime = _realEndTime.AddSeconds(-moveBackTimeInSeconds);
            DateTime currentTime = _realStartTime.AddSeconds(timeInSeconds);
            int playerScore = EvaluatePlayerScoreBasedOnNowTime(playerKeyFrame);

            LeaderboardCalculatedData leaderboardCalculatedData = null;

            switch (_leaderboardType)
            {
                case LeaderboardType.DynamicScore:
                    leaderboardCalculatedData = _leaderboardInstanceHolder.dynamicScoreLeaderboard.CalculatedData(_realStartTime, _realEndTime, currentTime, playerScore);
                    break;
                case LeaderboardType.ProbabilityDistribution:
                    leaderboardCalculatedData = _leaderboardInstanceHolder.probabilityDistributionLeaderboard.CalculatedData(_realStartTime, _realEndTime, currentTime, playerScore);
                    break;
            }

            return leaderboardCalculatedData;
        }

        private int EvaluatePlayerScoreBasedOnNowTime(Keyframe playerKeyFrame)
        {
            float totalScore = 0;
            switch (_leaderboardType)
            {
                case LeaderboardType.DynamicScore:
                    DynamicScoreLeaderboardData data  = _leaderboardInstanceHolder.dynamicScoreLeaderboard.GetLeaderboardData();
                    totalScore = data.totalScore;
                    break;
                case LeaderboardType.ProbabilityDistribution:
                    totalScore = _playerMaximumScore;
                    break;
            }

            return (int)(playerKeyFrame.value * totalScore);
        }

        private void GenerateCurves(
            Transform childTransform, 
            Keyframe playerFrame, 
            ParticipantData participantData, 
            AnimationClip animationClip,
            string playerName)
        {
            string path = AnimationUtility.CalculateTransformPath(childTransform, _leaderboardInstanceHolder.transform);

            EditorCurveBinding binding;

            if (IsPlayer(participantData.profile.name, playerName))
            {
                binding = EditorCurveBinding.FloatCurve(path, typeof(CurveData), "tPlayerCurveData");
            }
            else
            {
                binding = EditorCurveBinding.FloatCurve(path, typeof(CurveData), "curveValue");
            }
            
            AnimationCurve curve = AnimationUtility.GetEditorCurve(animationClip, binding);

            Undo.RecordObject(animationClip, "Record curve value");

            Keyframe key = new Keyframe()
            {
                time = playerFrame.time * _competitionTotalTimeInSeconds,
                value = participantData.score,
            };

            if (curve == null)
            {
                curve = AnimationCurve.Linear(0, 0, 0, 0);
                
                if (IsPlayer(participantData.profile.name, playerName))
                {
                    animationClip.SetCurve(path, typeof(CurveData), "tPlayerCurveData", curve);
                }
                else
                {
                    animationClip.SetCurve(path, typeof(CurveData), "curveValue", curve);
                }

                int isAdded = curve.MoveKey(0, key);
            }
            else
            {
                int isAdded = curve.AddKey(key);
            }

            //Make tangents linear
            for (int i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
            }

            AnimationUtility.SetEditorCurve(animationClip, binding, curve);
        }

        private float GetTimeRatio(DateTime startTime, DateTime endTime)
        {
            return startTime.Second / (float)endTime.Second;
        }

        private bool IsPlayer(string compare1, string compare2)
        {
            return compare1.Contains(compare2);
        }
    }
}