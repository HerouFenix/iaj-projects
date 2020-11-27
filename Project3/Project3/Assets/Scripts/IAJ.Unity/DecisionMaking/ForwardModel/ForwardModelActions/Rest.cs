using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.GameManager;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class Rest : Action
    {
        public AutonomousCharacter Character { get; private set; }

        public Rest(AutonomousCharacter character) : base("Rest")
        {
            this.Character = character;
        }

        public override bool CanExecute()
        {
            return true;
        }


        public override bool CanExecute(WorldModel worldModel)
        {
            return true;
        }

        public override void Execute()
        {
            this.Character.GameManager.Rest();
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            var duration = 5;
            
            var quicknessValue = worldModel.GetGoalValue(AutonomousCharacter.BE_QUICK_GOAL);
            worldModel.SetGoalValue(AutonomousCharacter.BE_QUICK_GOAL, quicknessValue + duration * 0.1f);

            var expectedHPGain = 2;

            var surviveValue = worldModel.GetGoalValue(AutonomousCharacter.SURVIVE_GOAL) - expectedHPGain;

            if (surviveValue < 0)
            {
                surviveValue = 0;
            }
            worldModel.SetGoalValue(AutonomousCharacter.SURVIVE_GOAL, surviveValue);

            int newHP = (int)worldModel.GetProperty(Properties.HP) + 2;
            if (newHP > (int)worldModel.GetProperty(Properties.MAXHP))
            {
                newHP = (int)worldModel.GetProperty(Properties.MAXHP);
            }

            worldModel.SetProperty(Properties.HP, newHP);
        }

        public override float GetHValue(WorldModel worldModel)
        {
            var curHP = (int)worldModel.GetProperty(Properties.HP);
            if (curHP < 5)
            {
                return 3.0f;
            }else if(curHP > (int)worldModel.GetProperty(Properties.MAXHP)-2)
            {
                return 7.5f;
            }else if(curHP == (int)worldModel.GetProperty(Properties.MAXHP))
            {
                return 100.0f;
            }
            return 5.0f;
        }
    }
}
