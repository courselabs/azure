Write-Output '* Installing Chocolatey'
Set-ExecutionPolicy Bypass -Scope Process -Force 
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
Invoke-Expression ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))

Write-Output '* Installing tools'
choco install -y git
choco install -y vscode

Write-Output '-VM setup script done-'