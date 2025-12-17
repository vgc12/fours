namespace StateMachine
{
    public interface ITransition
    {
        public IPredicate Predicate { get; }
        public IState To { get; }

    }
}