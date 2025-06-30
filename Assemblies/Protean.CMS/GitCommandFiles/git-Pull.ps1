git init
git remote add -f origin https://eonicsource.visualstudio.com/ProteanCMS/_git/ProteanCMS
 
 
git config core.sparseCheckout true
 
echo "wwwroot/ewcommon/" >> .git/info/sparse-checkout
echo "GACInstaller/bin/Release/" >> .git/info/sparse-checkout
 
 
git pull origin ITB-GEN-DEV