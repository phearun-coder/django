# ProductInventoryAPI Deployment Script
# This script automates the deployment of the Product Inventory API to IIS

param(
    [Parameter(Mandatory=$true)]
    [string]$TargetServer,
    
    [Parameter(Mandatory=$true)]
    [string]$SiteName,
    
    [Parameter(Mandatory=$false)]
    [string]$ApplicationPool = "ProductInventoryAPI",
    
    [Parameter(Mandatory=$false)]
    [string]$DeploymentPath = "C:\inetpub\wwwroot\ProductInventoryAPI",
    
    [Parameter(Mandatory=$false)]
    [string]$BackupPath = "C:\Deployments\Backups",
    
    [Parameter(Mandatory=$false)]
    [string]$Configuration = "Release",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipBackup,
    
    [Parameter(Mandatory=$false)]
    [switch]$RunTests
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Define paths
$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptRoot
$PublishPath = "$ProjectRoot\publish"
$LogPath = "$ScriptRoot\logs"
$LogFile = "$LogPath\deployment-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"

# Ensure log directory exists
if (!(Test-Path $LogPath)) {
    New-Item -ItemType Directory -Path $LogPath -Force | Out-Null
}

# Logging function
function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $LogEntry = "[$Timestamp] [$Level] $Message"
    Write-Host $LogEntry
    Add-Content -Path $LogFile -Value $LogEntry
}

# Function to check prerequisites
function Test-Prerequisites {
    Write-Log "Checking prerequisites..."
    
    # Check if .NET 8 SDK is installed
    $dotnetVersion = dotnet --version 2>$null
    if ($LASTEXITCODE -ne 0) {
        throw ".NET SDK not found. Please install .NET 8 SDK."
    }
    Write-Log ".NET SDK version: $dotnetVersion"
    
    # Check if IIS Management Tools are available
    $iisFeature = Get-WindowsFeature -Name IIS-ManagementConsole 2>$null
    if ($iisFeature -and $iisFeature.InstallState -ne "Installed") {
        Write-Log "IIS Management Console not installed" "WARNING"
    }
    
    # Check if ASP.NET Core Module is installed
    $aspNetCoreModule = Get-Module -ListAvailable -Name IISAdministration 2>$null
    if (!$aspNetCoreModule) {
        Write-Log "IIS Administration module not available" "WARNING"
    }
    
    Write-Log "Prerequisites check completed"
}

# Function to run tests
function Invoke-Tests {
    if ($RunTests) {
        Write-Log "Running unit tests..."
        
        Push-Location "$ProjectRoot"
        try {
            # Run unit tests
            dotnet test tests/ProductInventoryAPI.UnitTests/ --configuration $Configuration --logger "console;verbosity=minimal"
            if ($LASTEXITCODE -ne 0) {
                throw "Unit tests failed"
            }
            
            # Run integration tests (if available)
            if (Test-Path "tests/ProductInventoryAPI.IntegrationTests/") {
                dotnet test tests/ProductInventoryAPI.IntegrationTests/ --configuration $Configuration --logger "console;verbosity=minimal"
                if ($LASTEXITCODE -ne 0) {
                    Write-Log "Integration tests failed" "WARNING"
                }
            }
            
            Write-Log "Tests completed successfully"
        }
        finally {
            Pop-Location
        }
    }
}

# Function to build and publish application
function Publish-Application {
    Write-Log "Building and publishing application..."
    
    Push-Location "$ProjectRoot"
    try {
        # Clean previous publish
        if (Test-Path $PublishPath) {
            Remove-Item -Recurse -Force $PublishPath
        }
        
        # Restore packages
        Write-Log "Restoring NuGet packages..."
        dotnet restore
        if ($LASTEXITCODE -ne 0) {
            throw "Package restore failed"
        }
        
        # Build solution
        Write-Log "Building solution..."
        dotnet build --configuration $Configuration --no-restore
        if ($LASTEXITCODE -ne 0) {
            throw "Build failed"
        }
        
        # Publish application
        Write-Log "Publishing application..."
        dotnet publish src/ProductInventoryAPI.Web/ `
            --configuration $Configuration `
            --output $PublishPath `
            --no-build `
            --verbosity minimal
        
        if ($LASTEXITCODE -ne 0) {
            throw "Publish failed"
        }
        
        # Copy deployment configuration files
        Write-Log "Copying deployment configuration files..."
        Copy-Item "$ScriptRoot\web.config" "$PublishPath\" -Force
        Copy-Item "$ScriptRoot\appsettings.Production.json" "$PublishPath\" -Force
        
        Write-Log "Application published successfully to: $PublishPath"
    }
    finally {
        Pop-Location
    }
}

