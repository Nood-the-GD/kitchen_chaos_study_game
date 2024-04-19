using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Events
    public event EventHandler OnStateChanged;
    public event EventHandler OnGamePause;
    public event EventHandler OnGameUnPause;
    #endregion

    #region Instance
    public static GameManager Instance {get; private set;}
    #endregion

    #region Enum
    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver
    }
    #endregion

    #region Variables
    [SerializeField] private float gamePlayingTimerMax = 10f;
    private float gamePlayingTimer = 100;
    private State state;
    private float waitingToStartTimer = 1f;
    private float countdownToStartTimer = 3f;
    private bool isGamePause = false;
    public Transform[] spawnPoints;
    public static int levelId;
    bool isGameOver = false;
    public static StageData getStageData{
        get{
            return GameData.s.GetStage(levelId);
        }
    }
    #endregion

    #region Unity events
    private void Awake()
    {
        Application.targetFrameRate = 60;
        Instance = this;
        state = State.WaitingToStart;

    }
    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        OnJoinRoom();
    }
    

    private void Update()
    {
        if(isGameOver) return;

        if(!PhotonManager.s.isJoinedRoom) return;

        switch(state)
        {
            case State.WaitingToStart:
                waitingToStartTimer -= Time.deltaTime;
                if(waitingToStartTimer < 0f)
                {
                    ChangeState(State.CountdownToStart);
                }
                break;
            case State.CountdownToStart:
                countdownToStartTimer -= Time.deltaTime;
                if(countdownToStartTimer < 0f)
                {
                    gamePlayingTimer = gamePlayingTimerMax;
                    ChangeState(State.GamePlaying);
                }
                break;
            case State.GamePlaying:
                gamePlayingTimer -= Time.deltaTime;
                isGameOver = true;
                if(gamePlayingTimer < 0f)
                {
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
    void OnJoinRoom(){
        Debug.Log("OnJoinRoom");
        var id = PhotonManager.s.myPlayerPhoton.ActorNumber;
        var ob = ObjectEnum.MainPlayer.SpawnMultiplay(spawnPoints[0].position, Quaternion.identity);
        ob.name = "MainPlayer_"+id;
    }
    private void GameInput_OnPauseAction(object sender, System.EventArgs e)
    {
        PauseGame();
}
    #endregion

    #region Support
    private void ChangeState(State state)
    {
        this.state = state;
        this.OnStateChanged?.Invoke(this, EventArgs.Empty);
    }
    public float GetCountdownToStartTimer()
    {
        return countdownToStartTimer;
    }
    public float GetPlayingTimerNormalized()
    {
        return 1 - (gamePlayingTimer/gamePlayingTimerMax);
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
        if(isGamePause)
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
