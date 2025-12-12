using System.Threading.Tasks;

namespace Board.Commands
{
    public abstract class BaseCommand : ICommand
    {
        public abstract string Description { get; }
        public virtual bool CanUndo => true;
        
        public abstract Task<bool> Execute();
        public abstract Task<bool> Undo();
    }
}