using NavigationDJIA.Interfaces;
using NavigationDJIA.World;
using QMind;
using QMind.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Rendering;

namespace GrupoT
{
    public class QTrainer : IQMindTrainer
    {
        public int CurrentEpisode { get; set; }
        public int CurrentStep { get; set; }
        public CellInfo AgentPosition { get; private set; }
        public CellInfo OtherPosition { get; private set; }
        public float Return { get; set; }
        public float ReturnAveraged { get; set; }
        public event EventHandler OnEpisodeStarted;
        public event EventHandler OnEpisodeFinished;

        private QTable _qTable;
        private QMindTrainerParams _params;
        private WorldInfo _worldInfo;
        private INavigationAlgorithm _navigationAlgorithm;

        public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            _params = qMindTrainerParams;
            _worldInfo = worldInfo;
            _navigationAlgorithm = navigationAlgorithm;
            _navigationAlgorithm.Initialize(worldInfo);

            int possibleActions = 4;
            int possibleStates = 16 * 9 * 18;
            _qTable = new QTable(possibleActions, possibleStates);

            AgentPosition = worldInfo.RandomCell();
            OtherPosition = worldInfo.RandomCell();

            CurrentEpisode = 0;
            CurrentStep = 0;
            Return = 0;
            ReturnAveraged = 0;

            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);

            Debug.Log("QMindTrainerDummy: initialized");
        }

        public void DoStep(bool train)
        {
            Debug.Log("QMindTrainerDummy: DoStep");

            CellInfo nextCell;
            int action;

            do
            {
                action = ExplorationStrat() ? UnityEngine.Random.Range(0, 4) : _qTable.GetBestAction(GetCurrentStateId());

                nextCell = _worldInfo.NextCell(AgentPosition, GetDirection(action));
                if(!nextCell.Walkable)
                {
                    float currentQ = _qTable.GetQValue(GetCurrentStateId(), action);
                    float newQ = UpdateQ(currentQ, -999);
                    _qTable.UpdateQValue(GetCurrentStateId(), action, newQ);
                }
            } while (!nextCell.Walkable);

            AgentPosition = nextCell;
            float reward = CalculateReward(nextCell);

            if(train)
            {
                float currentQ = _qTable.GetQValue(GetCurrentStateId(), action);
                float newQ = UpdateQ(currentQ, reward);
                _qTable.UpdateQValue(GetCurrentStateId(), action, newQ);
            }

            CellInfo[] path = _navigationAlgorithm.GetPath(OtherPosition, AgentPosition, 1);
            if (path != null && path.Length > 0)
            {
                // Update the Player's position to the next step in the path
                OtherPosition = path[0];

                Debug.Log($"Player moved to {OtherPosition.x}, {OtherPosition.y}");
            }
            else
            {
                Debug.Log("No valid path found for Player.");
            }
            

            if (OtherPosition == null || CurrentStep == _params.maxSteps || OtherPosition == AgentPosition)
            {
                OnEpisodeFinished?.Invoke(this, EventArgs.Empty);
                NewEpisode();
            }
            else
            {
                CurrentStep++;
            }

            //Debug.Log($"Agent moved to {AgentPosition.x}, {AgentPosition.y} using action {action}");
        }

        private void NewEpisode()
        {
            CurrentEpisode++;

            AgentPosition = _worldInfo.RandomCell();
            OtherPosition = _worldInfo.RandomCell();

            CurrentStep = 0;
            Return = 0;

            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);

            Debug.Log($"New episode started. Episode: {CurrentEpisode}, Agent: ({AgentPosition.x}, {AgentPosition.y}), Player: ({OtherPosition.x}, {OtherPosition.y})");
        }

        public bool ExplorationStrat()
        {
            float random = UnityEngine.Random.Range(0.0f, 1.0f);
            return random <= _params.epsilon;
        }

        private Directions GetDirection(int action)
        {
            switch(action)
            {
                case 0: return Directions.Up;
                case 1: return Directions.Right;
                case 2: return Directions.Down;
                case 3: return Directions.Left;
                default: throw new ArgumentOutOfRangeException("Invalid action.");
            }
        }

        public float UpdateQ(float currentQ, float reward)
        {
            float bestNextQ = _qTable.GetBestQ(GetCurrentStateId());
            return (1 - _params.alpha) * currentQ + _params.alpha * (reward + _params.gamma * bestNextQ);
        }

        private int GetCurrentStateId()
        {
            bool nIsWall = !_worldInfo.NextCell(AgentPosition, Directions.Up).Walkable;
            bool sIsWall = !_worldInfo.NextCell(AgentPosition, Directions.Down).Walkable;
            bool eIsWall = !_worldInfo.NextCell(AgentPosition, Directions.Right).Walkable;
            bool wIsWall = !_worldInfo.NextCell(AgentPosition, Directions.Left).Walkable;

            float manhattan = AgentPosition.Distance(OtherPosition, CellInfo.DistanceType.Manhattan);

            int position = CalculatePosition();

            Debug.Log($"Current State: N={nIsWall}, S={sIsWall}, E={eIsWall}, W={wIsWall}, Dist={manhattan}, Pos={position}");

            foreach(State state in _qTable.GetAllStates())
            {
                if(state.NorthIsWall == nIsWall &&  state.SouthIsWall == sIsWall && state.EastIsWall == eIsWall && 
                    state.WestIsWall == wIsWall && Mathf.Approximately(state.ManhattanDistanceToPlayer, manhattan) &&
                    state.PositionToPlayer == position)
                {
                    return state.stateId;
                }
            }

            throw new Exception("Current state does not match any precomputed state.");
        }

        private int CalculatePosition()
        {
            if (OtherPosition.x < AgentPosition.x) return 1;
            if (OtherPosition.x > AgentPosition.x) return 2;
            if (OtherPosition.y > AgentPosition.y) return 3;
            if (OtherPosition.y < AgentPosition.y) return 4;
            return 0;
        }

        private float CalculateReward(CellInfo nextCell)
        {
            float iDistance = AgentPosition.Distance(OtherPosition, CellInfo.DistanceType.Manhattan);
            float fDistance = nextCell.Distance(OtherPosition, CellInfo.DistanceType.Manhattan);

            if (nextCell == OtherPosition) return -100;
            if (fDistance > iDistance) return 10;
            return -1;
        }
    }
}
