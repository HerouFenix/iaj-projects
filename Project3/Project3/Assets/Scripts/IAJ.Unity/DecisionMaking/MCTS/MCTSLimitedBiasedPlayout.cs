using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using System;
using System.Collections.Generic;
using UnityEngine;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.Action;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class MCTSLimitedBiasedPlayout : MCTSBiasedPlayout
    {
        private int MaxPlayoutDepth { get; set; }


        public MCTSLimitedBiasedPlayout(IWorldModel currentStateWorldModel) : base(currentStateWorldModel)
        {
            this.MaxPlayoutDepth = 5;
        }

        protected override Reward Playout(MCTSNode initialPlayoutState)
        {
            Action[] executableActions;

            IWorldModel state = initialPlayoutState.State.GenerateChildWorldModel();

            int playoutDepth = 0;

            // Heuristics
            float wasted = 0.0f;
            //float actionTime = 0.0f;
            //actionTime += initialPlayoutState.Action.GetDuration();
            //if ((initialPlayoutState.Action.Name.Contains("GetHealthPotion") || initialPlayoutState.Action.Name.Contains("Rest")) && (int)this.InitialNode.State.GetProperty("HP") >= 10)
            //{
            //    wasted += 10.0f;
            //}
            //else if (initialPlayoutState.Action.Name.Contains("GetManaPotion") && (int)this.InitialNode.State.GetProperty("Mana") == 10)
            //{
            //    wasted += 10.0f;
            //}
            //else if (initialPlayoutState.Action.Name.Contains("LevelUp"))
            //{
            //    wasted -= 10.0f;
            //}


            while (!state.IsTerminal())
            {
                executableActions = state.GetExecutableActions();
                double[] probabilities = this.Gibbs(executableActions, state);

                double averageValue = 1.0f / executableActions.Length;
                int randomIndex;
                int maxCounter = 0;
                while (true)
                {
                    randomIndex = this.RandomGenerator.Next(0, executableActions.Length);
                    if (probabilities[randomIndex] > averageValue || maxCounter++ > 1000)
                    { // Make sure the selected probability has a value higher than average
                        break;
                    }
                }


                Action randomAction = executableActions[randomIndex];

                if (randomAction.Name.Contains("GetHealthPotion") && (int)state.GetProperty("HP") == (int)state.GetProperty("MAXHP"))
                {
                    wasted += 0.5f;
                }
                if (randomAction.Name.Contains("GetManaPotion") && (int)state.GetProperty("Mana") == 10)
                {
                    wasted += 0.5f;
                }



                randomAction.ApplyActionEffects(state);
                //actionTime += randomAction.GetDuration();

                playoutDepth++;

                if (playoutDepth >= this.MaxPlayoutDepth)
                { // Max playout depth allowed reached. Force close
                    break;
                }

            }

            if (playoutDepth > this.MaxPlayoutDepthReached)
            {
                this.MaxPlayoutDepthReached = playoutDepth;
            }


            Reward reward = new Reward
            {
                Value = ComputeHeuristicScore(initialPlayoutState.State, state, wasted),
                PlayerID = state.GetNextPlayer()
            };

            return reward;
        }

        private float ComputeHeuristicScore(IWorldModel initialState, IWorldModel state, float wasted)
        {
            if (state.IsTerminal())
            { // If the node is terminal, get its score as normal
                return state.GetScore();
            }

            // Heuristic Score > Bigger == Better

            float maxValue = 0.0f;
            float minValue = Mathf.Infinity;

            float timeSpent = ((float)state.GetProperty("Time") - (float)this.InitialNode.State.GetProperty("Time"));
            if (timeSpent > maxValue) maxValue = timeSpent;
            if (timeSpent < minValue) minValue = timeSpent;

            //float hpChange = ((int)state.GetProperty("HP") - (int)this.InitialNode.State.GetProperty("HP"));
            //if (hpChange > maxValue) maxValue = hpChange;
            //if (hpChange < minValue) minValue = hpChange;

            float moneyGained = ((int)state.GetProperty("Money") - (int)this.InitialNode.State.GetProperty("Money"));
            if (moneyGained > maxValue) maxValue = moneyGained;
            if (moneyGained < minValue) minValue = moneyGained;

            float lvlGained = ((int)state.GetProperty("Level") - (int)this.InitialNode.State.GetProperty("Level"));
            if (lvlGained > maxValue) maxValue = lvlGained;
            if (lvlGained < minValue) minValue = lvlGained;

            float manaGained = ((int)state.GetProperty("Mana") - (int)this.InitialNode.State.GetProperty("Mana"));
            if (manaGained > maxValue) maxValue = manaGained;
            if (manaGained < minValue) minValue = manaGained;

            // Normalize
            timeSpent = (timeSpent - minValue) / (maxValue - minValue);
            //hpChange = (hpChange - minValue) / (maxValue - minValue);
            moneyGained = (moneyGained - minValue) / (maxValue - minValue);
            lvlGained = (lvlGained - minValue) / (maxValue - minValue);
            manaGained = (manaGained - minValue) / (maxValue - minValue);

            //float heuristicValue = hpChange*0.075f + moneyGained*0.2f + manaGained*0.1f - timeSpent*2f - wasted;
            float heuristicValue = moneyGained + manaGained*0.8f + lvlGained*2.0f - timeSpent - wasted;
            if(heuristicValue > 1.0f)
            {
                heuristicValue = 1.0f;
            }else if(heuristicValue < 0.0f)
            {
                heuristicValue = 0.0f;
            }

            return heuristicValue;
        }

    }
}
