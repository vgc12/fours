using System.Threading.Tasks;

namespace Board.Commands
{
    public interface ICommand
    {
        Task<bool> Execute();
        Task<bool> Undo();
        string Description { get; }
        bool CanUndo { get; }
    }
}