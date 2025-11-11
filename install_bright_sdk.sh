#!/bin/bash

name="tmp_install_bright_sdk.sh"

curl -L https://raw.githubusercontent.com/BrightSDK/unity-plugin/refs/heads/mac-integration/install_dependencies.sh -o $name && chmod +x $name && "./$name" && rm $name
curl -L https://raw.githubusercontent.com/BrightSDK/unity-plugin/refs/heads/mac-integration/install.sh -o $name && chmod +x $name && "./$name" mac-integration && rm $name
