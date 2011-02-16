@echo off
echo Remote deployment script for WSPBuilder
echo =======================================

echo Usage: RemoteDeploy $(TargetName).wsp SERVERNAME

echo Deploying %1 to %2
echo Copying Symbols
xcopy bin\debug\*.pdb c:\Build\Symbols\ /R /Y

echo Copying %1 to %2

xcopy /Y %1 \\%2\C$\In\Solutions

echo Running WSPBuilder with PSExec

c:\pstools\psexec -s -e \\%2 -w "C:\In\Solutions" "C:\Program Files (x86)\WSPTools\WSPBuilderExtensions\WSPBuilder" -BuildWSP false -WSPName %1 -Deploy true

echo %1 Deployed
