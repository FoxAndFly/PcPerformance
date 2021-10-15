cmd.exe /c 'msbuild' /target:Clean /target:Build /property:Configuration=Release /verbosity:minimal 'PcPerformance.sln'
Remove-Item -Path 'PcPerFormance/bin/Release/PcPerformance.exe.config'
Remove-Item -Path 'PcPerFormance/bin/Release/PcPerformance.pdb'