namespace PresupuestosAPI.Exceptions
{
    public class PlanLimitExceededException : Exception
    {
        public PlanLimitExceededException(string message) : base(message)
        {
        }
    }
}