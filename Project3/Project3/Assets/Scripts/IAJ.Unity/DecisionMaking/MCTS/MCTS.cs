﻿using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using System;
using System.Collections.Generic;
using UnityEngine;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.Action;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class MCTS
    {
        public const float C = 1.4f;

        public bool ChildCulling = false;

        public bool InProgress { get; set; }
        public int MaxIterations { get; set; }
        public int MaxIterationsProcessedPerFrame { get; set; }
        public int MaxPlayoutDepthReached { get; set; }
        public int MaxPlayouts { get; set; }
        public int MaxSelectionDepthReached { get; set; }
        public float TotalProcessingTime { get; set; }
        public MCTSNode BestFirstChild { get; set; }
        public List<Action> BestActionSequence { get; private set; }
        public IWorldModel BestActionSequenceWorldState { get; private set; }
        protected int CurrentIterations { get; set; }
        protected int CurrentIterationsInFrame { get; set; }
        protected int CurrentDepth { get; set; }
        public IWorldModel CurrentStateWorldModel { get; set; }
        public IWorldModel NextWorldModel { get; set; }
        protected MCTSNode InitialNode { get; set; }
        protected System.Random RandomGenerator { get; set; }

        public bool fearWorld;


        public MCTS(IWorldModel currentStateWorldModel)
        {
            this.InProgress = false;
            this.CurrentStateWorldModel = currentStateWorldModel;
            this.MaxIterations = 800;
            this.MaxIterationsProcessedPerFrame = 30;
            this.RandomGenerator = new System.Random();
            this.MaxPlayouts = 6;
        }


        public void InitializeMCTSearch()
        {
            if (fearWorld)
            {
                this.CurrentStateWorldModel.UpdateWorldArray();
            }

            this.MaxPlayoutDepthReached = 0;
            this.MaxSelectionDepthReached = 0;
            this.CurrentIterations = 0;
            this.CurrentIterationsInFrame = 0;
            this.TotalProcessingTime = 0.0f;
            this.CurrentStateWorldModel.Initialize();
            this.InitialNode = new MCTSNode(this.CurrentStateWorldModel)
            {
                Action = null,
                Parent = null,
                PlayerID = 0
            };
            this.InProgress = true;
            this.BestFirstChild = null;
            this.BestActionSequence = new List<Action>();
        }

        public virtual Action Run()
        {
            MCTSNode selectedNode;
            Reward reward;

            var startTime = Time.realtimeSinceStartup;

            while (this.CurrentIterations < this.MaxIterations)
            {
                if (this.CurrentIterationsInFrame > this.MaxIterationsProcessedPerFrame)
                {
                    this.CurrentIterationsInFrame = 0;
                    this.TotalProcessingTime += Time.realtimeSinceStartup - startTime;
                    return null;
                }

                // Selection
                selectedNode = this.Selection(this.InitialNode);

                // Playout (Simulate result)
                reward = this.Playout(selectedNode);

                // Do several playouts and get the average reward
                for (int i = 1; i < this.MaxPlayouts; i++)
                {
                    reward.Value += this.Playout(selectedNode).Value;
                }

                reward.Value = reward.Value / this.MaxPlayouts;

                // Backpropagate results
                this.Backpropagate(selectedNode, reward);

                this.CurrentIterationsInFrame++;
                this.CurrentIterations++;
            }

            this.TotalProcessingTime += Time.realtimeSinceStartup - startTime;
            this.CurrentIterationsInFrame = 0;
            this.InProgress = false;
            return this.BestFinalAction(this.InitialNode);
        }

        // Selection and Expantion
        protected MCTSNode Selection(MCTSNode initialNode)
        {
            Action nextAction;
            MCTSNode currentNode = initialNode;

            int selectionDepth = 0;

            while (!currentNode.State.IsTerminal())
            {
                selectionDepth++;
                nextAction = currentNode.State.GetNextAction();
                if (nextAction == null) // Node fully expanded, go to its best child and carry on
                {
                    currentNode = this.BestUCTChild(currentNode);
                }
                else
                { // Still got more actions so expand (will generate a new Leaf node)
                    return this.Expand(currentNode, nextAction);
                }

            }

            if (selectionDepth > this.MaxSelectionDepthReached)
            {
                this.MaxSelectionDepthReached = selectionDepth;
            }

            // CurrentNode is terminal, hence its a leaf
            return currentNode;
        }

        protected virtual Reward Playout(MCTSNode initialPlayoutState)
        {
            Action[] executableActions;

            IWorldModel state = initialPlayoutState.State.GenerateChildWorldModel();
            state.CalculateNextPlayer();

            int playoutDepth = 0;
            while (!state.IsTerminal())
            {
                executableActions = state.GetExecutableActions();
                int randomIndex = this.RandomGenerator.Next(0, executableActions.Length);

                Action randomAction = executableActions[randomIndex];

                randomAction.ApplyActionEffects(state);
                state.CalculateNextPlayer();

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

        protected virtual void Backpropagate(MCTSNode node, Reward reward)
        {
            while (node != null)
            {
                node.N = node.N + 1;
                node.Q = node.Q + reward.Value;

                node = node.Parent;
            }
        }

        protected MCTSNode Expand(MCTSNode parent, Action action)
        {
            IWorldModel newState = parent.State.GenerateChildWorldModel();

            action.ApplyActionEffects(newState);
            newState.CalculateNextPlayer();

            MCTSNode newNode = new MCTSNode(newState)
            {
                Action = action,
                Parent = parent,
                PlayerID = newState.GetNextPlayer(),
                Q = 0,
                N = 0,
                QRave = 0.0f,
                NRave = 0.0f
            };

            parent.ChildNodes.Add(newNode);

            return newNode;
        }

        protected virtual MCTSNode BestUCTChild(MCTSNode node)
        {
            double bestEstimatedValue = -Mathf.Infinity;
            MCTSNode bestChild = null;

            foreach (MCTSNode child in node.ChildNodes)
            {
                double estimatedValue;

                if (this.ChildCulling)
                {
                    if (child.Action.Name.Equals("LevelUp"))
                    { //  Cull children whose actions involve doing a completely unecessary move
                        estimatedValue = 10.0f;
                    }
                    else if (child.Action.Name.Contains("PickUp") && child.Action.GetDuration() <= 0.1f)
                    {
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
                    else if (child.Action.Name.Equals("ShieldOfFaith") && (int)node.State.GetProperty("ShieldHP") == 5)
                    {
                        estimatedValue = -10.0f;
                    }
                    else
                    {
                        estimatedValue = child.Q / child.N + C * Mathf.Sqrt(Mathf.Log10(node.N) / child.N);
                    }
                }
                else
                {
                    estimatedValue = child.Q / child.N + C * Mathf.Sqrt(Mathf.Log10(node.N) / child.N);
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

        //this method is very similar to the bestUCTChild, but it is used to return the final action of the MCTS search, and so we do not care about
        //the exploration factor
        protected virtual MCTSNode BestChild(MCTSNode node)
        {
            double bestEstimatedValue = -Mathf.Infinity;
            MCTSNode bestChild = null;

            foreach (MCTSNode child in node.ChildNodes)
            {
                double estimatedValue;
                if (this.ChildCulling)
                {//  Cull children whose actions involve doing a completely unecessary move
                    if (child.Action.Name.Equals("LevelUp"))
                    { 
                        estimatedValue = 10.0f;
                    }
                    else if (child.Action.Name.Contains("PickUp") && child.Action.GetDuration() <= 0.1f)
                    {
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
                    else if (child.Action.Name.Equals("ShieldOfFaith") && (int)node.State.GetProperty("ShieldHP") == 5)
                    {
                        estimatedValue = -10.0f;
                    }
                    else
                    {
                        estimatedValue = child.Q / child.N;
                    }
                }
                else
                {
                    estimatedValue = child.Q / child.N;
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


        protected Action BestFinalAction(MCTSNode node)
        {
            var bestChild = this.BestChild(node);
            if (bestChild == null) return null;

            this.BestFirstChild = bestChild;

            //this is done for debugging proposes only
            this.BestActionSequence = new List<Action>();
            this.BestActionSequence.Add(bestChild.Action);
            node = bestChild;

            while (!node.State.IsTerminal())
            {
                bestChild = this.BestChild(node);
                if (bestChild == null) break;
                this.BestActionSequence.Add(bestChild.Action);
                node = bestChild;
                this.BestActionSequenceWorldState = node.State;
            }

            return this.BestFirstChild.Action;
        }

    }
}
