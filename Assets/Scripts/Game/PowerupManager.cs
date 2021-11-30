using System;
using System.Collections.Generic;
using Game.Configs;
using Photon.Pun;
using UnityEngine;
using View.Components;
using Random = UnityEngine.Random;

namespace Game
{
    public class PowerupManager
    {

        private readonly GameConfig _config;
        

        public PowerupManager(GameConfig config)
        {
            _config = config;
        }

        public PowerupController CreatePowerup(Vector3 point, Quaternion rotation)
        {
            var powerup = PhotonNetwork.Instantiate(_config.PowerupPrefab.Path, point, rotation)
                .GetComponent<PowerupController>();
            powerup.TargetReachedEvent += OnPowerupPicked;
            return powerup;
        }
        

        private void OnPowerupPicked(PowerupController powerupController, PlayerController target)
        {
            Remove(powerupController);
           
        }

        private void Remove(PowerupController powerupController)
        {
            powerupController.TargetReachedEvent -= OnPowerupPicked;
            PhotonNetwork.Destroy(powerupController.gameObject);
        }
    }
}