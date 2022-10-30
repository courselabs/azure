#!/bin/sh

apt update && apt install -y python3-pip

pip3 install pip --upgrade
pip3 install azure-keyvault-secrets
pip3 install azure.identity
pip3 install pyopenssl --upgrade
