
namespace Ma.TimeManagement.Exceptions
{
    [Serializable]
    public class EntityExistException : Exception
    {
        public EntityExistException()
        {
        }

        public EntityExistException(string? message) : base(message)
        {
        }

        public EntityExistException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}