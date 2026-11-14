param(
    [Parameter(Mandatory = $true)]
    [int]$ProcessId,
    [Parameter(Mandatory = $true)]
    [string]$OutputPath
)

Add-Type -AssemblyName System.Drawing

Add-Type @"
using System;
using System.Runtime.InteropServices;

public static class NativeWindowCapture
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr windowHandle, out Rect rect);

    [DllImport("user32.dll")]
    public static extern bool PrintWindow(IntPtr windowHandle, IntPtr deviceContext, uint flags);
}
"@

$process = Get-Process -Id $ProcessId -ErrorAction Stop
$windowHandle = $process.MainWindowHandle
if ($windowHandle -eq [IntPtr]::Zero) {
    throw "The selected process has no visible main window."
}

$rect = New-Object NativeWindowCapture+Rect
if (-not [NativeWindowCapture]::GetWindowRect($windowHandle, [ref]$rect)) {
    throw "Unable to read the main window bounds."
}

$width = $rect.Right - $rect.Left
$height = $rect.Bottom - $rect.Top
if ($width -le 0 -or $height -le 0) {
    throw "The selected window has invalid bounds."
}

$bitmap = New-Object System.Drawing.Bitmap($width, $height)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
try {
    $deviceContext = $graphics.GetHdc()
    try {
        if (-not [NativeWindowCapture]::PrintWindow($windowHandle, $deviceContext, 2)) {
            throw "PrintWindow could not capture the selected window."
        }
    }
    finally {
        $graphics.ReleaseHdc($deviceContext)
    }

    $bitmap.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)
}
finally {
    $graphics.Dispose()
    $bitmap.Dispose()
}
