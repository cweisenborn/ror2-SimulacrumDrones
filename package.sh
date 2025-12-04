#!/bin/bash

# Define paths
SOURCE_DIR="SimulacrumDronesPlugin" # The actual source code folder
THUNDERSTORE_DIR="Thunderstore"
BUILD_DIR="build_tmp"
PACKAGE_NAME="SimulacrumDronesPlugin"
ZIP_NAME="SimulacrumDronesPlugin.zip"

# Cleanup previous build
rm -rf "$BUILD_DIR"
rm -f "$ZIP_NAME"

# Create build directory structure
mkdir -p "$BUILD_DIR/$PACKAGE_NAME"

# 1. Copy Thunderstore folder contents to the package folder
cp -r "$THUNDERSTORE_DIR/"* "$BUILD_DIR/$PACKAGE_NAME/"

# 2. Find and copy the DLL
# Look for the DLL in the bin folder of the source project
echo "Searching for DLL in $SOURCE_DIR/bin..."
DLL_PATH=$(find "$SOURCE_DIR/bin" -name "SimulacrumDronesPlugin.dll" | head -n 1)

if [ -n "$DLL_PATH" ] && [ -f "$DLL_PATH" ]; then
    echo "Found DLL at: $DLL_PATH"
    # Ensure plugins directory exists
    mkdir -p "$BUILD_DIR/$PACKAGE_NAME/plugins"
    cp "$DLL_PATH" "$BUILD_DIR/$PACKAGE_NAME/plugins/"
else
    echo "Error: SimulacrumDronesPlugin.dll not found in $SOURCE_DIR/bin. Please build the project first."
    rm -rf "$BUILD_DIR"
    exit 1
fi

# 3. Zip it up
# Navigate to build dir to zip relative path
cd "$BUILD_DIR"
zip -r "../$ZIP_NAME" "$PACKAGE_NAME"
cd ..

# 4. Cleanup
rm -rf "$BUILD_DIR"

echo "Packaging complete: $ZIP_NAME"
