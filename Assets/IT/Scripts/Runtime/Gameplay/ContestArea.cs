using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using IT.Data.Gameplay;
using IT.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;

namespace IT.Gameplay
{
    public class ContestArea : NetworkBehaviour
    {
        [SerializeField] private float _timeToContest;
        
        [SerializeField, ReadOnly] private float _currentContestTime;
        private List<ContestAreaPlayerData> _contestAreaPlayersData;
        private IPointCounter _currentContestPlayer;
        public override void OnStartServer()
        {
            base.OnStartServer();
            TimeManager.OnTick += OnTick;
            TimeManager.OnPostTick += OnPostTick;
            
            _contestAreaPlayersData = new List<ContestAreaPlayerData>();
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            TimeManager.OnTick -= OnTick;
            TimeManager.OnPostTick -= OnPostTick;
        }

        private void OnTriggerStay(Collider other)
        {
            if(!IsServer)
                return;
            
            if(!other.TryGetComponent(out IPointCounter pointCounter))
                return;

            if (ExistInContestList(pointCounter, out ContestAreaPlayerData data))
            {
                data.WasContestingPastTick = true;
                return;
            }
            
            AddNewContestData(pointCounter);
        }

        private void OnPostTick()
        {
            ClearContestList();
            FlagData();
        }

        private void OnTick()
        {
            TryAssignCurrentContestPlayer();
            TryToIncrement();
            TryToDecrement();
            TryGivePoints();
        }

        private void FlagData()
        {
            foreach (ContestAreaPlayerData data in _contestAreaPlayersData)
            {
                data.WasContestingPastTick = false;
            }
        }
        
        private void ClearContestList()
        {
            if(_contestAreaPlayersData.Count == 0)
                return;

            _contestAreaPlayersData.RemoveAll(predicate => !predicate.WasContestingPastTick);
        }

        private void AddNewContestData(IPointCounter pointCounter)
        {
            ContestAreaPlayerData data = new ContestAreaPlayerData
            {
                PointCounter = pointCounter,
                WasContestingPastTick = true
            };
            
            _contestAreaPlayersData.Add(data);
        }

        private bool ExistInContestList(IPointCounter pointCounter, out ContestAreaPlayerData data)
        {
            data = default;
            bool exist = false;
                
            foreach (ContestAreaPlayerData contestData in _contestAreaPlayersData)
            {
                if (contestData.PointCounter != pointCounter) continue;
                data = contestData;
                exist = true;
                break;
            }

            return exist;
        }
        
        private void TryGivePoints()
        {
            if(_currentContestTime < _timeToContest)
                return;
            
            if(_contestAreaPlayersData.Count is 0 or > 1)
                return;
            
            if(_currentContestPlayer == null || _currentContestPlayer != _contestAreaPlayersData[0].PointCounter)
                return;
            
            _currentContestPlayer.GainPoints();
        }

        private void TryToIncrement()
        {
            if(_contestAreaPlayersData.Count is 0 or > 1)
                return;
            
            if(_currentContestPlayer == null || _currentContestPlayer != _contestAreaPlayersData[0].PointCounter)
                return;
            
            if(_currentContestTime >= _timeToContest)
                return;

            _currentContestTime = Mathf.MoveTowards(_currentContestTime, _timeToContest, (float)TimeManager.TickDelta);
        }

        private void TryToDecrement()
        {
            if(_contestAreaPlayersData.Count > 1)
                return;
            
            if(_contestAreaPlayersData.Count == 1 && _currentContestPlayer == _contestAreaPlayersData[0].PointCounter)
                return;
            
            _currentContestTime = Mathf.MoveTowards(_currentContestTime, 0, (float)TimeManager.TickDelta);

            if (_currentContestTime <= 0 && _currentContestPlayer != null)
                _currentContestPlayer = null;
        }
        
        private void TryAssignCurrentContestPlayer()
        {
            if(_contestAreaPlayersData.Count > 1 || _currentContestTime > 0)
                return;
            
            if(_contestAreaPlayersData.Count == 0)
                return;

            _currentContestPlayer = _contestAreaPlayersData[0].PointCounter;
        }

        public void ResetTimer()
        {
            _contestAreaPlayersData.Clear();
            _currentContestTime = 0;
        }
    }
}
