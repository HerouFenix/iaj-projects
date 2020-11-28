using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.IAJ.Unity.Utils;

using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;

namespace Assets.Scripts.GameManager
{
    public class WorldModelFEAR : IWorldModel
    {
        private Dictionary<string, int> WorldIndexes { get; set; }

        private object[] World { get; set; }

        protected GameManager GameManager { get; set; }
        protected int NextPlayer { get; set; }
        protected Action NextEnemyAction { get; set; }
        protected Action[] NextEnemyActions { get; set; }

        private List<Action> Actions { get; set; }
        protected IEnumerator<Action> ActionEnumerator { get; set; }

        private Dictionary<string, float> GoalValues { get; set; }
        private Dictionary<string, Goal> Goals { get; set; }

        public WorldModelFEAR(GameManager gameManager, List<Action> actions, List<Goal> goals)
        {
            this.GameManager = gameManager;

            // Set the World

            this.World = new object[this.GameManager.disposableObjects.Count + 9];

            this.WorldIndexes = new Dictionary<string, int>();
            int indexCounter = 0;

            // Add Disposable Objects (Enemies + Chests + Potions)
            foreach (string obj in this.GameManager.disposableObjects.Keys)
            {
                for (int i = 0; i < this.GameManager.disposableObjects[obj].Count; i++)
                {
                    this.World[indexCounter] = true;
                    this.WorldIndexes.Add(this.GameManager.disposableObjects[obj][i].name, indexCounter);

                    indexCounter++;
                }
            }

            // Set the Character's properties
            // HP
            this.World[indexCounter] = this.GameManager.characterData.HP;
            this.WorldIndexes.Add(Properties.HP, indexCounter);
            indexCounter++;
            // Shield HP
            this.World[indexCounter] = this.GameManager.characterData.ShieldHP;
            this.WorldIndexes.Add(Properties.ShieldHP, indexCounter);
            indexCounter++;
            // MAXHP
            this.World[indexCounter] = this.GameManager.characterData.MaxHP;
            this.WorldIndexes.Add(Properties.MAXHP, indexCounter);
            indexCounter++;
            // Mana
            this.World[indexCounter] = this.GameManager.characterData.Mana;
            this.WorldIndexes.Add(Properties.MANA, indexCounter);
            indexCounter++;
            // Time
            this.World[indexCounter] = this.GameManager.characterData.Time;
            this.WorldIndexes.Add(Properties.TIME, indexCounter);
            indexCounter++;
            // Money
            this.World[indexCounter] = this.GameManager.characterData.Money;
            this.WorldIndexes.Add(Properties.MONEY, indexCounter);
            indexCounter++;
            // Level
            this.World[indexCounter] = this.GameManager.characterData.Level;
            this.WorldIndexes.Add(Properties.LEVEL, indexCounter);
            indexCounter++;
            // XP
            this.World[indexCounter] = this.GameManager.characterData.XP;
            this.WorldIndexes.Add(Properties.XP, indexCounter);
            indexCounter++;
            //Position
            this.World[indexCounter] = this.GameManager.characterData.CharacterGameObject.transform.position;
            this.WorldIndexes.Add(Properties.POSITION, indexCounter);


            this.NextPlayer = 0;

            this.GoalValues = new Dictionary<string, float>();
            this.Goals = new Dictionary<string, Goal>();
            foreach (var goal in goals)
            {
                this.Goals.Add(goal.Name, goal);
            }

            this.Actions = new List<Action>(actions);
            this.Actions.Shuffle();
            this.ActionEnumerator = this.Actions.GetEnumerator();
        }

        public WorldModelFEAR(WorldModelFEAR parent)
        {
            this.GameManager = parent.GameManager;
            this.World = (object[])parent.World.Clone();
            this.WorldIndexes = parent.WorldIndexes;

            this.GoalValues = new Dictionary<string, float>(parent.GoalValues);
            this.Goals = new Dictionary<string, Goal>(parent.Goals);

            this.Actions = new List<Action>(parent.Actions);
            this.Actions.Shuffle();
            this.ActionEnumerator = this.Actions.GetEnumerator();
        }

