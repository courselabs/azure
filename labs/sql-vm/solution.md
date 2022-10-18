# Lab Solution

Using SSMS on the SQL VM, create a new login with a strong password with the [CREATE LOGIN](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-login-transact-sql?view=sql-server-ver16) statement:

```
CREATE LOGIN labs2   
   WITH PASSWORD = '00234$$$jhjhj' 
GO  
```

The SQL VM is already configured for public access, so you can connect. If you try to use the UDF:

```
SELECT dbo.LegacyDate() 
```

You'll see an error:

_The EXECUTE permission was denied on the object 'LegacyDate', database 'master', schema 'dbo'_

So back in the VM you need to [grant object permission](https://learn.microsoft.com/en-us/sql/t-sql/statements/grant-object-permissions-transact-sql?view=sql-server-ver16) - but permissions are granted to a user, so you need to create a user for your login first:

```
CREATE USER labs2 FOR LOGIN labs2

GRANT EXECUTE ON LegacyDate TO labs2
```

Now in your remote session you can execute the UDF. 