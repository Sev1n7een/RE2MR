@echo off
chcp 65001 >nul
title 卸载 Interception 驱动 - RE2鼠标复活计划
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
echo  卸载 Interception 驱动
echo  若提示 Error deleting ...sys 或 ...driver key 属正常：
echo  注册表已清除，重启后文件会自动删除。
echo ============================================================
echo.
install-interception.exe /uninstall
echo.
echo 请现在【重启电脑】完成卸载。
echo 重启后建议再运行一次本脚本核对：若显示无法找到驱动即已卸载干净。
pause