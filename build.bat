@echo off
chcp 65001 >nul
echo ========================================
echo   Task Manager - SQLite Version
echo   Compiling...
echo ========================================

set CSC=C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe
set SQLITE_DLL=packages\Stub.System.Data.SQLite.Core.NetFramework.1.0.119.0\lib\net40\System.Data.SQLite.dll

if not exist "%CSC%" (
    echo ERROR: csc.exe not found!
    pause
    exit /b 1
)

if not exist "%SQLITE_DLL%" (
    echo ERROR: System.Data.SQLite.dll not found!
    echo Please run: nuget.exe install System.Data.SQLite.Core -OutputDirectory packages
    pause
    exit /b 1
)

echo Compiling TaskManager...
"%CSC%" /target:winexe /out:TaskManager.exe /r:"%SQLITE_DLL%" /r:System.Drawing.dll ^
    Program.cs ^
    LoginForm.cs ^
    LoginForm.Designer.cs ^
    MainForm.cs ^
    MainForm.Designer.cs

if %errorlevel% neq 0 (
    echo.
    echo Compile FAILED!
    pause
    exit /b 1
)

echo.
echo Compile successful!
echo Copying dependencies...

:: Copy main DLL
copy /Y "%SQLITE_DLL%" . >nul

:: Create x86 and x64 folders for native SQLite library
if not exist "x86" mkdir x86
if not exist "x64" mkdir x64

:: Copy native interop DLLs
copy /Y "packages\Stub.System.Data.SQLite.Core.NetFramework.1.0.119.0\build\net40\x86\SQLite.Interop.dll" x86\ >nul
copy /Y "packages\Stub.System.Data.SQLite.Core.NetFramework.1.0.119.0\build\net40\x64\SQLite.Interop.dll" x64\ >nul

echo.
echo ========================================
echo   Build completed!
echo   Output: TaskManager.exe
echo   Database: tasks.db (auto-created)
echo ========================================
echo.
echo Press any key to run the program...
pause >nul
start TaskManager.exe
