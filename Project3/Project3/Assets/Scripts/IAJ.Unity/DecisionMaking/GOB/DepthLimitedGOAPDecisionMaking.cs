using Assets.Scripts.GameManager;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.GOB
{
    public class DepthLimitedGOAPDecisionMaking
    {
        public int MAX_DEPTH = 4;
        public int ActionCombinationsProcessedPerFrame { get; set; }
        public float TotalProcessingTime { get; set; }
        public int TotalActionCombinationsProcessed { get; set; }
        public bool InProgress { get; set; }

        public CurrentStateWorldModel InitialWorldModel { get; set; }
        private List<Goal> Goals { get; set; }
        private WorldModel[] Models { get; set; }
        private Action[] ActionPerLevel { get; set; }
        public Action[] BestActionSequence { get; private set; }
        public Action BestAction { get; private set; }
        public float BestDiscontentmentValue { get; private set; }
        private int CurrentDepth { get; set; }

        public DepthLimitedGOAPDecisionMaking(CurrentStateWorldModel currentStateWorldModel, List<Action> actions, List<Goal> goals)
        {
            this.ActionCombinationsProcessedPerFrame = 200;
            this.Goals = goals;
            this.InitialWorldModel = currentStateWorldModel;
        }

        public void InitializeDecisionMakingProcess()
        {
            this.InProgress = true;
            this.TotalProcessingTime = 0.0f;
            this.TotalActionCombinationsProcessed = 0;
            this.CurrentDepth = 0;
            this.Models = new WorldModel[MAX_DEPTH + 1];
            this.Models[0] = this.InitialWorldModel;
            this.ActionPerLevel = new Action[MAX_DEPTH];
            this.BestActionSequence = new Action[MAX_DEPTH];
            this.BestAction = null;
            this.BestDiscontentmentValue = float.MaxValue;
            this.InitialWorldModel.Initialize();
        }

        public Action ChooseAction()
        {
            var startTime = Time.realtimeSinceStartup;

            float currentValue;
            int combinationsProcessed = 0; // Number of combinations processed on current frame

            while (this.CurrentDepth >= 0)
            {
                currentValue = this.Models[this.CurrentDepth].CalculateDiscontentment(this.Goals);

                if (this.CurrentDepth >= MAX_DEPTH)
                {
                    if (currentValue < this.BestDiscontentmentValue)
                    {
                        // Found better action/sequence
                        this.BestDiscontentmentValue = currentValue;
                        this.BestAction = this.ActionPerLevel[0];
                        this.BestActionSequence = (Action[])this.ActionPerLevel.Clone();
                    }

                    this.CurrentDepth--;

                    // We reached and processed a new action combination
                    combinationsProcessed++;
                    if (combinationsProcessed >= this.ActionCombinationsProcessedPerFrame)
                    {
                        this.TotalActionCombinationsProcessed += combinationsProcessed;
                        this.TotalProcessingTime += Time.realtimeSinceStartup - startTime;
                        return null;
                    }

                    continue;
                }
                else if (this.CurrentDepth > 0 && currentValue > this.BestDiscontentmentValue)
                {
                    // Current world has worse discontentment than the best computed so far. Not worth it to continue expanding it. Backtrack
                    this.CurrentDepth--;
                    continue;
                }

                var nextAction = this.Models[this.CurrentDepth].GetNextAction();
                if (nextAction != null && nextAction.CanExecute(this.Models[this.CurrentDepth]))
                {
                    this.Models[this.CurrentDepth + 1] = this.Models[this.CurrentDepth].GenerateChildWorldModel();
                    nextAction.ApplyActionEffects(this.Models[this.CurrentDepth + 1]);

                    this.ActionPerLevel[this.CurrentDepth] = nextAction;
                    this.CurrentDepth++;
                }
                else
                {
                    // No more actions to be applied
                    this.CurrentDepth--;
                }
            }

            this.TotalActionCombinationsProcessed += combinationsProcessed;
            this.TotalProcessingTime += Time.realtimeSinceStartup - startTime;
            this.InProgress = false;

            if (this.BestAction == null)
            { // In case we couldn't execute any action, try reducing the depth
                MAX_DEPTH--;
                this.InitializeDecisionMakingProcess();
            }

            return this.BestAction;
        }
    }
}
