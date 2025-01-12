using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QTable
{
    private int _actionNum;
    private int _stateNum;
    private float[,] _qTable;
    private List<State> _stateList;

    public QTable(int actions, int states) 
    {
        _actionNum = actions;
        _stateNum = states;
        _qTable = new float[_stateNum, _actionNum];
        _stateList = new List<State>();

        InitializeStates();
        InitializeQTable();
    }

    public void InitializeStates()
    {
        int stateId = 0;
        for (int n = 0; n <= 1; n++)
        {
            for (int s = 0; s <= 1; s++)
            {
                for (int e = 0; e <= 1; e++)
                {
                    for (int w = 0; w <= 1; w++)
                    {
                        for (int pos = 0; pos <= 4; pos++)
                        {
                            for (int dist = 0; dist <= 38; dist++)
                            {
                                State state = new State(n == 1, s == 1, e == 1, w == 1, dist, pos, stateId);
                                AddState(state);
                                stateId++;
                            }
                        }
                    }
                }
            }
        }

        Debug.Log($"Initialized {_stateList.Count} states.");
    }

    public void InitializeQTable()
    {
        for (int i = 0; i < _stateNum; i++)
        {
            for (int j = 0; j < _actionNum; j++) 
            {
                _qTable[i, j] = 0;
            }
        }
    }

    public void AddState(State state)
    {
        if (state.stateId != _stateList.Count)
        {
            throw new InvalidOperationException($"StateId {state.stateId} does not match the current index {_stateList.Count}.");
        }

        _stateList.Add(state);
    }

    public int GetStateIndex(int stateId)
    {
        if (stateId < 0 || stateId >= _stateList.Count)
        {
            throw new KeyNotFoundException($"State {stateId} is out of range.");
        }

        return stateId;
    }

    public float GetQValue(int stateId, int action)
    {
        int stateIndex = GetStateIndex(stateId);
        return _qTable[stateIndex, action];
    }

    public void UpdateQValue(int stateId, int action, float newQ)
    {
        int stateIndex = GetStateIndex(stateId);
        Debug.Log($"Updated Q value at: {stateIndex} with an action of {action} and a new Q of: {newQ}");
        _qTable[stateIndex, action] = newQ;
    }

    public float GetBestQ(int stateId)
    {
        int stateIndex = GetStateIndex(stateId);
        float maxQ = float.MinValue;

        for (int i = 0; i < _actionNum; i++)
        {
            if (_qTable[stateIndex, i] > maxQ)
            {
                maxQ = _qTable[stateIndex, i];
            }
        }

        return maxQ;
    }

    public int GetBestAction(int stateId)
    {
        int stateIndex = GetStateIndex(stateId);
        float maxQ = float.MinValue;
        int bestAction = -1;

        for (int i = 0; i < _actionNum; i++)
        {
            if(_qTable[stateIndex, i] > maxQ)
            {
                maxQ = _qTable[stateIndex, i];
                bestAction = i;
            }
        }

        return bestAction;
    }

    public IEnumerable<State> GetAllStates()
    {
        return _stateList;
    }
}
