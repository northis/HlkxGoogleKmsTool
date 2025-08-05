#!/bin/bash

# Build Docker image
echo "Building Docker image..."
docker build -t hlkxgooglekmstool:v1 .

echo "Starting HLKX archive signing with .NET Google KMS Signer..."

# Run Docker container with signing
docker run -v $(pwd)/data:/data hlkxgooglekmstool:v1 \
  sign -gcf /data/credentials.json \
  -cf /data/certificate.cer \
  -gks projects/project_name/locations/global/keyRings/ring_name/cryptoKeys/key_name/cryptoKeyVersions/1 \
  /data/file_to_sign.hlkx
  

echo "Done!"
