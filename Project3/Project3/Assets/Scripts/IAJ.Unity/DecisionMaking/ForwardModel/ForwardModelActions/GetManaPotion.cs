using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class GetManaPotion : WalkToTargetAndExecuteAction
    {
        public GetManaPotion(AutonomousCharacter character, GameObject target) : base("GetManaPotion", character, target)
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
            this.Character.GameManager.GetManaPotion(this.Target);
        }

        public override void ApplyActionEffects(IWorldModel IWorldModel)
        {
            base.ApplyActionEffects(IWorldModel);

            IWorldModel.SetProperty(Properties.MANA, 10);

            //disables the target object so that it can't be reused again
            IWorldModel.SetProperty(this.Target.name, false);
        }



        public override float GetHValue(IWorldModel IWorldModel)
        {
            int addedMana = 10 - (int)IWorldModel.GetProperty(Properties.MANA);
            if (addedMana < 3)
            { // Makes no sense to try to go get a mana potion when you still got so much
                return 1000f;
            }

            return base.GetHValue(IWorldModel) / (addedMana * 2.0f); //The more Mana we add, the smaller the HValue
        }
    }
}
