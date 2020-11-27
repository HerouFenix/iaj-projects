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
        

        public override bool CanExecute(WorldModel worldModel)
        {
            int xp = (int)worldModel.GetProperty(Properties.XP);
            int level = (int)worldModel.GetProperty(Properties.LEVEL);

            return xp >= level * 10;
        }

        public override void Execute()
        {
            this.Character.GameManager.LevelUp();
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            int maxHP = (int)worldModel.GetProperty(Properties.MAXHP);
            int level = (int)worldModel.GetProperty(Properties.LEVEL);

            worldModel.SetProperty(Properties.LEVEL, level + 1);
            worldModel.SetProperty(Properties.MAXHP, maxHP + 10);
            worldModel.SetProperty(Properties.XP, (int)0);
            worldModel.SetGoalValue(AutonomousCharacter.GAIN_LEVEL_GOAL, 0);
        }

        public override float GetHValue(WorldModel worldModel)
        {
            //you would be dumb not to level up if possible
            return -100.0f;
        }
    }
}
