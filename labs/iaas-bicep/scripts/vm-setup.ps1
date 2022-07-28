Install-WindowsFeature Web-Server,NET-Framework-45-ASPNET,Web-Asp-Net45

curl -o signup.msi https://github.com/courselabs/azure/releases/download/labs-iaas-apps-1.0/SignUp-1.0.msi

Start-Process msiexec.exe -ArgumentList '/i', 'signup.msi', '/quiet', '/norestart' -NoNewWindow -Wait

#TODO - edit connection string; parms for sql creds