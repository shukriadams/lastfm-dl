#!/usr/bin/env bash
set -e

sudo apt-get update

# dotnetcore
sudo apt install dotnet-sdk-6.0 -y

# altecover report generator
dotnet tool install --global dotnet-reportgenerator-globaltool --version 4.1.5

# docker
sudo mkdir -p /usr/libexec/docker/cli-plugins
sudo apt install docker.io -y
sudo usermod -aG docker vagrant
sudo wget https://github.com/docker/compose/releases/download/v2.29.1/docker-compose-linux-x86_64 -O /usr/libexec/docker/cli-plugins/docker-compose
sudo chmod +x /usr/libexec/docker/cli-plugins/docker-compose
echo "export PATH=/usr/libexec/docker/cli-plugins:$PATH" >> /home/vagrant/.bashrc

# porter
sudo wget https://github.com/shukriadams/porter/releases/download/0.0.2/porter_linux-x64 -O /usr/bin/porter
sudo chmod +x /usr/bin/porter

# force startup folder to vagrant project
echo "cd /vagrant/src" >> /home/vagrant/.bashrc

# set hostname, makes console easier to identify
sudo echo "lfmdata" > /etc/hostname
sudo echo "127.0.0.1 lfmdata" >> /etc/hosts