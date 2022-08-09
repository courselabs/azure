#!/bin/bash

ACCOUNT_NAME='labsstoragefileses'
ACCOUNT_KEY='IFEEDndnIM5rsE6xBg2LX2scN95baMgnooO5bEn/PrhrLsTqGhhzmcj00I5A60tEspsRCUtdQbkG+AStIAGbtg=='
CONTAINER_NAME='labs'
SMB_FILE='/etc/smbcredentials/azure-files.cred'

mkdir -p /mnt/labs
mkdir -p /etc/smbcredentials
touch $SMB_FILE

echo "username=$ACCOUNT_NAME" > $SMB_FILE
echo "password=$ACCOUNT_KEY" >> $SMB_FILE
chmod 600 $SMB_FILE

echo "//$ACCOUNT_NAME.file.core.windows.net/$CONTAINER_NAME /mnt/$CONTAINER_NAME cifs nofail,credentials=$SMB_FILE,dir_mode=0777,file_mode=0777,serverino,nosharesock,actimeo=30" >> /etc/fstab
mount -t cifs "//$ACCOUNT_NAME.file.core.windows.net/labs" "/mnt/$CONTAINER_NAME" -o credentials=$SMB_FILE,dir_mode=0777,file_mode=0777,serverino,nosharesock,actimeo=30