using Game.Configs;
using Photon.Pun;
using UnityEngine;
using View;
using View.Components;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        [Header("Views")] [SerializeField] private GameView _view;
        [SerializeField] private GameplayView _gameplayView;

        [Header("Managers")] [SerializeField] private NetworkManager _networkManager;

        [Header("Configs")] [SerializeField] private GameConfig _config;
        [SerializeField] private NetworkEvents _networkEvents;

        private BulletManager _bulletManager;
        private PlayersManager _playersManager;
        private PowerupManager _powerupManager;

        public void OnEnable()
        {
            _view.CreateGameClickEvent += OnCreateGameClick;
            _view.FindRandomGameEvent += OnFindRandomGame;
            _view.StartGameEvent += OnStartGame;
            _networkManager.RoomJoinEvent += OnRoomJoin;
            _networkManager.ConnectedEvent += OnConnected;
            _networkManager.ModelChangedEvent += OnModelChanged;


            _bulletManager = new BulletManager(_config);
            _powerupManager = new PowerupManager(_config);
            _playersManager = new PlayersManager(_networkManager, _bulletManager, _powerupManager, _networkEvents,
                _config, _gameplayView);

            _view.SetLoadingState(true);
            _view.SetSettingsState(false);
            _view.SetError(string.Empty);
            _networkManager.Connect();
        }

        private void OnStartGame()
        {
            SetupPowerups();
            _playersManager.SetRandomFireman();
            _networkManager.StartGame();
        }

        private void SetupPowerups()
        {
            var powerupController1 = _powerupManager.CreatePowerup(new Vector3(0, 1, 2), Quaternion.identity);
            powerupController1.TargetReachedEvent += (controller, playerController) =>
            {
                Heal(playerController);
            };
            var powerupController2 = _powerupManager.CreatePowerup(new Vector3(-1, 1, -2),
                Quaternion.identity);
            powerupController2.TargetReachedEvent += (controller, playerController) =>
            {
                Shrink(playerController);
            };
        }

        private void Shrink( PlayerController playerController)
        {
            _networkManager.ChangePlayer(playerController.Id,Change.SCALE);
            
        }

        private void Heal(PlayerController playerController)
        {
            _networkManager.ChangePlayer(playerController.Id,Change.HEALTH);
           
        }

        private void Update()
        {
            _bulletManager.Tick(Time.deltaTime);
            _playersManager.Tick(Time.deltaTime);
        }

        private void OnModelChanged()
        {
            switch (_networkManager.GameState)
            {
                case GameState.Play:
                    _view.SetStartState(false, false);
                    break;

                case GameState.End:
                    _view.SetWinState(true, !_playersManager.IsFireman);
                    break;
            }
        }

        private void OnConnected()
        {
            _view.SetLoadingState(false);
            _view.SetSettingsState(true);
        }

        private void OnRoomJoin(bool ok, string error)
        {
            _view.SetLoadingState(false);
            _view.SetSettingsState(false);
            if (ok)
            {
                _playersManager.CreateLocalPlayer();

                _view.SetStartState(true, _networkManager.IsMaster);
            }
            else
            {
                _view.SetSettingsState(true);
                _view.SetError(error);
            }
        }

        public void OnDisable()
        {
            _view.CreateGameClickEvent -= OnCreateGameClick;
            _view.FindRandomGameEvent -= OnFindRandomGame;
            _networkManager.RoomJoinEvent -= OnRoomJoin;
            _networkManager.ConnectedEvent -= OnConnected;

            _playersManager.Release();
        }

        private void OnCreateGameClick()
        {
            SaveSettings();
            _view.SetLoadingState(true);
            _view.SetError(string.Empty);
            _networkManager.CreateGame(_view.RoomName);
        }

        private void OnFindRandomGame()
        {
            SaveSettings();
            _view.SetLoadingState(true);
            _view.SetError(string.Empty);
            _networkManager.FindRandomRoom();
        }

        private void SaveSettings()
        {
            PlayerPrefs.SetString("PlayerName", _view.PlayerName);
        }
    }
}