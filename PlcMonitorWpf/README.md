# PLC Monitor WPF

一个简洁的 WPF PLC 监控界面原型：左侧设备列表、中间摄像头画面、右侧报警与温度趋势。

## 打开和运行

1. 在 Windows 安装 Visual Studio 2022 Community，并在安装器中勾选 **.NET 桌面开发**。
2. 用 Visual Studio 打开上一级目录中的 `PlcMonitorWpf.sln`。
3. 按 `F5` 运行。

当前 PLC 数据每秒模拟刷新。中央视频区当前是未连接的占位界面；接入摄像头后，可在该区域显示实时画面。

## 后续接 PLC

依据 PLC 品牌接入：

- 西门子：S7 通讯
- 三菱、汇川等：Modbus TCP
- 支持 OPC UA 的 PLC：OPC UA

真实控制（启停、复位）应在加入权限、二次确认、急停联锁后才启用。

## 后续接摄像头

确定摄像头型号和视频协议后接入。网络摄像头通常使用 RTSP，USB 摄像头通常使用 Windows 摄像头接口。建议先确认每台摄像头的 IP、账号和视频流地址，再选择相应的 WPF 播放组件。
