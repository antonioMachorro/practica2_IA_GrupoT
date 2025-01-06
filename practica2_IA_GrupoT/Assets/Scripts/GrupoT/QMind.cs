using NavigationDJIA.World;
using QMind.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using QMind;

namespace GrupoT
{
    public class QMind : MonoBehaviour, IQMind
    {

        private Dictionary<(int, int), float[]> _qTable;
        private WorldInfo _worldInfo;
        private QMindTrainer _trainer;
        private static int ACTIONS = 4;
        private const string qTableFile = "QTable.csv";
        public bool train = false;

        public void Initialize(WorldInfo worldInfo)
        {
            Debug.Log("QMind: initialized");
            Debug.Log("TRAINING MODE: " + train);
            _worldInfo = worldInfo;
            _qTable = new Dictionary<(int, int), float[]>();

            _trainer = new QMindTrainer();
            _trainer.Initialize(new QMindTrainerParams(), worldInfo, new NavigationDJIA.Algorithms.AStar.AStarNavigation());

            LoadQTable();
        }

        private void LoadQTable()
        {
            string path = Path.Combine(Application.persistentDataPath, qTableFile);
            if (!File.Exists(path))
            {
                Debug.LogWarning("Q-Table not found.");
                train = true;

                return;
            }

            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
            {
                string[] parts = line.Split(',');
                int x = int.Parse(parts[0]);
                int y = int.Parse(parts[1]);
                float[] qValues = new float[ACTIONS];

                for (int i = 0; i < ACTIONS; i++)
                {
                    qValues[i] = float.Parse(parts[i + 2]);
                }

                _qTable[(x, y)] = qValues;
            }

            Debug.Log("Q-Table loaded.");
        }

        public CellInfo GetNextStep(CellInfo currentPosition, CellInfo otherPosition)
        {
            Debug.Log("QMind: GetNextStep");

            if(train)
            {
                _trainer.DoStep(true);
                return _trainer.AgentPosition;
            }

            if(!_qTable.ContainsKey((currentPosition.x, currentPosition.y)))
            {
                Debug.LogWarning($"No entry for position: ({currentPosition.x}, {currentPosition.y}). Random step.");
                return GetRandomNeighbor(currentPosition);
            }

            int bestAction = SelectBestAction(currentPosition.x, currentPosition.y);
            CellInfo nextCell = _worldInfo.NextCell(currentPosition, (Directions)bestAction);

            if (!nextCell.Walkable)
            {
                Debug.LogWarning($"Invalid move to: ({nextCell.x}, {nextCell.y}). Penalty applied.");

                // Penalize Q-value for invalid action
                _qTable[(currentPosition.x, currentPosition.y)][bestAction] -= 5f;

                // Force random movement to break the loop
                return GetRandomNeighbor(currentPosition);
            }

            Debug.Log($"Moving to: ({nextCell.x}, {nextCell.y}) from ({currentPosition.x}, {currentPosition.y}).");

            return nextCell.Walkable ? nextCell : currentPosition;
        }

        private CellInfo GetRandomNeighbor(CellInfo currentPosition)
        {
            List<CellInfo> neighbors = new List<CellInfo>();

            for (int i = 0; i < ACTIONS; i++)
            {
                CellInfo nextCell = _worldInfo.NextCell(currentPosition, (Directions)i);
                if(nextCell.Walkable)
                {
                    neighbors.Add(nextCell);
                }
            }

            if (neighbors.Count == 0)
            {
                Debug.LogWarning("No neighbors found.");
                return currentPosition;
            }

            return neighbors[UnityEngine.Random.Range(0, neighbors.Count)];
        }

        private int SelectBestAction(int x, int y)
        {
            float[] qValues = _qTable[(x, y)];
            int bestAction = Array.IndexOf(qValues, Mathf.Max(qValues));
            return bestAction;
        }
    }
}