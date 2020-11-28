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


        public override bool CanExecute(IWorldModel IWorldModel)
        {
            return true;
        }

        public override void Execute()
        {
            this.Character.GameManager.Rest();
        }

        public override void ApplyActionEffects(IWorldModel IWorldModel)
        {
            var duration = 5;
            
            var quicknessValue = IWorldModel.GetGoalValue(AutonomousCharacter.BE_QUICK_GOAL);
            IWorldModel.SetGoalValue(AutonomousCharacter.BE_QUICK_GOAL, quicknessValue + duration * 0.1f);

            var expectedHPGain = 2;

            var surviveValue = IWorldModel.GetGoalValue(AutonomousCharacter.SURVIVE_GOAL) - expectedHPGain;

            if (surviveValue < 0)
            {
                surviveValue = 0;
            }
            IWorldModel.SetGoalValue(AutonomousCharacter.SURVIVE_GOAL, surviveValue);

            int newHP = (int)IWorldModel.GetProperty(Properties.HP) + 2;
            if (newHP > (int)IWorldModel.GetProperty(Properties.MAXHP))
            {
                newHP = (int)IWorldModel.GetProperty(Properties.MAXHP);
            }

            IWorldModel.SetProperty(Properties.HP, newHP);
        }

        public override float GetHValue(IWorldModel IWorldModel)
        {
            var curHP = (int)IWorldModel.GetProperty(Properties.HP);
            if (curHP < 5)
            {
                return 4.0f;
            }else if(curHP > (int)IWorldModel.GetProperty(Properties.MAXHP)-2)
            {
                return 10.0f;
            }else if(curHP == (int)IWorldModel.GetProperty(Properties.MAXHP))
            {
                return 100.0f;
            }
            return 7.0f;
        }
    }
}
