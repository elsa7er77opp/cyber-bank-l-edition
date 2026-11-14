param(
  [Parameter(Mandatory = $true)]
  [int]$ProcessId
)

Add-Type -AssemblyName UIAutomationClient

$process = Get-Process -Id $ProcessId -ErrorAction Stop
$window = [System.Windows.Automation.AutomationElement]::FromHandle($process.MainWindowHandle)
$condition = New-Object System.Windows.Automation.PropertyCondition(
  [System.Windows.Automation.AutomationElement]::NameProperty,
  "Switch to dark"
)
$button = $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
if ($null -eq $button) {
  throw "The Light-to-Dark toggle was not found."
}

$pattern = $button.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
([System.Windows.Automation.InvokePattern]$pattern).Invoke()
