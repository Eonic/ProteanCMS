Set-Location "D:\Test"


echo "wwwroot/ewcommon/" >> .git/info/sparse-checkout
echo "GACInstaller/bin/Release/" >> .git/info/sparse-checkout


git pull origin ITB-GEN-DEV 