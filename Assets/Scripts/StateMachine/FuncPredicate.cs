namespace StateMachines
{
    public class FuncPredicate : IPredicate
    {
        private readonly System.Func<bool> _predicate;
        public FuncPredicate(System.Func<bool> predicate) => _predicate = predicate;

        public bool Evaluate() => _predicate.Invoke();
    }
}