using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using System.Collections.Generic;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class Reward
    {
        public float Value { get; set; }
        public int PlayerID { get; set; }

        public List<object []> ActionHistory { get; set; }

    }
}
