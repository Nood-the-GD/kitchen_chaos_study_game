using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    #region Events
    public event EventHandler OnStateChanged;
    public event EventHandler OnGamePause;
    public event EventHandler OnGameUnPause;
    #endregion

    #region Instance
    public static GameManager Instance { get; private set; }
    #endregion

    #region Enum
    private enum State
    {
        Tutorial,
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver
    }
    #endregion

    #region Variables
    public bool isTesting;
    [SerializeField] private float gamePlayingTimerMax = 10f;
    private float gamePlayingTimer = 100;
    private State state;
    private float waitingToStartTimer = 1f;
    private float countdownToStartTimer = 3f;
    private bool isGamePause = false;
    public List<Transform> spawnPoints = new List<Transform>();
    public static int levelId;
    public static StageData getStageData
    {
        get
        {
            return GameData.s.GetStage(levelId);
        }
    }
    bool isGameOver = false;
    #endregion

    #region Unity events
    protected override void Awake()
    {
        base.Awake();
        Application.targetFrameRate = 60;
        state = State.Tutorial;
    }
    protected override void Start()
    {
        base.Start();
        if (isTesting) return;
        if (SectionData.s.isSinglePlay)
        {
            // // Spawn AIManager
            // AISystem aISystem = Instantiate(Resources.Load<AISystem>(FAKE_PLAYER_PATH)).GetComponent<AISystem>();
            // Transform spawnPosition = spawnPoints[0];
            // aISystem.Init(spawnPosition.position);
            // spawnPoints.Remove(spawnPosition);
            SpawnSinglePlayerManager();
        }
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        TutorialUI.OnTutorialComplete += TutorialUI_OnTutorialComplete;
        OnJoinRoom();
    }
    private void Update()
    {
        if (isGameOver) return;

        if (!PhotonManager.s.isJoinedRoom) return;

        switch (state)
        {
            case State.Tutorial:
                return;
            case State.WaitingToStart:
                waitingToStartTimer -= Time.deltaTime;
                if (waitingToStartTimer < 0f)
                {
                    ChangeState(State.CountdownToStart);
                }
                break;
            case State.CountdownToStart:
                countdownToStartTimer -= Time.deltaTime;
                if (countdownToStartTimer < 0f)
                {
                    gamePlayingTimer = gamePlayingTimerMax;
                    ChangeState(State.GamePlaying);
                }
                break;
            case State.GamePlaying:
                gamePlayingTimer -= Time.deltaTime;
                if (gamePlayingTimer < 0f)
                {
                    isGameOver = true;
                    ChangeState(State.GameOver);
                    PhotonManager.s.RPCEndGame();
                }
                break;
            case State.GameOver:

                break;
        }
    }
    #endregion

    #region Event functions
    private void TutorialUI_OnTutorialComplete()
    {
        state = State.WaitingToStart;
    }
    void OnJoinRoom()
    {
        Debug.Log("OnJoinRoom");
        var id = PhotonManager.s.myPlayerPhoton.ActorNumber;
        Transform spawnPosition = spawnPoints[0];
        var ob = ObjectEnum.MainPlayer.SpawnMultiplay(spawnPosition.position, Quaternion.identity);
        ob.name = "MainPlayer_" + id;
        spawnPoints.Remove(spawnPosition);
    }
    private void GameInput_OnPauseAction(object sender, System.EventArgs e)
    {
        PauseGame();
    }
    #endregion

    #region private functions
    private SinglePlayManager SpawnSinglePlayerManager()
    {
        var singlePlayOb = new GameObject("SinglePlayManager");
        return singlePlayOb.AddComponent<SinglePlayManager>();
    }
    private void ChangeState(State state)
    {
        this.state = state;
        this.OnStateChanged?.Invoke(this, EventArgs.Empty);
    }
    #endregion

    #region Support
    public float GetCountdownToStartTimer()
    {
        return countdownToStartTimer;
    }
    public float GetPlayingTimerNormalized()
    {
        return 1 - (gamePlayingTimer / gamePlayingTimerMax);
    }
    public bool IsGamePlaying()
    {
        return state == State.GamePlaying;
    }
    public bool IsCountdownToStartActive()
    {
        return state == State.CountdownToStart;
    }
    public bool IsGameOver()
    {
        return state == State.GameOver;
    }
    public void PauseGame()
    {
        isGamePause = !isGamePause;
        if (isGamePause)
        {
            Time.timeScale = 0f;
            OnGamePause?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1f;
            OnGameUnPause?.Invoke(this, EventArgs.Empty);
        }
    }
    #endregion
}
