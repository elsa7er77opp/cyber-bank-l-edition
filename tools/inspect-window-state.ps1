param(
  [Parameter(Mandatory = $true)]
  [int]$ProcessId
)

Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class WindowInspector
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    [DllImport("user32.dll")] public static extern bool IsIconic(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr hWnd);
}
'@

$process = Get-Process -Id $ProcessId -ErrorAction Stop
$rect = New-Object WindowInspector+RECT
[void][WindowInspector]::GetWindowRect($process.MainWindowHandle, [ref]$rect)
[pscustomobject]@{
  Handle = $process.MainWindowHandle
  Visible = [WindowInspector]::IsWindowVisible($process.MainWindowHandle)
  Iconic = [WindowInspector]::IsIconic($process.MainWindowHandle)
  Left = $rect.Left
  Top = $rect.Top
  Width = $rect.Right - $rect.Left
  Height = $rect.Bottom - $rect.Top
}
