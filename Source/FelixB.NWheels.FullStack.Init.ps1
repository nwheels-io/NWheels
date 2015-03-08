param($installPath, $toolsPath, $package)

# find out where to put the files, we're going to create a runtime directory
# at the same level as the solution.

$rootDir = (Get-Item $installPath).parent.parent.fullname
$runtimeTarget = "$rootDir\Runtime"

# cleanup Runtime folder installed by previous version, if any

if (test-path $runtimeTarget) {
	Remove-Item "$runtimeTarget/*" -recurse -force
}

# create Runtime folder if it doesn't exist yet

$runtimeSource = join-path $installPath 'lib/net45/runtime'

if (!(test-path $runtimeTarget)) {
	mkdir $runtimeTarget
}

# copy contents of the Runtime folder

Copy-Item "$runtimeSource/*" $runtimeTarget -Recurse -Force
