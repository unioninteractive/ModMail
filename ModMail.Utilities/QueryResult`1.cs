namespace ModMail.Utilities
{
    public class QueryResult<T>
    {
        public bool Successful { get; }
         
        public string Message { get; }
        
        public T Result { get;}
 
        public QueryResult(bool success, string message, T result)
        {
            Successful = success;
            Message = message;
            Result = result;
        }
        
        public static QueryResult<T> FromSuccess(string message = "No message specified.", T result = default(T))
        {
            return new QueryResult<T>(true, message, result);
        }

        public static QueryResult<T> FromError(string message = "No message specified.", T result = default(T))
        {
            return new QueryResult<T>(false, message, result);
        }
    }
}