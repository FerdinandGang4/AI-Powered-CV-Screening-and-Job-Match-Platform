#!/bin/bash
set -euxo pipefail

sudo dnf update -y
sudo dnf install -y docker git
sudo systemctl enable docker
sudo systemctl start docker
sudo usermod -aG docker ec2-user

sudo mkdir -p /usr/local/lib/docker/cli-plugins
sudo curl -SL "https://github.com/docker/compose/releases/download/v2.36.0/docker-compose-linux-x86_64" -o /usr/local/lib/docker/cli-plugins/docker-compose
sudo chmod +x /usr/local/lib/docker/cli-plugins/docker-compose

cd /home/ec2-user
if [ ! -d AI-Powered-CV-Screening-and-Job-Match-Platform ]; then
  git clone https://github.com/FerdinandGang4/AI-Powered-CV-Screening-and-Job-Match-Platform.git
fi

cd /home/ec2-user/AI-Powered-CV-Screening-and-Job-Match-Platform
if [ ! -f .env ]; then
  cp .env.example .env
fi
chown -R ec2-user:ec2-user /home/ec2-user/AI-Powered-CV-Screening-and-Job-Match-Platform
docker compose up -d --build
