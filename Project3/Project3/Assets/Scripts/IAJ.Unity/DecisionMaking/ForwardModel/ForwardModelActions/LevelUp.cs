using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.GameManager;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class LevelUp : Action
    {
        public AutonomousCharacter Character { get; private set; }

        public LevelUp(AutonomousCharacter character) : base("LevelUp")
        {
            this.Character = character;
        }

        public override bool CanExecute()
        {
            var level = this.Character.GameManager.characterData.Level;
            var xp = this.Character.GameManager.characterData.XP;

            return xp >= level * 10;
        }
        

        public override bool CanExecute(IWorldModel IWorldModel)
        {
            int xp = (int)IWorldModel.GetProperty(Properties.XP);
            int level = (int)IWorldModel.GetProperty(Properties.LEVEL);

            return xp >= level * 10;
        }

        public override void Execute()
        {
            this.Character.GameManager.LevelUp();
        }

        public override void ApplyActionEffects(IWorldModel IWorldModel)
        {
            int maxHP = (int)IWorldModel.GetProperty(Properties.MAXHP);
            int level = (int)IWorldModel.GetProperty(Properties.LEVEL);

            IWorldModel.SetProperty(Properties.LEVEL, level + 1);
            IWorldModel.SetProperty(Properties.MAXHP, maxHP + 10);
            IWorldModel.SetProperty(Properties.XP, (int)0);
            IWorldModel.SetGoalValue(AutonomousCharacter.GAIN_LEVEL_GOAL, 0);
        }

        public override float GetHValue(IWorldModel IWorldModel)
        {
            //you would be dumb not to level up if possible
            return -100.0f;
        }
    }
}
