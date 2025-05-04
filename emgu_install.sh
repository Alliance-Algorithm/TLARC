sudo apt install build-essential libgtk-3-dev libgstreamer1.0-dev \
  libavcodec-dev libswscale-dev libavformat-dev libdc1394-dev libv4l-dev \
  cmake-curses-gui ocl-icd-dev freeglut3-dev libgeotiff-dev libusb-1.0-0-dev \
  libvtk9-dev libfreetype-dev libharfbuzz-dev qtbase5-dev libeigen3-dev \
  libgstreamer1.0-dev libgstreamer-plugins-base1.0-dev libgflags-dev \
  libgoogle-glog-dev liblapacke-dev libva-dev  libatlas-base-dev  
  
sudo apt -y install mono-complete


if [ "$1" == "cuda" ]; then
    sudo apt-get install nvidia-cuda-dev nvidia-cuda-toolkit nvidia-cudnn libcudnn-frontend-dev nvidia-cuda-toolkit-gcc 
fi