using NavigationDJIA.Interfaces;
using NavigationDJIA.World;
using QMind.Interfaces;
using QMind;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace GrupoT
{
    public class QMindTrainer : IQMindTrainer
    {
        public int CurrentEpisode { get; private set; }
        public int CurrentStep { get; private set; }
        public CellInfo AgentPosition { get; private set; }
        public CellInfo OtherPosition { get; private set; }
        public float Return { get; }
        public float ReturnAveraged { get; }
        public event EventHandler OnEpisodeStarted;
        public event EventHandler OnEpisodeFinished;

        private Dictionary<(int, int), float[]> _qTable;

        private static int ACTIONS = 4;
        private const string qTableFile = "QTable.csv";

        private WorldInfo _worldInfo;
        private QMindTrainerParams _params;

        public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            Debug.Log("QMindTrainer: initialized");

            _params = qMindTrainerParams;
            _worldInfo = worldInfo;
            _qTable = new Dictionary<(int, int), float[]>();

            AgentPosition = worldInfo.RandomCell();
            OtherPosition = worldInfo.RandomCell();

            for (int x = 0; x < _worldInfo.WorldSize.x; x++)
            {
                for (int y = 0; y < _worldInfo.WorldSize.y; y++)
                {
                    _qTable[(x, y)] = new float[ACTIONS];
                    for (int i = 0; i < ACTIONS; i++)
                    {
                        _qTable[(x, y)][i] = UnityEngine.Random.Range(0f, 1f);
                    }
                }
            }

            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
            SaveQTable();
        }

        public void DoStep(bool train)
        {
            Debug.Log("QMindTrainer: DoStep");
            CurrentStep++;
            int action = SelectAction(AgentPosition.x, AgentPosition.y);
            CellInfo nextCell = MoveAgent(action);

            float reward = CalculateReward(nextCell);
            UpdateQTable(AgentPosition, action, nextCell, reward);

            AgentPosition = nextCell;

            Debug.Log($"Step: {CurrentStep}, Agent at: ({AgentPosition.x},{AgentPosition.y}), Action: {action}");

            if (AgentPosition == OtherPosition)
            {
                Debug.Log($"Agent caught! Episode: {CurrentEpisode}, Steps: {CurrentStep}");
                OnEpisodeFinished?.Invoke(this, EventArgs.Empty);
                CurrentEpisode++;
                ResetEpisode();

                if (CurrentEpisode % _params.episodesBetweenSaves == 0)
                {
                    SaveQTable();
                }
            }
        }

        private int SelectAction(int x, int y)
        {
            if (UnityEngine.Random.value < _params.epsilon)
            {
                return UnityEngine.Random.Range(0, ACTIONS);
            }
            else
            {
                return Array.IndexOf(_qTable[(x, y)], Mathf.Max(_qTable[(x, y)]));
            }
        }

        private CellInfo MoveAgent(int action)
        {
            Directions direction = (Directions)action;
            CellInfo nextCell = _worldInfo.NextCell(AgentPosition, direction);

            return nextCell.Walkable ? nextCell : AgentPosition;
        }

        private float CalculateReward(CellInfo nextCell)
        {
            if (nextCell == OtherPosition)
            {
                return -100f;
            }
            if (nextCell.Type == CellInfo.CellType.Exit)
            {
                return 100f;
            }
            if (!nextCell.Walkable)
            {
                return -100f;
            }
            return -1f;
        }

        private void UpdateQTable(CellInfo state, int action, CellInfo nextCell, float reward)
        {
            float[] nextQValues = _qTable[(nextCell.x, nextCell.y)];
            float maxNextQ = Mathf.Max(nextQValues);
            float oldQ = _qTable[(state.x, state.y)][action];

            Debug.Log($"Q-Table Updated: State ({state.x},{state.y}), Action: {action}, Q-Value: {_qTable[(state.x, state.y)][action]}");

            _qTable[(state.x, state.y)][action] = oldQ + _params.alpha * (reward + _params.gamma * maxNextQ - oldQ);
        }

        private void ResetEpisode()
        {
            AgentPosition = _worldInfo.RandomCell();
            OtherPosition = _worldInfo.RandomCell();
            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
        }

        private void SaveQTable()
        {
            string path = Path.Combine(Application.persistentDataPath, qTableFile);
            using (StreamWriter writer = new StreamWriter(path))
            {
                foreach (var entry in _qTable)
                {
                    string line = $"{entry.Key.Item1},{entry.Key.Item2}";
                    foreach (float value in entry.Value) 
                    {
                        line += $",{value}";
                    }
                    writer.WriteLine(line);
                }
            }

            Debug.Log($"Q-Table saved to: {path}");
        }
    }
}