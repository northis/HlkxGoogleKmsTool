### HLKX + Google KMS = ♥

This tool is for [signing HLKX](https://learn.microsoft.com/en-us/windows-hardware/test/hlk/user/digitally-sign-an-hlkx-package) files using [Google KMS, asymmetric sign](https://cloud.google.com/kms/docs/samples/kms-sign-asymmetric#kms_sign_asymmetric-csharp).

Requirements:

- .NET 8+ (SDK and runtime)
- Docker (optional)

Build:

```shell
mkdir build
dotnet publish src/OpenVsixSignTool/OpenVsixSignTool.csproj -c Release -o ./build
```
To sign a HLKX file, you need a public leaf certificate, a [JSON file with credentials](https://cloud.google.com/docs/authentication/application-default-credentials#personal) and a key ring string given from Google KMS service. This tools supports work with [service accounts](https://cloud.google.com/iam/docs/service-accounts) only.

Usage:

```shell
cd build
OpenVsixSignTool sign -cf cerfiticate.cer -gcf credentials.json -gks projects/<project_id>/locations/global/keyRings/<key_ring>/cryptoKeys/<crypto_keys>/cryptoKeyVersions/1 <file_to_sign>.hlkx
```

Use `run.ps1` or `run.sh` to run it via Docker. See `Dockerfile` for more information.

### This fork is based on:

1. Original Tool [VsixSignTool](https://www.nuget.org/packages/Microsoft.VSSDK.Vsixsigntool)
2. An open-source implemention [OpenOpcSignTool](https://github.com/vcsjones/OpenOpcSignTool)
3. HLKX and Azure [fork](https://github.com/monrapps/OpenOpcSignTool)
