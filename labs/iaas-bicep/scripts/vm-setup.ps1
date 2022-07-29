param(
    [string] $sqlServerName,
    [string] $sqlPassword
)

Start-Transcript -path C:\vm-setup.log

Write-Output '* Setup starting'

Write-Output '** Installing Windows features'
Install-WindowsFeature Web-Server,NET-Framework-45-ASPNET,Web-Asp-Net45

Write-Output '** Downloading app MSI'
curl -o signup.msi https://github.com/courselabs/azure/releases/download/labs-iaas-apps-1.0/SignUp-1.0.msi

Write-Output '** Installing app'
Start-Process msiexec.exe -ArgumentList '/i', 'signup.msi', '/quiet', '/norestart' -NoNewWindow -Wait

$configTemplate = '<connectionStrings><add name="SignUpDb" connectionString="Server=[SQL_SERVER].database.windows.net;Initial Catalog=signup-db;Persist Security Info=False;User ID=sqladmin;Password=[SQL_PASSWORD];MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" providerName="System.Data.SqlClient"/></connectionStrings>'
$config=$configTemplate.Replace("[SQL_SERVER]", $sqlServerName).Replace("[SQL_PASSWORD]", $sqlPassword)
$configPath='C:\docker4.net\SignUp.Web\connectionStrings.config'

Write-Output '** Writing config file'
[IO.File]::Move($configPath, "${configPath}.bak")
[IO.File]::WriteAllLines($configPath, $config)

Write-Output '* Setup done'

Stop-Transcript