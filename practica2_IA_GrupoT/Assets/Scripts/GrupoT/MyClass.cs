using NavigationDJIA.Interfaces;
using NavigationDJIA.World;
using QMind.Interfaces;
using System;
using UnityEngine;
using QMind;
using Random = UnityEngine.Random;


namespace GrupoT 
{ 

    public class MyClass : IQMindTrainer
    {
        public int CurrentEpisode { get; }
        public int CurrentStep { get; }
        public CellInfo AgentPosition { get; private set; }
        public CellInfo OtherPosition { get; private set; }
        public float Return { get; }
        public float ReturnAveraged { get; }
        public event EventHandler OnEpisodeStarted;
        public event EventHandler OnEpisodeFinished;

        private INavigationAlgorithm _navigationAlgorithm;
        private QMindTrainerParams _qMindTrainerParams;
        private WorldInfo _worldInfo;

        public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            _worldInfo = worldInfo;
            _qMindTrainerParams = qMindTrainerParams;
            _navigationAlgorithm = navigationAlgorithm;
            _navigationAlgorithm.Initialize(_worldInfo);
        

            Debug.Log("QMindTrainerDummy: initialized");
            AgentPosition = worldInfo.RandomCell();
            OtherPosition = worldInfo.RandomCell();
            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
        }

        public void DoStep(bool train)
        {
            int action = Random.Range(0, 4);
            CellInfo newAgentPosition = _worldInfo.NextCell(AgentPosition, _worldInfo.AllowedMovements.FromIntValue(action));
            CellInfo[] path = _navigationAlgorithm.GetPath(OtherPosition, AgentPosition, 1);
            AgentPosition = newAgentPosition;
            Debug.Log("QMindTrainerDummy: DoStep");
        }
    }
}
