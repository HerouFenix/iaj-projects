using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using System;
using System.Collections.Generic;
using UnityEngine;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.Action;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class MCTSRave : MCTS
    {
        public float KParameter { get; set; }

        public MCTSRave(IWorldModel currentStateWorldModel) : base(currentStateWorldModel)
        {
            KParameter = 10.0f;
        }

        protected override Reward Playout(MCTSNode initialPlayoutState)
        {
            Action[] executableActions;
            List<object[]> actionHistory = new List<object[]>(); // Stores Player/Action combo

            IWorldModel state = initialPlayoutState.State.GenerateChildWorldModel();

            int playoutDepth = 0;
            while (!state.IsTerminal())
            {
                executableActions = state.GetExecutableActions();
                int randomIndex = this.RandomGenerator.Next(0, executableActions.Length);

                Action randomAction = executableActions[randomIndex];
                actionHistory.Add(new object[2] { state.GetNextPlayer(), randomAction });

                randomAction.ApplyActionEffects(state);
                playoutDepth++;
            }

            if (playoutDepth > this.MaxPlayoutDepthReached)
            {
                this.MaxPlayoutDepthReached = playoutDepth;
            }

            Reward reward = new Reward
            {
                Value = state.GetScore(),
                PlayerID = state.GetNextPlayer(),
                ActionHistory = actionHistory
            };

            return reward;
        }

        protected override void Backpropagate(MCTSNode node, Reward reward)
        {
            while (node != null)
            {
                node.N = node.N + 1;
                node.Q = node.Q + reward.Value;

                //List<Action> actionHistory = reward.ActionHistory;

                node = node.Parent;
                if (node != null)
                {
                    int p = node.PlayerID;
                    foreach (MCTSNode child in node.ChildNodes)
                    {
                        for (int i = 0; i < reward.ActionHistory.Count; i++)
                        {
                            var oof = (int)reward.ActionHistory[i][0];
                            var oof2 = ((Action)reward.ActionHistory[i][1]).Name;
                            var oof3 = child.Action.Name;
                            if ((int)reward.ActionHistory[i][0] == p && ((Action)reward.ActionHistory[i][1]).Name.Equals(child.Action.Name))
                            {
                                child.NRave = child.NRave + 1;
                                child.QRave = child.QRave + reward.Value;
                            }
                        }
                    }
                }
            }
        }

        private double ComputeBeta(int N)
        {
            return Math.Sqrt(this.KParameter / (3 * N + this.KParameter));
        }

        protected override MCTSNode BestUCTChild(MCTSNode node)
        {
            double bestEstimatedValue = -1.0f;
            MCTSNode bestChild = null;

            foreach (MCTSNode child in node.ChildNodes)
            {
                double estimatedValue;

                if (this.ChildCulling)
                {
                    if (child.Action.Name == "LevelUp")
                    { //  Cull children whose actions involve doing a completely unecessary move
                        estimatedValue = 10.0f;
                    }
                    else if (child.Action.Name.Contains("GetHealthPotion") && (int)node.State.GetProperty("HP") >= 10)
                    {
                        estimatedValue = -10.0f;
                    }
                    else if (child.Action.Name.Contains("Rest") && (int)node.State.GetProperty("HP") >= 10)
                    {
                        estimatedValue = -10.0f;
                    }
                    else if (child.Action.Name.Contains("GetManaPotion") && (int)node.State.GetProperty("Mana") >= 7)
                    {
                        estimatedValue = -10.0f;
                    }
                    else if (child.Action.Name == "ShieldOfFaith" && (int)node.State.GetProperty("ShieldHP") == 5)
                    {
                        estimatedValue = -10.0f;
                    }
                    else
                    {
                        double niu = child.Q / child.N;
                        double niuRave;
                        if (child.NRave == 0)
                        {
                            niuRave = 0;
                        }
                        else
                        {
                            niuRave = child.QRave / child.NRave;
                        }
                        double beta = this.ComputeBeta(node.N);

                        estimatedValue = ((1 - beta) * niu + beta * niuRave) + C * Mathf.Sqrt(Mathf.Log10(node.N) / child.N);
                    }
                }
                else
                {
                    double niu = child.Q / child.N;
                    double niuRave;
                    if (child.NRave == 0)
                    {
                        niuRave = 0;
                    }
                    else
                    {
                        niuRave = child.QRave / child.NRave;
                    }
                    double beta = this.ComputeBeta(node.N);

                    estimatedValue = ((1 - beta) * niu + beta * niuRave) + C * Mathf.Sqrt(Mathf.Log10(node.N) / child.N);
                }
                

                if (estimatedValue > bestEstimatedValue)
                {
                    bestEstimatedValue = estimatedValue;
                    bestChild = child;
                }
                else if (Math.Abs(estimatedValue - bestEstimatedValue) < 1e-3)
                { //If same estimated value, then check if one is quicker than the other
                    if (child.Action.GetDuration() < bestChild.Action.GetDuration())
                    {
                        bestEstimatedValue = estimatedValue;
                        bestChild = child;
                    }
                }
            }

            return bestChild;
        }

        protected override MCTSNode BestChild(MCTSNode node)
        {
            double bestEstimatedValue = -1.0f;
            MCTSNode bestChild = null;

            foreach (MCTSNode child in node.ChildNodes)
            {
                double estimatedValue;

                if (this.ChildCulling)
                {
                    if (child.Action.Name == "LevelUp")
                    { //  Cull children whose actions involve doing a completely unecessary move
                        estimatedValue = 10.0f;
                    }
                    else if (child.Action.Name.Contains("GetHealthPotion") && (int)node.State.GetProperty("HP") >= 10)
                    {
                        estimatedValue = -10.0f;
                    }
                    else if (child.Action.Name.Contains("Rest") && (int)node.State.GetProperty("HP") >= 10)
                    {
                        estimatedValue = -10.0f;
                    }
                    else if (child.Action.Name.Contains("GetManaPotion") && (int)node.State.GetProperty("Mana") >= 7)
                    {
                        estimatedValue = -10.0f;
                    }
                    else if (child.Action.Name == "ShieldOfFaith" && (int)node.State.GetProperty("ShieldHP") == 5)
                    {
                        estimatedValue = -10.0f;
                    }
                    else
                    {
                        double niu = child.Q / child.N;
                        double niuRave;
                        if (child.NRave == 0)
                        {
                            niuRave = 0;
                        }
                        else
                        {
                            niuRave = child.QRave / child.NRave;
                        }
                        double beta = this.ComputeBeta(node.N);

                        estimatedValue = ((1 - beta) * niu + beta * niuRave);
                    }
                }
                else
                {
                    double niu = child.Q / child.N;
                    double niuRave;
                    if (child.NRave == 0)
                    {
                        niuRave = 0;
                    }
                    else
                    {
                        niuRave = child.QRave / child.NRave;
                    }
                    double beta = this.ComputeBeta(node.N);

                    estimatedValue = ((1 - beta) * niu + beta * niuRave);
                }

                if (estimatedValue > bestEstimatedValue)
                {
                    bestEstimatedValue = estimatedValue;
                    bestChild = child;
                }
                else if (Math.Abs(estimatedValue - bestEstimatedValue) < 1e-3)
                { //If same estimated value, then check if one is quicker than the other
                    if (child.Action.GetDuration() < bestChild.Action.GetDuration())
                    {
                        bestEstimatedValue = estimatedValue;
                        bestChild = child;
                    }
                }
            }

            return bestChild;
        }

    }
}
