using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.GameManager;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class Teleport : Action
    {
        public AutonomousCharacter Character { get; private set; }

        public Teleport(AutonomousCharacter character) : base("Teleport")
        {
            this.Character = character;
        }

        public override bool CanExecute()
        {
            int mana = (int)this.Character.GameManager.characterData.Mana;
            int level = (int)this.Character.GameManager.characterData.Level;

            return  level >= 2 && mana >= 5;
        }


        public override bool CanExecute(IWorldModel IWorldModel)
        {
            int mana = (int)IWorldModel.GetProperty(Properties.MANA);
            int level = (int)IWorldModel.GetProperty(Properties.LEVEL);

            return level >= 2 && mana >= 5;
        }

        public override void Execute()
        {
            this.Character.GameManager.Teleport();
        }

        public override void ApplyActionEffects(IWorldModel IWorldModel)
        {
            int mana = (int)IWorldModel.GetProperty(Properties.MANA);

            IWorldModel.SetProperty(Properties.POSITION, this.Character.GameManager.initialPosition);
            IWorldModel.SetProperty(Properties.MANA, mana - 5);
        }

        public override float GetHValue(IWorldModel IWorldModel)
        {
            // TODO
            return 0.0f;
        }
    }
}
