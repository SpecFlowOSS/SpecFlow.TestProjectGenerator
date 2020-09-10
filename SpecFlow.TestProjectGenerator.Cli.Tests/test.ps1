Function Write-Header 
{
	Param([String]$headerText)
	
	Write-Host $headerText -ForegroundColor Blue -BackgroundColor Gray -NoNewLine
	Write-Host
}

$scriptDir = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
$testFolder = "TestTemp"
$testDir="$scriptDir\$testFolder"
$testSlnFolder = "TestSln"

Write-Header "Cleanup test directory"
Write-Host $testDir
rd $testDir -r -force -ErrorAction Ignore
md $testDir

Push-Location -Path $testDir

Write-Header "Create tool manifest"
dotnet new tool-manifest

Write-Header "Install TPG tool"
dotnet tool install --no-cache --add-source ..\..\..\SpecFlow.TestProjectGenerator\SpecFlow.TestProjectGenerator.Cli\nupkg\ SpecFlow.TestProjectGenerator.Cli

Write-Header "Run TPG"
specflow-tpg.exe --sln-name Test

Write-Header "Run tests in generated project"
dotnet test Test

Pop-Location