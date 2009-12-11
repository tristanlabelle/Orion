using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;

namespace Orion.Commandment.Commands.Pipeline
{
    public class CommandOptimizer : CommandFilter
    {
        private Stack<Command> commands = new Stack<Command>();
        private List<Handle> concernedHandles = new List<Handle>();

        public override void Handle(Command command)
        {
            commands.Push(command);
        }

        public override void Update(int updateNumber, float timeDeltaInSeconds)
        {
            while (commands.Count > 0)
            {
                Command command = commands.Pop();
                IEnumerable<Handle> handles = command.ExecutingEntityHandles;
                IEnumerable<Handle> availableUnits = handles.Except(concernedHandles);

                if (availableUnits.Count() > 0)
                {
                    #region Move Command
                    MoveCommand move = command as MoveCommand;
                    if (move != null)
                    {
                        MoveCommand newCommand = new MoveCommand(move.FactionHandle, availableUnits, move.Destination);
                        FlushNew(newCommand);
                        continue;
                    }
                    #endregion

                    #region Attack Command
                    AttackCommand attack = command as AttackCommand;
                    if (attack != null)
                    {
                        AttackCommand newCommand = new AttackCommand(attack.FactionHandle, availableUnits, attack.TargetHandle);
                        FlushNew(newCommand);
                        continue;
                    }
                    #endregion

                    #region Build Command
                    BuildCommand build = command as BuildCommand;
                    if (build != null)
                    {
                        BuildCommand newCommand = new BuildCommand(build.FactionHandle, availableUnits, build.BuildingTypeHandle, build.Destination);
                        FlushNew(newCommand);
                        continue;
                    }
                    #endregion

                    #region Harvest Command
                    HarvestCommand harvest = command as HarvestCommand;
                    if (harvest != null)
                    {
                        HarvestCommand newCommand = new HarvestCommand(harvest.FactionHandle, availableUnits, harvest.TargetHandle);
                        FlushNew(newCommand);
                        continue;
                    }
                    #endregion

                    #region Heal Command
                    HealCommand heal = command as HealCommand;
                    if (heal != null)
                    {
                        HealCommand newCommand = new HealCommand(heal.FactionHandle, availableUnits, heal.TargetHandle);
                        FlushNew(newCommand);
                        continue;
                    }
                    #endregion

                    #region Repair Command
                    RepairCommand repair = command as RepairCommand;
                    if (repair != null)
                    {
                        RepairCommand newCommand = new RepairCommand(repair.FactionHandle, availableUnits, repair.TargetHandle);
                        FlushNew(newCommand);
                        continue;
                    }
                    #endregion

                    #region Zone Attack Command
                    ZoneAttackCommand zoneAttack = command as ZoneAttackCommand;
                    if (zoneAttack != null)
                    {
                        ZoneAttackCommand newCommand = new ZoneAttackCommand(zoneAttack.FactionHandle, availableUnits, zoneAttack.Destination);
                        FlushNew(newCommand);
                        continue;
                    }
                    #endregion

                    #region Otherwise
                    Flush(command);
                    #endregion
                }
            }
            concernedHandles.Clear();
        }

        private void FlushNew(Command command)
        {
            concernedHandles.AddRange(command.ExecutingEntityHandles);
            Flush(command);
        }
    }
}
