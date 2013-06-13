# uninstall.ps1
param($installPath, $toolsPath, $package, $project)

Function Clean ($localPath)
{
    $xml = New-Object xml

	# load config as XML
	$xml.Load($localPath)
	$parentNode = $xml.SelectSingleNode("configuration")
	$nodes = $xml.SelectNodes("configuration" + "/" + "RaygunSettings")
	$i = 0
	while($i -lt $nodes.Count)
	{
	  $parentNode.RemoveChild($nodes.Item($i))
	  $i++
	}
	$xml.Save($localPath)
}

$configs = $project.ProjectItems | where {$_.Name -eq "Web.config" -or $_.Name -eq "App.config" }
Foreach ($config in $configs)
{
    $localPath = $config.Properties | where {$_.Name -eq "LocalPath"}
	Write-Host "LoaclPath: " + $localPath.Value
    Clean $localPath.Value
}