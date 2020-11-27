using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using System;
using System.Collections.Generic;
using UnityEngine;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.Action;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class MCTSBiasedPlayout : MCTS
    {

        public MCTSBiasedPlayout(CurrentStateWorldModel currentStateWorldModel) : base(currentStateWorldModel)
        {
        }

        protected double[] Gibbs(Action[] actions, WorldModel state)
        {
            var probabilities = new double[actions.Length];
            double heuristicSum = 0.0;
            foreach (Action ac in actions)
            {
                var h = ac.GetHValue(state);
                heuristicSum += Math.Exp(-h);
            }

            for (int i = 0; i < actions.Length; i++)
            {
                probabilities[i] = Math.Exp(-actions[i].GetHValue(state)) / heuristicSum;
            }


            return probabilities;
        }

        /*
        protected override Reward Playout(WorldModel initialPlayoutState)
        {
            Action[] executableActions;

            WorldModel state = initialPlayoutState.GenerateChildWorldModel();

            int playoutDepth = 0;
            while (!state.IsTerminal())
            {
                executableActions = state.GetExecutableActions();
                double[] probabilities = this.Gibbs(executableActions, state);

                Array.Sort(probabilities, executableActions);


                
                int randomIndex = 0;
                double probValue = 0.0f;
                int maxCounter = 0;
                double minProb = 0.1;
                while (probValue < minProb &&  maxCounter < 300)
                { // Done so we ignore probabilities that are really poopy
                    double randomValue = this.RandomGenerator.NextDouble();
                    double cumulative = 0.0;
                    for (int i = 0; i < probabilities.Length; i++)
                    {
                        cumulative += probabilities[i];
                        if (randomValue < cumulative)
                        {
                            probValue = probabilities[i];
                            randomIndex = i;
                            break;
                        }
                    }

                    maxCounter++;
                    if (maxCounter == 100) minProb = 0.09;
                    else if (maxCounter == 200) minProb = 0.08;
                    else if (maxCounter == 250) minProb = 0.05;
                }
                

                
                //int randomIndex = 0;
                //
                //double randomValue = this.RandomGenerator.NextDouble();
                //double cumulative = 0.0;
                //for (int i = 0; i < probabilities.Length; i++)
                //{
                //    cumulative += probabilities[i];
                //    if (randomValue < cumulative)
                //    {
                //        randomIndex = i;
                //        break;
                //    }
                //}
                


                //double averageValue = 1.0f / executableActions.Length;
                //int randomIndex;
                //int maxCounter = 0;
                //while (true)
                //{
                //    randomIndex = this.RandomGenerator.Next(0, executableActions.Length);
                //    if (probabilities[randomIndex] > averageValue || maxCounter++ > 1000)
                //    {
                //        break;
                //    }
                //}

                Action randomAction = executableActions[randomIndex];

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
                PlayerID = state.GetNextPlayer()
            };

            return reward;
        }
        */

        protected override Reward Playout(WorldModel initialPlayoutState)
        {
            Action[] executableActions;

            WorldModel state = initialPlayoutState.GenerateChildWorldModel();

            int playoutDepth = 0;
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
                PlayerID = state.GetNextPlayer()
            };

            return reward;
        }

    }
}
