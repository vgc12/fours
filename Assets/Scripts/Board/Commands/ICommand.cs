using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace Board.Commands
{
    public interface ICommand
    {
        UniTask<bool> Execute();
        UniTask<bool> Undo();
        string Description { get; }
        bool CanUndo { get; }
    }
}