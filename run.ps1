# Build Docker image
Write-Host "Building Docker image..." -ForegroundColor Green
docker build --no-cache -t hlkxgooglekmstool:v1 .

Write-Host "Starting HLKX archive signing with .NET Google KMS Signer..." -ForegroundColor Green

# Run Docker container with signing
docker run -v "${PWD}/data:/data" hlkxgooglekmstool:v1 `
    sign -gcf /data/credentials.json `
    -cf /data/certificate.cer `
    -gks projects/project_name/locations/global/keyRings/ring_name/cryptoKeys/key_name/cryptoKeyVersions/1 `
    /data/file_to_sign.hlkx    

Write-Host "Done!" -ForegroundColor Green
