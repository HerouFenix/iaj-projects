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

        public override bool CanExecute(WorldModel worldModel)
        {
            if (!base.CanExecute(worldModel)) return false;
            return true;
        }

        public override void Execute()
        {

            base.Execute();
            this.Character.GameManager.GetHealthPotion(this.Target);
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

            var expectedHPGain = (int)worldModel.GetProperty(Properties.MAXHP) - (int)worldModel.GetProperty(Properties.HP);

            var surviveValue = worldModel.GetGoalValue(AutonomousCharacter.SURVIVE_GOAL);
            worldModel.SetGoalValue(AutonomousCharacter.SURVIVE_GOAL, surviveValue - expectedHPGain);


            worldModel.SetProperty(Properties.HP, (int)worldModel.GetProperty(Properties.MAXHP));

            //disables the target object so that it can't be reused again
            worldModel.SetProperty(this.Target.name, false);
        }



        public override float GetHValue(WorldModel worldModel)
        {
            float addedHP = (int)worldModel.GetProperty(Properties.MAXHP) - (int)worldModel.GetProperty(Properties.HP);
            float maxHP = (int)worldModel.GetProperty(Properties.MAXHP);
            float proportionCured = (addedHP / maxHP)*10.0f;
            if (addedHP < 3)
            { // Makes no sense to try to go get a health potion when you're at max HP (i.e addedHP is 0)
                return 1000f;
            }
            
            return base.GetHValue(worldModel) / (addedHP/2.0f); // ; //The more HP we add, the smaller the HValue
        }
    }
}