        public virtual object GetProperty(string propertyName)
        {
            if (this.WorldIndexes.ContainsKey(propertyName))
            {
                return this.World[this.WorldIndexes[propertyName]];
            }
            return null;
        }

        public virtual void SetProperty(string propertyName, object value)
        {
            if (this.WorldIndexes.ContainsKey(propertyName))
            {
                this.World[this.WorldIndexes[propertyName]] = value;
            }
        }

        public virtual float GetGoalValue(string goalName)
        {
            if (this.GoalValues.ContainsKey(goalName))
            {
                return this.GoalValues[goalName];
            }
            return 0;
        }

        public virtual void SetGoalValue(string goalName, float value)
        {
            var limitedValue = value;
            if (value > 10.0f)
            {
                limitedValue = 10.0f;
            }

            else if (value < 0.0f)
            {
                limitedValue = 0.0f;
            }

            this.GoalValues[goalName] = limitedValue;
        }

        public virtual IWorldModel GenerateChildWorldModel()
        {
            return new WorldModelFEAR(this);
        }

        public float CalculateDiscontentment(List<Goal> goals)
        {
            var discontentment = 0.0f;

            foreach (var goal in goals)
            {
                var newValue = this.GetGoalValue(goal.Name);

                discontentment += goal.GetDiscontentment(newValue);
            }

            return discontentment;
        }

        public float CalculateUtility(List<Goal> goals)
        {
            var utility = 0.0f;

            foreach (var goal in goals)
            {
                var newValue = this.GetGoalValue(goal.Name);

                utility += newValue;
            }

            return utility;
        }

        public virtual Action GetNextAction()
        {
            Action action = null;
            if (this.NextPlayer == 1)
            {
                action = this.NextEnemyAction;
                this.NextEnemyAction = null;
                return action;
            }
            else
            {
                //returns the next action that can be executed or null if no more executable actions exist
                if (this.ActionEnumerator.MoveNext())
                {
                    action = this.ActionEnumerator.Current;
                }

                while (action != null && !action.CanExecute(this))
                {
                    if (this.ActionEnumerator.MoveNext())
                    {
                        action = this.ActionEnumerator.Current;
                    }
                    else
                    {
                        action = null;
                    }
                }

                return action;
            }
        }

        public virtual Action[] GetExecutableActions()
        {
            if (this.NextPlayer == 1)
            {
                return this.NextEnemyActions;
            }
            else return this.Actions.Where(a => a.CanExecute(this)).ToArray();
        }

        public virtual bool IsTerminal()
        {
            int HP = (int)this.GetProperty(Properties.HP);
            float time = (float)this.GetProperty(Properties.TIME);
            int money = (int)this.GetProperty(Properties.MONEY);

            return HP <= 0 || time >= 200 || (this.NextPlayer == 0 && money == 25);
        }


        public virtual float GetScore()
        {
            int money = (int)this.GetProperty(Properties.MONEY);
            int HP = (int)this.GetProperty(Properties.HP);

            if (HP <= 0) return 0.0f;
            else if (money == 25)
            {
                return 1.0f;
            }
            else return 0.0f;
        }

        public virtual int GetNextPlayer()
        {
            return this.NextPlayer;
        }

        public virtual void CalculateNextPlayer()
        {
            Vector3 position = (Vector3)this.GetProperty(Properties.POSITION);
            bool enemyEnabled;

            //basically if the character is close enough to an enemy, the next player will be the enemy.
            foreach (var enemy in this.GameManager.enemies)
            {
                enemyEnabled = (bool)this.GetProperty(enemy.name);
                if (enemyEnabled && (enemy.transform.position - position).sqrMagnitude <= 100)
                {
                    this.NextPlayer = 1;
                    this.NextEnemyAction = new SwordAttack(this.GameManager.autonomousCharacter, enemy);
                    this.NextEnemyActions = new Action[] { this.NextEnemyAction };
                    return;
                }
            }
            this.NextPlayer = 0;
            //if not, then the next player will be player 0
        }

        public void Initialize()
        {
            this.ActionEnumerator.Reset();
        }
    }
}
