using NavigationDJIA.World;
using QMind.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GrupoT
{
    public class QTester : IQMind
    {

        private QTable _qTable;
        private WorldInfo _worldInfo;

        public void Initialize(WorldInfo worldInfo)
        {
            Debug.Log("QMind: initialized");
            _worldInfo = worldInfo;
            _qTable = new QTable(4, 16 * 9 * 38);
            LoadQTable(@"Assets/Scripts/GrupoT/QTable.csv");
        }

        public void LoadQTable(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"QTable not found {filePath}");
                return;
            }

            string[] lines = File.ReadAllLines(filePath);
            for (int i = 1; i < lines.Length; i++)
            {

                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] values = line.Split(',');

                if (values.Length < 11)
                {
                    Debug.LogError($"Malformed line: {line}");
                }

                try
                {

                    int stateId = int.Parse(values[0]);

                    if (stateId >= 16 * 5 * 39)
                    {
                        Debug.LogError($"Invalid stateId: {stateId}. Skipping line: {line}");
                        continue;
                    }

                    for (int action = 0; action < 4; action++)
                    {
                        float qValue = float.Parse(values[7 + action]);
                        _qTable.UpdateQValue(stateId, action, qValue);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error parsing line: {line}. Exception: {ex.Message}");
                }
            }

            Debug.Log($"QTable loaded. {_qTable.GetBestQ(87)}");
        }

        public CellInfo GetNextStep(CellInfo currentPosition, CellInfo otherPosition)
        {
            try
            {
                bool northIsWall = !_worldInfo.NextCell(currentPosition, Directions.Up).Walkable;
                bool southIsWall = !_worldInfo.NextCell(currentPosition, Directions.Down).Walkable;
                bool eastIsWall = !_worldInfo.NextCell(currentPosition, Directions.Right).Walkable;
                bool westIsWall = !_worldInfo.NextCell(currentPosition, Directions.Left).Walkable;

                float manhattanDistance = currentPosition.Distance(otherPosition, CellInfo.DistanceType.Manhattan);
                int positionToPlayer = CalculatePosition(currentPosition, otherPosition);

                State currentState = new State(northIsWall, southIsWall, eastIsWall, westIsWall, manhattanDistance, positionToPlayer, -1);

                Debug.Log($"Current state: N={currentState.NorthIsWall}, S={currentState.SouthIsWall}, E={currentState.EastIsWall}, W={currentState.WestIsWall}, Dist={currentState.ManhattanDistanceToPlayer}, Pos={currentState.PositionToPlayer}");
                int stateId = GetStateId(currentState);
                Debug.Log($"State id: {stateId}");
                Debug.Log($"Best Q:{_qTable.GetBestQ(stateId)}");

                float bestQ = _qTable.GetBestQ(stateId);

                int bestAction = _qTable.GetBestAction(stateId);

                Debug.Log($"Best action: {bestAction}");

                Directions direction = GetDirection(bestAction);
                return _worldInfo.NextCell(currentPosition, direction);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in GetNextStep: {ex.Message}");
                return null;
            }
        }

        private int CalculatePosition(CellInfo currentPosition, CellInfo otherPosition)
        {
            if (otherPosition.x < currentPosition.x) return 1;
            if (otherPosition.x > currentPosition.x) return 2;
            if (otherPosition.y > currentPosition.y) return 3;
            if (otherPosition.y < currentPosition.y) return 4;
            return 0;
        }

        private int GetStateId(State currentState)
        {
            foreach (State state in _qTable.GetAllStates())
            {
                if(state.NorthIsWall == currentState.NorthIsWall && state.SouthIsWall == currentState.SouthIsWall &&
                    state.EastIsWall == currentState.EastIsWall && state.WestIsWall == currentState.WestIsWall &&
                    Mathf.Approximately(state.ManhattanDistanceToPlayer, currentState.ManhattanDistanceToPlayer) &&
                    state.PositionToPlayer == currentState.PositionToPlayer)
                {
                    return state.stateId;
                }
            }

            throw new Exception($"State not found: N={currentState.NorthIsWall}, S={currentState.SouthIsWall}, E={currentState.EastIsWall}, W={currentState.WestIsWall}, Dist={currentState.ManhattanDistanceToPlayer}, Pos={currentState.PositionToPlayer}");
        }

        private Directions GetDirection(int action)
        {
            return action switch
            {
                0 => Directions.Up,
                1 => Directions.Right,
                2 => Directions.Down,
                3 => Directions.Left,
                _ => throw new ArgumentOutOfRangeException("Invalid action.")
            };
        }
    }
}