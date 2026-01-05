namespace Board
{
    public interface IFactory<out T, in TContext>
    {
        T Create(TContext context);
    }
}