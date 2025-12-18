# ProteanCMS Baseline Metrics Capture Script
# Uses Windows Performance Counters - No external tools required

param(
    [int]$DurationSeconds = 300,  # Run for 5 minutes
    [string]$ProcessName = "w3wp",  # IIS worker process
    [string]$OutputPath = ".\.artiforge\baseline-metrics.csv"
)

Write-Host "=== ProteanCMS Baseline Metrics Capture ===" -ForegroundColor Cyan
Write-Host "Duration: $DurationSeconds seconds" -ForegroundColor Yellow
Write-Host "Process: $ProcessName" -ForegroundColor Yellow
Write-Host ""

# Define performance counters
$counters = @(
    "\Process($ProcessName)\Working Set",
    "\Process($ProcessName)\Private Bytes",
    "\Process($ProcessName)\Handle Count",
    "\Process($ProcessName)\Thread Count",
    "\.NET CLR Memory($ProcessName)\# Bytes in all Heaps",
    "\.NET CLR Memory($ProcessName)\# Gen 0 Collections",
    "\.NET CLR Memory($ProcessName)\# Gen 1 Collections",
    "\.NET CLR Memory($ProcessName)\# Gen 2 Collections",
    "\.NET CLR Memory($ProcessName)\Large Object Heap size",
    "\.NET Data Provider for SqlServer($ProcessName)\NumberOfActiveConnections",
    "\.NET Data Provider for SqlServer($ProcessName)\NumberOfPooledConnections"
)

# Initialize results
$results = @()

Write-Host "Starting metrics collection..." -ForegroundColor Green
Write-Host "Please execute your test workflows now." -ForegroundColor Yellow
Write-Host ""

$startTime = Get-Date
$endTime = $startTime.AddSeconds($DurationSeconds)
$sampleInterval = 5  # seconds

while ((Get-Date) -lt $endTime) {
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $elapsed = [math]::Round(((Get-Date) - $startTime).TotalSeconds, 0)
    
    Write-Host "[$elapsed/$DurationSeconds sec] Capturing metrics..." -NoNewline
    
    $sample = [PSCustomObject]@{
        Timestamp = $timestamp
        ElapsedSeconds = $elapsed
    }
    
    foreach ($counter in $counters) {
        try {
            $value = (Get-Counter -Counter $counter -ErrorAction SilentlyContinue).CounterSamples[0].CookedValue
            $counterName = ($counter -split '\\')[-1]
            $sample | Add-Member -NotePropertyName $counterName -NotePropertyValue $value
        }
        catch {
            $counterName = ($counter -split '\\')[-1]
            $sample | Add-Member -NotePropertyName $counterName -NotePropertyValue 0
        }
    }
    
    $results += $sample
    
    Write-Host " ?" -ForegroundColor Green
    Start-Sleep -Seconds $sampleInterval
}

Write-Host ""
Write-Host "Exporting results to: $OutputPath" -ForegroundColor Cyan

# Export to CSV
$results | Export-Csv -Path $OutputPath -NoTypeInformation

# Calculate summary statistics
Write-Host ""
Write-Host "=== Summary Statistics ===" -ForegroundColor Cyan
Write-Host ""

$avgMemory = ($results | Measure-Object -Property "Private Bytes" -Average).Average / 1MB
$maxMemory = ($results | Measure-Object -Property "Private Bytes" -Maximum).Maximum / 1MB
$avgHandles = ($results | Measure-Object -Property "Handle Count" -Average).Average
$maxHandles = ($results | Measure-Object -Property "Handle Count" -Maximum).Maximum

Write-Host "Memory Usage:" -ForegroundColor Yellow
Write-Host "  Average: $([math]::Round($avgMemory, 2)) MB"
Write-Host "  Maximum: $([math]::Round($maxMemory, 2)) MB"
Write-Host ""
Write-Host "Handle Count:" -ForegroundColor Yellow
Write-Host "  Average: $([math]::Round($avgHandles, 0))"
Write-Host "  Maximum: $maxHandles"
Write-Host ""

if ($results[0]."NumberOfActiveConnections" -ne $null) {
    $avgConnections = ($results | Measure-Object -Property "NumberOfActiveConnections" -Average).Average
    $maxConnections = ($results | Measure-Object -Property "NumberOfActiveConnections" -Maximum).Maximum
    
    Write-Host "SQL Connections:" -ForegroundColor Yellow
    Write-Host "  Average: $([math]::Round($avgConnections, 1))"
    Write-Host "  Maximum: $maxConnections"
    Write-Host ""
}

Write-Host "? Baseline capture complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Review the CSV file: $OutputPath"
Write-Host "2. After fixes, run this script again to compare results"
Write-Host "3. Look for reductions in memory, handles, and connections"
