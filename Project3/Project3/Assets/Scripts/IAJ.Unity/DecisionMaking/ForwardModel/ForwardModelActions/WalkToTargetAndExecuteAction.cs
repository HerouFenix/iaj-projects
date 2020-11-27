using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using UnityEngine;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.IAJ.Unity.Movement.DynamicMovement;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public abstract class WalkToTargetAndExecuteAction : Action
    {
        protected AutonomousCharacter Character { get; set; }

        public GameObject Target { get; set; }

        protected WalkToTargetAndExecuteAction(string actionName, AutonomousCharacter character, GameObject target) : base(actionName + "(" + target.name + ")")
        {
            this.Character = character;
            this.Target = target;
        }

        public override float GetDuration()
        {
            return this.GetDuration(this.Character.Character.KinematicData.position);
        }

        public override float GetDuration(WorldModel worldModel)
        {
            var position = (Vector3)worldModel.GetProperty(Properties.POSITION);
            return this.GetDuration(position);
        }

        private float GetDuration(Vector3 currentPosition)
        {
            var distance = getDistance(currentPosition, Target.transform.position);
            return distance / this.Character.Character.MaxSpeed;
        }

        public override bool CanExecute()
        {
            return this.Target != null;
        }

        public override bool CanExecute(WorldModel worldModel)
        {
            if (this.Target == null) return false;
            var targetEnabled = (bool)worldModel.GetProperty(this.Target.name);
            return targetEnabled;
        }

        public override void Execute()
        {
            float distance = this.getDistance(this.Character.gameObject.transform.position, Target.transform.position);

            if (distance <= 0.5)
            {
                this.Character.Character.Movement = null;
            }
            else
            {
                this.Character.StartPathfinding(this.Target.transform.position);
            }
        }


        public override void ApplyActionEffects(WorldModel worldModel)
        {
            var duration = this.GetDuration(worldModel);

            var quicknessValue = worldModel.GetGoalValue(AutonomousCharacter.BE_QUICK_GOAL);
            worldModel.SetGoalValue(AutonomousCharacter.BE_QUICK_GOAL, quicknessValue + duration * 0.1f);

            var time = (float)worldModel.GetProperty(Properties.TIME);
            worldModel.SetProperty(Properties.TIME, time + duration);

            worldModel.SetProperty(Properties.POSITION, Target.transform.position);
        }

        public override float GetHValue(WorldModel worldModel)
        {
            var position = (Vector3)worldModel.GetProperty(Properties.POSITION);
            var distance = getDistance(position, Target.transform.position);
            return distance* 1/25.0f;
        }

        private float getDistance(Vector3 currentPosition, Vector3 targetPosition)
        {
            int cX, cY, tX, tY;
            var grid = this.Character.AStarPathFinding.grid;
            grid.GetXY(currentPosition, out cX, out cY);
            grid.GetXY(targetPosition, out tX, out tY);

            var distance = this.Character.AStarPathFinding.Heuristic.H(grid.GetGridObject(cX, cY), grid.GetGridObject(tX, tY));
            return distance;
        }
    }
}