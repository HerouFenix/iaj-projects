using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.IAJ.Unity.Utils;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel
{
    public interface IWorldModel
    {
        object GetProperty(string propertyName);

        void Initialize();

        void SetProperty(string propertyName, object value);

        IWorldModel GenerateChildWorldModel();

        float GetGoalValue(string goalName);

        void SetGoalValue(string goalName, float value);

        float CalculateDiscontentment(List<Goal> goals);

        float CalculateUtility(List<Goal> goals);

        Action GetNextAction();

        Action[] GetExecutableActions();

        bool IsTerminal();

        float GetScore();

        int GetNextPlayer();

        void CalculateNextPlayer();

        void UpdateWorldArray();
    }
}
