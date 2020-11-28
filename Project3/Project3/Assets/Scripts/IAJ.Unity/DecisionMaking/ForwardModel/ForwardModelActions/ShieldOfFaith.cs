using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.GameManager;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class ShieldOfFaith : Action
    {
        public AutonomousCharacter Character { get; private set; }

        public ShieldOfFaith(AutonomousCharacter character) : base("ShieldOfFaith")
        {
            this.Character = character;
        }

        public override bool CanExecute()
        {
            return this.Character.GameManager.characterData.Mana >= 5;
        }


        public override bool CanExecute(IWorldModel IWorldModel)
        {
            return (int)IWorldModel.GetProperty(Properties.MANA) >= 5;
        }

        public override void Execute()
        {
            this.Character.GameManager.ShieldOfFaith();
        }

        public override void ApplyActionEffects(IWorldModel IWorldModel)
        {
            base.ApplyActionEffects(IWorldModel);

            var expectedHPGain = 5 - (int)IWorldModel.GetProperty(Properties.ShieldHP);

            var surviveValue = IWorldModel.GetGoalValue(AutonomousCharacter.SURVIVE_GOAL);
            IWorldModel.SetGoalValue(AutonomousCharacter.SURVIVE_GOAL, surviveValue - expectedHPGain);

            IWorldModel.SetProperty(Properties.ShieldHP, 5);
            IWorldModel.SetProperty(Properties.MANA, (int)IWorldModel.GetProperty(Properties.MANA)-5);
        }

        public override float GetHValue(IWorldModel IWorldModel)
        {
            int addedHP = 5 - (int)IWorldModel.GetProperty(Properties.ShieldHP);

            if (addedHP == 0)
            { // Makes no sense to try to go get a health potion when you're at max shield HP (i.e addedHP is 0)
                return 1000f;
            }
            return 1/(addedHP*2.0f); //The more HP we add, the smaller the HValue
        }
    }
}
