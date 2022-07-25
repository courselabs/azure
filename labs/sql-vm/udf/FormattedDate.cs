using System; 
  
public class FormattedDate
{  
    public static string LegacyNow()  
    { 
        return DateTime.Now.ToString("1yyMMdd");
    }  
}