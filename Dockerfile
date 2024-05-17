FROM ros:humble

SHELL ["/bin/bash", "-c"]

RUN sed -i 's/archive.ubuntu.com/mirrors.ustc.edu.cn/g' /etc/apt/sources.list

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

RUN wget https://gitee.com/mirrors/oh-my-zsh/raw/master/tools/install.sh -O zsh-install.sh && \
  chmod +x ./zsh-install.sh && ./zsh-install.sh && \
  sed -i 's/REPO=${REPO:-ohmyzsh\/ohmyzsh}/REPO=${REPO:-mirrors\/oh-my-zsh}/' ./zsh-install.sh && \
  sed -i 's/REMOTE=${REMOTE:-https:\/\/github.com\/${REPO}.git}/REMOTE=${REMOTE:-https:\/\/gitee.com\/${REPO}.git}/' ./zsh-install.sh && \
  sed -i 's/ZSH_THEME=\"[a-z0-9\-]*\"/ZSH_THEME="af-magic"/g' ~/.zshrc && \
  sed -i 's/plugins=(git)/plugins=(git zsh-syntax-highlighting zsh-autosuggestions)/g' ~/.zshrc  && \
  git clone https://github.com/zsh-users/zsh-syntax-highlighting.git ${ZSH_CUSTOM:-~/.oh-my-zsh/custom}/plugins/zsh-syntax-highlighting && git clone https://github.com/zsh-users/zsh-autosuggestions ${ZSH_CUSTOM:-~/.oh-my-zsh/custom}/plugins/zsh-autosuggestions && \
  rm ./zsh-install.sh 


RUN chsh root -s /bin/zsh

RUN echo "source /opt/ros/humble/setup.zsh" >> ~/.zshrc