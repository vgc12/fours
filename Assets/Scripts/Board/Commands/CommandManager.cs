using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Board.Commands
{
    public sealed class CommandManager
    {
        private readonly Stack<ICommand> _undoStack;
        private readonly Stack<ICommand> _redoStack;
        private readonly int _maxUndoHistory;

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;
        public int UndoStackCount => _undoStack.Count;

        public event Action<ICommand> OnCommandExecuted;
        public event Action<ICommand> OnCommandUndone;
        public event Action<ICommand> OnCommandRedone;

        public CommandManager(int maxUndoHistory = 50)
        {
            _maxUndoHistory = maxUndoHistory;
            _undoStack = new Stack<ICommand>();
            _redoStack = new Stack<ICommand>();
        }

        public async Task<bool> ExecuteCommand(ICommand command)
        {
            if (command == null) return false;

            var success = await command.Execute();
            if (success)
            {
                _undoStack.Push(command);
                _redoStack.Clear(); // Clear redo stack when new command is executed
                
                // Limit undo history size
                while (_undoStack.Count > _maxUndoHistory)
                {
                    var oldCommands = new ICommand[_undoStack.Count];
                    _undoStack.CopyTo(oldCommands, 0);
                    _undoStack.Clear();
                    
                    for (int i = 1; i < oldCommands.Length; i++)
                    {
                        _undoStack.Push(oldCommands[i]);
                    }
                }
                
                OnCommandExecuted?.Invoke(command);
                Debug.Log($"Executed command: {command.Description}");
            }
            else
            {
                Debug.LogError($"Failed to execute command: {command.Description}");
            }

            return success;
        }

        public async Task<bool> UndoLastCommand()
        {
            if (!CanUndo) return false;

            var command = _undoStack.Pop();
            if (!command.CanUndo)
            {
                Debug.LogWarning($"Command cannot be undone: {command.Description}");
                return false;
            }

            var success = await command.Undo();
            if (success)
            {
                _redoStack.Push(command);
                OnCommandUndone?.Invoke(command);
                Debug.Log($"Undone command: {command.Description}");
            }
            else
            {
                // If undo failed, put the command back
                _undoStack.Push(command);
                Debug.LogError($"Failed to undo command: {command.Description}");
            }

            return success;
        }

        public async Task<bool> RedoLastCommand()
        {
            if (!CanRedo) return false;

            var command = _redoStack.Pop();
            var success = await command.Execute();
            
            if (success)
            {
                _undoStack.Push(command);
                OnCommandRedone?.Invoke(command);
                Debug.Log($"Redone command: {command.Description}");
            }
            else
            {
                // If redo failed, put the command back
                _redoStack.Push(command);
                Debug.LogError($"Failed to redo command: {command.Description}");
            }

            return success;
        }

        public void ClearHistory()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            Debug.Log("Command history cleared");
        }

        public List<string> GetUndoHistory(int count = 10)
        {
            var history = new List<string>();
            var commands = _undoStack.ToArray();
            
            for (int i = 0; i < Math.Min(count, commands.Length); i++)
            {
                history.Add(commands[i].Description);
            }
            
            return history;
        }
    }
}