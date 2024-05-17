git clone --mirror %1
java -jar bfg.jar --strip-blobs-bigger-than 10M %2
cd %2
git reflog expire --expire=now --all && git gc --prune=now --aggressive
git push
exit