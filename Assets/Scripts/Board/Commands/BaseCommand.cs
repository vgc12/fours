using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace Board.Commands
{
    public abstract class BaseCommand : ICommand
    {
        public abstract string Description { get; }
        public virtual bool CanUndo => true;
        
        public abstract UniTask<bool> Execute();
        public abstract UniTask<bool> Undo();
    }
}