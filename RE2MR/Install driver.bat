@echo off
chcp 65001 >nul
title 安装 Interception 驱动 - RE2鼠标复活计划
setlocal
cd /d "%~dp0"

if not exist "install-interception.exe" goto :err_missing
net session >nul 2>&1
if errorlevel 1 goto :elevate
goto :run

:err_missing
echo [错误] 找不到 install-interception.exe
echo        请保持本脚本与所有程序文件在同一个文件夹里运行。
pause
exit /b 1

:elevate
echo 正在请求管理员权限，请在弹窗中点“是”...
powershell -NoProfile -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
exit /b

:run
echo ============================================================
echo  安装 Interception 官方驱动 v1.0.1
echo  来源 https://github.com/oblitum/Interception
echo ============================================================
echo.
reg query "HKLM\SYSTEM\CurrentControlSet\Control\Class\{4D36E96F-E325-11CE-BFC1-08002BE10318}" /v UpperFilters | findstr /i "mouse" >nul
if errorlevel 1 goto :install
echo [已安装] 检测到驱动已存在，无需重复安装。
echo 直接重启电脑后运行 2.启动RE2鼠标复活计划 即可。
pause
exit /b 0

:install
install-interception.exe /install
echo.
reg query "HKLM\SYSTEM\CurrentControlSet\Control\Class\{4D36E96F-E325-11CE-BFC1-08002BE10318}" /v UpperFilters | findstr /i "mouse" >nul
if errorlevel 1 goto :fail
echo [OK] 驱动安装完成，过滤项已写入。
echo 请现在【重启电脑】，然后运行 2.启动RE2鼠标复活计划。
pause
exit /b 0

:fail
echo [失败] 安装未生效。请确认：
echo   1. 本脚本以管理员身份运行（弹窗点了“是”）
echo   2. install-interception.exe 与脚本在同一文件夹
echo 然后将本窗口内容截图发回排查。
pause
exit /b 1