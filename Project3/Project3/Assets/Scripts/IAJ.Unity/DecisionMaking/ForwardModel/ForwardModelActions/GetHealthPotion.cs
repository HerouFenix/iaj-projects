using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class GetHealthPotion : WalkToTargetAndExecuteAction
    {
        public GetHealthPotion(AutonomousCharacter character, GameObject target) : base("GetHealthPotion", character, target)
        {

        }

        public override bool CanExecute()
        {

            if (!base.CanExecute())
                return false;
            return true;
        }

        public override bool CanExecute(IWorldModel IWorldModel)
        {
            if (!base.CanExecute(IWorldModel)) return false;
            return true;
        }

        public override void Execute()
        {

            base.Execute();
            this.Character.GameManager.GetHealthPotion(this.Target);
        }

        public override void ApplyActionEffects(IWorldModel IWorldModel)
        {
            base.ApplyActionEffects(IWorldModel);

            var expectedHPGain = (int)IWorldModel.GetProperty(Properties.MAXHP) - (int)IWorldModel.GetProperty(Properties.HP);

            var surviveValue = IWorldModel.GetGoalValue(AutonomousCharacter.SURVIVE_GOAL);
            IWorldModel.SetGoalValue(AutonomousCharacter.SURVIVE_GOAL, surviveValue - expectedHPGain);


            IWorldModel.SetProperty(Properties.HP, (int)IWorldModel.GetProperty(Properties.MAXHP));

            //disables the target object so that it can't be reused again
            IWorldModel.SetProperty(this.Target.name, false);
        }



        public override float GetHValue(IWorldModel IWorldModel)
        {
            float addedHP = (int)IWorldModel.GetProperty(Properties.MAXHP) - (int)IWorldModel.GetProperty(Properties.HP);
            float maxHP = (int)IWorldModel.GetProperty(Properties.MAXHP);
            float proportionCured = (addedHP / maxHP)*10.0f;
            if (addedHP < 3)
            { // Makes no sense to try to go get a health potion when you're at max HP (i.e addedHP is 0)
                return 1000f;
            }
            
            return base.GetHValue(IWorldModel) / (addedHP/2.0f); // ; //The more HP we add, the smaller the HValue
        }
    }
}
