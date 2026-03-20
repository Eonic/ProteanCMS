param (
    [string]$ClientId,
    [string]$TenantId,
    [string]$ClientSecret,
    [string]$RepoPath,
    [string]$GitUrl
)

Write-Host "⚙️ Git pull via OAuth: starting..."

# Safe directory config (local only)
$repoGitConfig = Join-Path $RepoPath ".git\config"
if (Test-Path $repoGitConfig) {
    git config --file "$repoGitConfig" --add safe.directory "$RepoPath"
} else {
    Write-Error "❌ .git/config not found in repo path: $RepoPath"
    exit 1
}
# Create askpass script
$askPassPath = "$env:TEMP\askpass_oauth2.bat"
Set-Content -Path $askPassPath -Value "@echo off`necho oauth2:$accessToken"

# Git environment setup
$env:GIT_ASKPASS = $askPassPath
$env:GIT_TERMINAL_PROMPT = 0

# Git pull
Set-Location $RepoPath
Write-Host "Running: git pull $GitUrl"
git config core.sparseCheckout true
echo "wwwroot/ewcommon/" >> .git/info/sparse-checkout
echo "GACInstaller/bin/Release/" >> .git/info/sparse-checkout

# Pull
git pull origin ITB-GEN-DEV

# Cleanup
Remove-Item $askPassPath -Force