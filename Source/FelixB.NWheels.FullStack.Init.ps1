param($installPath, $toolsPath, $package)

# find out where to put the files, we're going to create a runtime directory
# at the same level as the solution.

$rootDir = (Get-Item $installPath).parent.parent.fullname
$runtimeTarget = "$rootDir\Runtime"

# create our runtime support directory if it doesn't exist yet

$runtimeSource = join-path $installPath 'lib/net45/runtime'

if (!(test-path $runtimeTarget)) {
	mkdir $runtimeTarget
}

# copy everything in there

Copy-Item "$runtimeSource/*" $runtimeTarget -Recurse -Force

# get the active solution
#$solution = Get-Interface $dte.Solution ([EnvDTE80.Solution2])

# create a runtime solution folder if it doesn't exist

#$runtimeFolder = $solution.Projects | where-object { $_.ProjectName -eq "runtime" } | select -first 1

#if(!$runtimeFolder) {
	#$runtimeFolder = $solution.AddSolutionFolder("runtime")
#}

# add all our support runtime scripts to our Support solution folder

#$folderItems = Get-Interface $runtimeFolder.ProjectItems ([EnvDTE.ProjectItems])

#ls $runtimeTarget | foreach-object { 
#	$folderItems.AddFromFile($_.FullName) > $null
#} > $null
