using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class PickUpChest : WalkToTargetAndExecuteAction
    {

        public PickUpChest(AutonomousCharacter character, GameObject target) : base("PickUpChest",character,target)
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
            this.Character.GameManager.PickUpChest(this.Target);
        }

        public override void ApplyActionEffects(IWorldModel IWorldModel)
        {
            base.ApplyActionEffects(IWorldModel);

            var goalValue = IWorldModel.GetGoalValue(AutonomousCharacter.GET_RICH_GOAL);
            IWorldModel.SetGoalValue(AutonomousCharacter.GET_RICH_GOAL, goalValue - 5.0f);

            var money = (int)IWorldModel.GetProperty(Properties.MONEY);
            IWorldModel.SetProperty(Properties.MONEY, money + 5);

            //disables the target object so that it can't be reused again
            IWorldModel.SetProperty(this.Target.name, false);
        }

        public override float GetHValue(IWorldModel IWorldModel)
        {
            return base.GetHValue(IWorldModel)/5.0f;
        }
    }
}
