@echo off

xcopy "C:\FolderFlect\TestFiles\*.*" "C:\FolderFlect\ToReplicate\" /E

echo Press any key to move test.txt and test1.txt to the test folder...
pause >nul

move "C:\FolderFlect\ToReplicate\test.txt" "C:\FolderFlect\ToReplicate\test\"
move "C:\FolderFlect\ToReplicate\test1.txt" "C:\FolderFlect\ToReplicate\test\"

echo Press any key to rename the files...
pause >nul

rename "C:\FolderFlect\ToReplicate\test\test1.txt" t1.txt
rename "C:\FolderFlect\ToReplicate\test\test.txt" t2.txt

echo Press any key to delete the test folder...
pause >nul

rd /s /q "C:\FolderFlect\ToReplicate\test\"
