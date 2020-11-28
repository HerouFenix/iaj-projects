using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.IAJ.Unity.Utils;
using System;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class DivineSmite : WalkToTargetAndExecuteAction
    {
        private float expectedXPChange;
        private int xpChange;

        public DivineSmite(AutonomousCharacter character, GameObject target) : base("DivineSmite", character, target)
        {
            this.xpChange = 3;
            this.expectedXPChange = 2.7f;
        }

        public override bool CanExecute()
        {
            return base.CanExecute() && this.Character.GameManager.characterData.Mana >= 2;
        }

        public override bool CanExecute(IWorldModel IWorldModel)
        {
            return base.CanExecute(IWorldModel) && (int)IWorldModel.GetProperty(Properties.MANA) >= 2;
        }

        public override void Execute()
        {
            base.Execute();
            this.Character.GameManager.DivineSmite(this.Target);
        }

        public override void ApplyActionEffects(IWorldModel IWorldModel)
        {
            base.ApplyActionEffects(IWorldModel);

            int xpValue = (int)IWorldModel.GetGoalValue(AutonomousCharacter.GAIN_LEVEL_GOAL);
            IWorldModel.SetGoalValue(AutonomousCharacter.GAIN_LEVEL_GOAL, xpValue - expectedXPChange);
            
            int xp = (int)IWorldModel.GetProperty(Properties.XP);
            IWorldModel.SetProperty(Properties.MANA, (int)IWorldModel.GetProperty(Properties.MANA) - 2);
            IWorldModel.SetProperty(Properties.XP, xp + this.xpChange);

            IWorldModel.SetProperty(this.Target.name, false);
        }

        public override float GetHValue(IWorldModel IWorldModel)
        {

            return base.GetHValue(IWorldModel) / 1.5f;

        }
    }
}
