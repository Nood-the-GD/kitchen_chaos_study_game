using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinglePlayManager : Singleton<SinglePlayManager>
{
    const string FAKE_PLAYER_PATH = "Prefabs/FakePlayer";
    private Player _fakePlayer;
    private Player _mainPlayer;
    private Player _currentPlayer;

    protected override void Awake()
    {
        Player.OnPlayerSpawn += Player_OnPlayerSpawnHandler;
    }
    private void OnDestroy()
    {
        GameInput.Instance.OnChangeCharacterAction -= GameInput_OnChangeCharacterActionHandler;
    }

    #region Event handlers
    private void Player_OnPlayerSpawnHandler(Player player)
    {
        if (SectionData.s.isSinglePlay)
        {
            SetMainPlayer(player);
            Init();
        }
    }
    private void GameInput_OnChangeCharacterActionHandler(object sender, EventArgs e)
    {
        ChangePlayer();
        ReassignCamera();
    }
    #endregion

    public void Init()
    {
        //Spawn fake player
        SpawnFakePlayer();
        GameInput.Instance.OnChangeCharacterAction += GameInput_OnChangeCharacterActionHandler;
    }

    #region private functions
    private void SetMainPlayer(Player player)
    {
        _mainPlayer = player;
        _mainPlayer.IsControlling = true;
        _currentPlayer = _mainPlayer;
    }

    private void ReassignCamera()
    {
        if (_currentPlayer == _mainPlayer)
        {
            CameraController.Instance.SetFollowTarget(_mainPlayer.transform);
            CameraController.Instance.SetLookAtTarget(_mainPlayer.transform);
        }
        else
        {
            CameraController.Instance.SetFollowTarget(_fakePlayer.transform);
            CameraController.Instance.SetLookAtTarget(_fakePlayer.transform);
        }
    }
    private void ChangePlayer()
    {
        _currentPlayer.IsControlling = false;
        _currentPlayer.UnregisterGameInputEvent();
        if (_currentPlayer == _mainPlayer)
        {
            _currentPlayer = _fakePlayer;
        }
        else
        {
            _currentPlayer = _mainPlayer;
        }
        _currentPlayer.IsControlling = true;
        _currentPlayer.RegisterGameInputEvent();
    }
    private void SpawnFakePlayer()

    {
        //Spawn fake player
        Transform spawnPosition = GameManager.s.spawnPoints[0];
        var ob = Instantiate(Resources.Load<GameObject>(FAKE_PLAYER_PATH));
        ob.transform.position = spawnPosition.position;
        _fakePlayer = ob.GetComponent<Player>();
        _fakePlayer.playerAnimator.SetPlayerName("FakePlayer");
        _fakePlayer.UnregisterGameInputEvent();
        _fakePlayer.IsControlling = false;
        GameManager.s.spawnPoints.Remove(spawnPosition);
    }
    #endregion
}
