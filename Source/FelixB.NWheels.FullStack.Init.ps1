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
