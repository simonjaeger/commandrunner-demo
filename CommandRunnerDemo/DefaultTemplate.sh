#!/bin/bash

# Variables. Keep short to save space.
PTH="/tmp/[DOWNLOAD_FILE_NAME]";
URI="[URI]";

# TODO: Install delivery optimization agent if not available.
# TODO: Find port of the delivery optimization agent.
# Default port is 50000, but will be incremented by 1 if its already taken until it finds an available one.
PRT=50000;

# Install required tools.
apt-get -qq install wget >> /dev/null;

# Remove file if already present. Otherwise the delivery optimization agent fails.
rm -f $PTH;

# Download file.
wget $URI -O $PTH

# Execute file.
chmod +x $PTH;
$PTH;