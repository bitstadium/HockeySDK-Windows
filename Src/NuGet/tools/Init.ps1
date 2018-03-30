param($installPath, $toolsPath, $package, $project)

if ($project) {
    $project.DTE.ItemOperations.Navigate('https://github.com/bitstadium/HockeySDK-Windows/blob/preseason/README.md')
}
