using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Board.Commands
{
    public class MacroCommand : BaseCommand
    {
        private readonly List<ICommand> _commands;
        private readonly List<ICommand> _executedCommands;

        public override string Description { get; }

        public MacroCommand(string description, params ICommand[] commands)
        {
            Description = description;
            _commands = new List<ICommand>(commands);
            _executedCommands = new List<ICommand>();
        }

        public override async Task<bool> Execute()
        {
            _executedCommands.Clear();
            
            foreach (var command in _commands)
            {
                var success = await command.Execute();
                if (success)
                {
                    _executedCommands.Add(command);
                }
                else
                {
                    // If any command fails, undo all executed commands
                    await UndoExecutedCommands();
                    return false;
                }
            }
            
            return true;
        }

        public override async Task<bool> Undo()
        {
            return await UndoExecutedCommands();
        }

        private async Task<bool> UndoExecutedCommands()
        {
            bool allSucceeded = true;
            
            // Undo in reverse order
            for (int i = _executedCommands.Count - 1; i >= 0; i--)
            {
                var command = _executedCommands[i];
                if (command.CanUndo)
                {
                    var success = await command.Undo();
                    if (!success)
                    {
                        allSucceeded = false;
                        Debug.LogError($"Failed to undo command: {command.Description}");
                    }
                }
            }
            
            if (allSucceeded)
            {
                _executedCommands.Clear();
            }
            
            return allSucceeded;
        }
    }
}