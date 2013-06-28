# uninstall.ps1
param($installPath, $toolsPath, $package, $project)

Function Clean ($localPath)
{
    $xml = New-Object xml

	$xml.Load($localPath)
	$configNode = $xml.SelectSingleNode("configuration")
	$allsections = $xml.SelectNodes("configuration/configSections/section")
	Write-Host $allSections.Count + " sections found"
	foreach($section in $allSections)
	{
		Write-Host $section.GetAttribute("name")
	}
	$sections = $xml.SelectNodes("configuration/configSections/section") | where { $_.name -eq "RaygunSettings" }
	if ($sections.Count -eq 0)
	{
		$nodes = $xml.SelectNodes("configuration/RaygunSettings")
		$i = 0
		while($i -lt $nodes.Count)
		{
		  $configNode.RemoveChild($nodes.Item($i))
		  $i++
		}
		$xml.Save($localPath)
	}
}

$configs = $project.ProjectItems | where { $_.Name -eq "Web.config" -or $_.Name -eq "App.config" }
Foreach ($config in $configs)
{
	Write-Host "Uninstalling"
    $localPath = $config.Properties | where { $_.Name -eq "LocalPath" }
    Clean $localPath.Value
}