# Function to create backup
function New-Backup {
    if (!$SkipBackup -and (Test-Path $DeploymentPath)) {
        Write-Log "Creating backup..."
        
        $BackupTimestamp = Get-Date -Format "yyyyMMdd-HHmmss"
        $BackupName = "$SiteName-$BackupTimestamp"
        $BackupFullPath = "$BackupPath\$BackupName"
        
        # Ensure backup directory exists
        if (!(Test-Path $BackupPath)) {
            New-Item -ItemType Directory -Path $BackupPath -Force | Out-Null
        }
        
        # Create backup
        Copy-Item -Recurse $DeploymentPath $BackupFullPath
        
        Write-Log "Backup created: $BackupFullPath"
        
        # Clean old backups (keep last 5)
        $OldBackups = Get-ChildItem $BackupPath | 
                     Where-Object { $_.Name -like "$SiteName-*" } | 
                     Sort-Object CreationTime -Descending | 
                     Select-Object -Skip 5
        
        foreach ($OldBackup in $OldBackups) {
            Remove-Item -Recurse -Force $OldBackup.FullName
            Write-Log "Removed old backup: $($OldBackup.Name)"
        }
    }
}

# Function to stop application pool and site
function Stop-IISServices {
    Write-Log "Stopping IIS services..."
    
    try {
        # Import IIS module
        Import-Module WebAdministration -ErrorAction SilentlyContinue
        
        # Stop application pool
        if (Get-IISAppPool -Name $ApplicationPool -ErrorAction SilentlyContinue) {
            Stop-WebAppPool -Name $ApplicationPool -ErrorAction SilentlyContinue
            Write-Log "Stopped application pool: $ApplicationPool"
        }
        
        # Stop website
        if (Get-Website -Name $SiteName -ErrorAction SilentlyContinue) {
            Stop-Website -Name $SiteName -ErrorAction SilentlyContinue
            Write-Log "Stopped website: $SiteName"
        }
        
        # Wait for processes to stop
        Start-Sleep -Seconds 5
    }
    catch {
        Write-Log "Error stopping IIS services: $($_.Exception.Message)" "WARNING"
    }
}

# Function to deploy application files
function Deploy-Files {
    Write-Log "Deploying application files..."
    
    # Ensure deployment directory exists
    if (!(Test-Path $DeploymentPath)) {
        New-Item -ItemType Directory -Path $DeploymentPath -Force | Out-Null
    }
    
    # Clear deployment directory (except logs)
    Get-ChildItem $DeploymentPath | 
        Where-Object { $_.Name -ne "logs" } | 
        Remove-Item -Recurse -Force
    
    # Copy published files
    Copy-Item -Recurse "$PublishPath\*" $DeploymentPath -Force
    
    # Ensure logs directory exists
    $LogsPath = "$DeploymentPath\logs"
    if (!(Test-Path $LogsPath)) {
        New-Item -ItemType Directory -Path $LogsPath -Force | Out-Null
    }
    
    # Set permissions for IIS_IUSRS
    try {
        $acl = Get-Acl $DeploymentPath
        $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS_IUSRS", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
        $acl.SetAccessRule($accessRule)
        Set-Acl -Path $DeploymentPath -AclObject $acl
        Write-Log "Set permissions for IIS_IUSRS"
    }
    catch {
        Write-Log "Error setting permissions: $($_.Exception.Message)" "WARNING"
    }
    
    Write-Log "Files deployed successfully"
}

