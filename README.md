# Alliance 2024 RM Sentry Decision Maker

## [TLARC](https://github.com/Alliance-Algorithm/TLARC) ：the Top Layer Apex Contol System with ROS2：humble and dotnet C#

## Fatal: Do not use in HIGHT SPEED control flow
this is a top layer controller,you can use in frequency less then 300 (I guess), we have prove that ros is not fit in hight speed control flow 

## Tutorial
Is Internal Project, so i will teach it offline

## Environment Build
### [RCLNET](https://github.com/noelex/rclnet)
### Dev Container
if you want to deploy it,just add these into your Dockerfile
```[docker]
RUN apt update && apt install -y  \
  zsh wget curl vim 

RUN apt-get update && \
  apt-get install -y \
  dotnet-sdk-8.0  aspnetcore-runtime-8.0 zlib1g &&\
  rm -rf /var/lib/apt/lists/*

RUN wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh && \
  chmod +x ./dotnet-install.sh && ./dotnet-install.sh --channel 8.0 &&\
  rm -rf ./dotnet-install.sh

RUN echo "export DOTNET_ROOT=$HOME/.dotnet" >> ~/.zshrc &&\
  echo "export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools" >> ~/.zshrc

SHELL [ "/bin/zsh" ,"-c"]
RUN echo "dotnet add package Rcl.NET && \
  dotnet add package Rosidl.Runtime && \
  dotnet tool install -g ros2cs" > /root/dotnetpkgs.zsh

```
---
There is nothing diff between c++ and c#,just colcon build and runit