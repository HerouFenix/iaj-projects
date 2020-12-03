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
            state.CalculateNextPlayer();

            int playoutDepth = 0;

            // Heuristics
            float wasted = 0.0f;

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
                state.CalculateNextPlayer();

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
                Value = ComputeHeuristicScore(initialPlayoutState, state, wasted),
                PlayerID = state.GetNextPlayer()
            };

            return reward;
        }

        private float ComputeHeuristicScore(MCTSNode initialState, IWorldModel state, float wasted)
        {
            int finalHP = (int)state.GetProperty("HP") + (int)state.GetProperty("ShieldHP");
            int currentHP = (int)this.InitialNode.State.GetProperty("HP") + (int)state.GetProperty("ShieldHP");

            if (initialState.Parent.Parent == null)
            {
                // If our parent is the root cull stupid actions
                if (initialState.Action.Name.Contains("PickUp") && initialState.Action.GetDuration() <= 0.1f)
                { // If we're right on top of a chest just pick it up
                    return Mathf.Infinity;
                }
                else if (initialState.Action.Name.Contains("PickUp"))
                { // If we're right on top of a chest just pick it up
                    var chestPosition = ((WalkToTargetAndExecuteAction)initialState.Action).Target.transform.position;
                    chestPosition.y = 0;
                    foreach (var enemy in initialState.State.GetEnemies())
                    {
                        var enemyPosition = enemy.transform.position;
                        enemyPosition.y = 0;
                        if (Vector3.Distance(chestPosition, enemyPosition) <= 30)
                        {
                            if (enemy.name.Contains("Dragon") && currentHP <= 15)
                            {
                                return -300; // Dont get a chest next to a dragon if the HP is low
                            }
                            else if (enemy.name.Contains("Orc") && currentHP <= 10)
                            {
                                return -300; // Dont get a chest next to an orc if the HP is low
                            }
                            else if (enemy.name.Contains("Skelleton") && currentHP <= 4)
                            {
                                return -300; // Dont get a chest next to a spooky scary skelleton if the HP is low
                            }
                        }
                    }
                }
                else if (initialState.Action.Name.Contains("SwordAttack") && initialState.Action.Name.Contains("Dragon") && currentHP <= 15)
                { // Don't try to fight the dragon if u got less than 16...suposively u should be able to kill em with less than that but my luck is poop
                    return -10;
                }
                else if (initialState.Action.Name.Contains("SwordAttack") && initialState.Action.Name.Contains("Orc") && currentHP <= 10)
                { // Don't try to fight the orc if u got less than 10...suposively u should be able to kill em with less than that but my luck is poop
                    return -10;
                }
                else if (initialState.Action.Name.Contains("LevelUp"))
                { // If we can level up, just do it
                    return Mathf.Infinity;
                }
                else if ((initialState.Action.Name.Contains("ShieldOfFaith")))
                {
                    if ((int)this.InitialNode.State.GetProperty("ShieldHP") == 5) // If shield is full don't try to heal dude
                        return -Mathf.Infinity;
                    else if ((int)this.InitialNode.State.GetProperty("ShieldHP") == 0) // Replenish the shield when it dies..just to be safe
                        return 150.0f;
                    else if ((int)this.InitialNode.State.GetProperty("ShieldHP") <= 2 && (int)this.InitialNode.State.GetProperty("HP") <= 5) // If health is real low and so is the shield hp, just reuse it as a safety measure
                        return 150.0f;
                }
                else if ((initialState.Action.Name.Contains("GetHealthPotion") || initialState.Action.Name.Contains("Rest")) && (int)this.InitialNode.State.GetProperty("HP") >= (int)this.InitialNode.State.GetProperty("MAXHP") - 2)
                { // If health is full don't try to heal dude
                    return -Mathf.Infinity;
                }
                else if ((initialState.Action.Name.Contains("GetManaPotion")) && (int)this.InitialNode.State.GetProperty("Mana") == 10)
                { // If mana is full don't try to get more mana dude
                    return -Mathf.Infinity;
                }
                else if ((initialState.Action.Name.Contains("GetManaPotion")) && (int)this.InitialNode.State.GetProperty("Mana") == 0)
                { // If mana is empty try to get mana
                    return 50.0f;
                }
            }

            if (finalHP < 1)
            { // If we lost thats like...real bad dude
                return -10.0f;
            }

            int initialHP = (int)this.InitialNode.State.GetProperty("HP") + (int)this.InitialNode.State.GetProperty("ShieldHP");
            int maxHP = (int)this.InitialNode.State.GetProperty("MAXHP") + 5;

            int finalMana = (int)state.GetProperty("Mana");
            int initialMana = (int)this.InitialNode.State.GetProperty("Mana");

            int finalMoney = (int)state.GetProperty("Money");
            int initialMoney = (int)this.InitialNode.State.GetProperty("Money");

            int finalLvl = (int)state.GetProperty("Level");
            int initialLvl = (int)this.InitialNode.State.GetProperty("Level");

            float finalTime = (float)state.GetProperty("Time");
            float initialTime = (float)this.InitialNode.State.GetProperty("Time");

            var HPGain = finalHP - initialHP;
            var ManaGain = finalMana - initialMana;
            var MoneyGain = finalMoney - initialMoney;
            var LvlGain = finalLvl - initialLvl;
            var TimeLoss = finalTime - initialTime;


            var heuristicScore = HPGain / maxHP + ManaGain / 10.0f + MoneyGain + LvlGain - TimeLoss * 2.5f;
            return heuristicScore;

        }


        /*
         * private float ComputeHeuristicScore(IWorldModel initialState, IWorldModel state, float wasted)
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
            }else if(heuristicValue < 0)
            {
                heuristicValue = 0.0f;
            }

            return heuristicValue;
        }*/

    }
}
