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

        public override bool CanExecute(WorldModel worldModel)
        {
            return base.CanExecute(worldModel) && (int)worldModel.GetProperty(Properties.MANA) >= 2;
        }

        public override void Execute()
        {
            base.Execute();
            this.Character.GameManager.DivineSmite(this.Target);
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

            int xpValue = (int)worldModel.GetGoalValue(AutonomousCharacter.GAIN_LEVEL_GOAL);
            worldModel.SetGoalValue(AutonomousCharacter.GAIN_LEVEL_GOAL, xpValue - expectedXPChange);
            
            int xp = (int)worldModel.GetProperty(Properties.XP);
            worldModel.SetProperty(Properties.MANA, (int)worldModel.GetProperty(Properties.MANA) - 2);
            worldModel.SetProperty(Properties.XP, xp + this.xpChange);

            worldModel.SetProperty(this.Target.name, false);
        }

        public override float GetHValue(WorldModel worldModel)
        {

            return base.GetHValue(worldModel) / 1.5f;

        }
    }
}
