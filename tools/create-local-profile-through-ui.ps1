param(
  [Parameter(Mandatory = $true)]
  [int]$ProcessId,
  [Parameter(Mandatory = $true)]
  [string]$DisplayName,
  [Parameter(Mandatory = $true)]
  [string]$Passphrase
)

Add-Type -AssemblyName UIAutomationClient

$process = Get-Process -Id $ProcessId -ErrorAction Stop
$window = [System.Windows.Automation.AutomationElement]::FromHandle($process.MainWindowHandle)
$edits = $window.FindAll(
  [System.Windows.Automation.TreeScope]::Descendants,
  (New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
    [System.Windows.Automation.ControlType]::Edit
  ))
)

if ($edits.Count -lt 3) {
  throw "Expected display-name, passphrase, and confirmation edit controls."
}

$values = @($DisplayName, $Passphrase, $Passphrase)
for ($index = 0; $index -lt 3; $index++) {
  $pattern = $edits[$index].GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
  ([System.Windows.Automation.ValuePattern]$pattern).SetValue($values[$index])
}

$button = $window.FindFirst(
  [System.Windows.Automation.TreeScope]::Descendants,
  (New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::NameProperty,
    "Create local profile"
  ))
)

if ($null -eq $button) {
  throw "The local-profile action was not found."
}

$invoke = $button.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
([System.Windows.Automation.InvokePattern]$invoke).Invoke()
Start-Sleep -Milliseconds 1000