# Function to configure IIS
function Set-IISConfiguration {
    Write-Log "Configuring IIS..."
    
    try {
        Import-Module WebAdministration -ErrorAction SilentlyContinue
        
        # Create application pool if it doesn't exist
        if (!(Get-IISAppPool -Name $ApplicationPool -ErrorAction SilentlyContinue)) {
            New-WebAppPool -Name $ApplicationPool
            Write-Log "Created application pool: $ApplicationPool"
        }
        
        # Configure application pool
        Set-ItemProperty -Path "IIS:\AppPools\$ApplicationPool" -Name "managedRuntimeVersion" -Value ""
        Set-ItemProperty -Path "IIS:\AppPools\$ApplicationPool" -Name "startMode" -Value "AlwaysRunning"
        Set-ItemProperty -Path "IIS:\AppPools\$ApplicationPool" -Name "idleTimeout" -Value "00:00:00"
        Set-ItemProperty -Path "IIS:\AppPools\$ApplicationPool" -Name "recycling.periodicRestart.time" -Value "1.05:00:00"
        
        # Create website if it doesn't exist
        if (!(Get-Website -Name $SiteName -ErrorAction SilentlyContinue)) {
            New-Website -Name $SiteName -Port 80 -PhysicalPath $DeploymentPath -ApplicationPool $ApplicationPool
            Write-Log "Created website: $SiteName"
        }
        else {
            Set-ItemProperty -Path "IIS:\Sites\$SiteName" -Name "physicalPath" -Value $DeploymentPath
            Set-ItemProperty -Path "IIS:\Sites\$SiteName" -Name "applicationPool" -Value $ApplicationPool
        }
        
        Write-Log "IIS configuration completed"
    }
    catch {
        Write-Log "Error configuring IIS: $($_.Exception.Message)" "ERROR"
        throw
    }
}

# Function to start IIS services
function Start-IISServices {
    Write-Log "Starting IIS services..."
    
    try {
        # Start application pool
        Start-WebAppPool -Name $ApplicationPool
        Write-Log "Started application pool: $ApplicationPool"
        
        # Start website
        Start-Website -Name $SiteName
        Write-Log "Started website: $SiteName"
        
        # Wait for services to start
        Start-Sleep -Seconds 10
    }
    catch {
        Write-Log "Error starting IIS services: $($_.Exception.Message)" "ERROR"
        throw
    }
}

# Function to verify deployment
function Test-Deployment {
    Write-Log "Verifying deployment..."
    
    try {
        # Check if website is running
        $site = Get-Website -Name $SiteName
        if ($site.State -ne "Started") {
            throw "Website is not running"
        }
        
        # Test HTTP endpoint
        $healthCheckUrl = "http://localhost/health"
        try {
            $response = Invoke-WebRequest -Uri $healthCheckUrl -TimeoutSec 30 -ErrorAction SilentlyContinue
            if ($response.StatusCode -eq 200) {
                Write-Log "Health check passed"
            }
            else {
                Write-Log "Health check returned status: $($response.StatusCode)" "WARNING"
            }
        }
        catch {
            Write-Log "Health check failed: $($_.Exception.Message)" "WARNING"
        }
        
        Write-Log "Deployment verification completed"
    }
    catch {
        Write-Log "Deployment verification failed: $($_.Exception.Message)" "ERROR"
        throw
    }
}

# Main deployment process
try {
    Write-Log "Starting deployment of $SiteName to $TargetServer"
    Write-Log "Configuration: $Configuration"
    Write-Log "Deployment Path: $DeploymentPath"
    Write-Log "Application Pool: $ApplicationPool"
    
    # Run deployment steps
    Test-Prerequisites
    Invoke-Tests
    Publish-Application
    New-Backup
    Stop-IISServices
    Deploy-Files
    Set-IISConfiguration
    Start-IISServices
    Test-Deployment
    
    Write-Log "Deployment completed successfully!" "SUCCESS"
    Write-Log "Application URL: http://$TargetServer"
    Write-Log "Deployment log: $LogFile"
}
catch {
    Write-Log "Deployment failed: $($_.Exception.Message)" "ERROR"
    Write-Log "Deployment log: $LogFile"
    exit 1
}
finally {
    # Cleanup
    if (Test-Path $PublishPath) {
        Remove-Item -Recurse -Force $PublishPath -ErrorAction SilentlyContinue
    }
}