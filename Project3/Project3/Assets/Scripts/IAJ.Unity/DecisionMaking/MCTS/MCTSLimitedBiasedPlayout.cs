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
            this.MaxPlayoutDepth = 15;
        }

        protected override Reward Playout(IWorldModel initialPlayoutState)
        {
            Action[] executableActions;

            IWorldModel state = initialPlayoutState.GenerateChildWorldModel();

            int playoutDepth = 0;

            // Heuristics
            float heuristicScore = 0.0f;

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

                // Heuristics
                heuristicScore += randomAction.GetHValue(state)*0.001f;

                // HP Wasted
                if(randomAction.Name.Contains("GetHealthPotion") && (int)state.GetProperty("HP") >= 10)
                    heuristicScore -= 10.0f;
                
                // Mana Lost -> The bigger the Mana loss, the bigger the loss / The Bigger the Mana Gain, the bigger the reward
                if (randomAction.Name.Contains("GetManaPotion") && (int)state.GetProperty("Mana") >= 7)
                    heuristicScore -= 10.0f;

                randomAction.ApplyActionEffects(state);

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

            // Time Wasted -> The bigger the waste the bigger the loss
            heuristicScore += ((float)state.GetProperty("Time") - (float)initialPlayoutState.GetProperty("Time"))*0.5f;
            //
            //// Money Gain -> The bigger the Money Gain, the bigger the reward
            heuristicScore -= ((int)state.GetProperty("Money") - (int)initialPlayoutState.GetProperty("Money"))*0.8f;

            float score = state.GetScore() - heuristicScore;
           

            Reward reward = new Reward
            {
                Value = score,
                PlayerID = state.GetNextPlayer()
            };

            return reward;
        }

    }
}
