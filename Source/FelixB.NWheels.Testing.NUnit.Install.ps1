param($installPath, $toolsPath, $package, $project)

# set Copy Local to false on references to assemblies in this package

$asms = $package.AssemblyReferences | %{$_.Name}

foreach ($reference in $project.Object.References)
{
    if ($asms -contains $reference.Name + ".dll")
    {
        $reference.CopyLocal = $false;
    }
}
