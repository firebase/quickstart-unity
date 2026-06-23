#!/bin/bash

# Exit immediately if a command fails
set -e

# Check if the folder name argument was provided
if [ -z "$1" ]; then
  echo "Error: No folder name provided."
  echo "Usage: $0 <folder_name>"
  echo "Example: $0 remote_config"
  exit 1
fi

MODULE="$1"

# Convert snake_case to PascalCase (e.g., remote_config -> RemoteConfig, database -> Database)
MODULE_CAP=$(echo "$MODULE" | awk -F_ '{for(i=1;i<=NF;i++) printf "%s", toupper(substr($i,1,1)) substr($i,2); print ""}')

# Define base source and destination paths
SRC_BASE="$HOME/Documents/GitHub/firebase-unity-sdk/${MODULE}/testapp"
DEST_BASE="${MODULE}/testapp"

echo "Copying files for module: ${MODULE} (Unity folder: ${MODULE_CAP})..."

# Ensure destination directories exist before copying to prevent errors
mkdir -p "${DEST_BASE}/Assets/Firebase/Sample/${MODULE_CAP}/"
mkdir -p "${DEST_BASE}/ProjectSettings/"

# Perform the copies
cp "${SRC_BASE}/Assets/Firebase/Sample/${MODULE_CAP}/MainScene.unity"* "${DEST_BASE}/Assets/Firebase/Sample/${MODULE_CAP}/"
cp "${SRC_BASE}/Assets/Firebase/Sample/${MODULE_CAP}/UIHandler.cs"* "${DEST_BASE}/Assets/Firebase/Sample/${MODULE_CAP}/"
cp "${SRC_BASE}/ProjectSettings/"* "${DEST_BASE}/ProjectSettings/"

echo "Done!"
