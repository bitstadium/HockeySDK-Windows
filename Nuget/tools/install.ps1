param($installPath, $toolsPath, $package, $project)

$project.DTE.ItemOperations.Navigate('https://github.com/bitstadium/HockeySDK-Windows/blob/develop/README.md')

//TODO braucht es das oder ist content der default ?! funzt das mit Extension (orig: Name)
$item = $project.ProjectItems | where-object {$_.Extension -eq "png"} 
$item.Properties.Item("BuildAction").Value = [int]2
