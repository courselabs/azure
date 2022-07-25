using Microsoft.SqlServer.Server; 
using System; 
using System.Data.SqlClient;  
  
public class FormattedDate
{  
    [SqlFunction(DataAccess = DataAccessKind.Read)]  
    public static string LegacySystem()  
    {  
        using (var conn = new SqlConnection("context connection=true"))  
        {  
            conn.Open();  
            var cmd = new SqlCommand("SELECT GETDATE()", conn);  
            var now = (DateTime)cmd.ExecuteScalar();  
            return now.ToString("1yyMMdd");
        }  
    }  
}