namespace ModMail.Utilities
 {
     public class QueryResult
     {
         public bool IsSuccessful { get; }
         
         public string Message { get;}
 
         public QueryResult(bool success, string message)
         {
             IsSuccessful = success;
             Message = message;
         }
         
         public static QueryResult FromSuccess(string message = "No message specified.")
         {
             return new QueryResult(true, message);
         }
 
         public static QueryResult FromError(string message = "No message specified.")
         {
             return new QueryResult(false, message);
         }
     }
 }