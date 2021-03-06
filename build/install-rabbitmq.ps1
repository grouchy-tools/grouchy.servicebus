Write-Host "Installing RabbitMQ..." -ForegroundColor Cyan

Write-Host "Downloading..."
$exePath = "$($env:USERPROFILE)\rabbitmq-server-3.6.15.exe"
(New-Object Net.WebClient).DownloadFile('http://www.rabbitmq.com/releases/rabbitmq-server/v3.6.15/rabbitmq-server-3.6.15.exe', $exePath)

Write-Host "Installing..."
cmd /c start /wait $exePath /S

$rabbitPath = 'C:\Program Files\RabbitMQ Server\rabbitmq_server-3.6.15'

Write-Host "Installing service..."
Start-Process -Wait "$rabbitPath\sbin\rabbitmq-service.bat" "install"

Write-Host "Starting service..."
Start-Process -Wait "$rabbitPath\sbin\rabbitmq-service.bat" "start"

Write-Host "Waiting for service availability..."
# TODO: Could investigate lower values here, 10 is not long enough
Start-Sleep -s 30 # Could wait for management UI if plugin was available.

Get-Service "RabbitMQ"

Write-Host "RabbitMQ installed and started" -ForegroundColor Green
