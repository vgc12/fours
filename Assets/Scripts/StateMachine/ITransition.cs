namespace StateMachines
{
    public interface ITransition
    {
        public IPredicate Predicate { get; }
        public IState To { get; }

    }
}