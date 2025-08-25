namespace StateMachines
{
    public class Transition : ITransition
    {
        public IPredicate Predicate { get; }
        public IState To { get; }

        public Transition(IState to, IPredicate predicate)
        {
            Predicate = predicate;
            To = to;
        }
    }
}