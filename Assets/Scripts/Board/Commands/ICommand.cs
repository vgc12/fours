using System.Threading.Tasks;

namespace Board
{
    public interface ICommand
    {
        Task<bool> Execute();
        Task<bool> Undo();
        string Description { get; }
        bool CanUndo { get; }
    }
}