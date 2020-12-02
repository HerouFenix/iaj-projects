using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class PickUpChest : WalkToTargetAndExecuteAction
    {

        public PickUpChest(AutonomousCharacter character, GameObject target) : base("PickUpChest", character, target)
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

            if (!this.Character.GameManager.SleepingNPCs)
            {
                var enemies = IWorldModel.GetEnemies();

                var chestPosition = this.Target.transform.position;
                chestPosition.y = 0;
                var totalHP = (int)IWorldModel.GetProperty(Properties.HP) + (int)IWorldModel.GetProperty(Properties.ShieldHP);
                foreach (var enemy in enemies)
                {
                    var enemyPosition = enemy.transform.position;
                    enemyPosition.y = 0;
                    if (Vector3.Distance(chestPosition, enemyPosition) <= 50)
                    {
                        if (enemy.name.Contains("Dragon") && totalHP  < 14)
                        {
                            return 100f; // Dont get a chest next to a dragon if the HP is low
                        }
                        else if (enemy.name.Contains("Orc") && totalHP < 10)
                        {
                            return 100f; // Dont get a chest next to an orc if the HP is low
                        }
                        else if (enemy.name.Contains("Skeleton") && totalHP < 4)
                        {
                            return 100f; // Dont get a chest next to a spooky scary skelleton if the HP is low
                        }
                    }
                }
            }

            return base.GetHValue(IWorldModel) / 5.0f;
        }
    }
}
