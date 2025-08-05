using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Google.Cloud.Kms.V1;
using Google.Apis.Auth.OAuth2;

namespace OpenVsixSignTool
{
    internal class GoogleKmsRsa : RSA
    {
        private readonly KeyManagementServiceClient _kmsClient;
        private readonly string _keyName;
        private RSAParameters _publicKeyParameters;

        public GoogleKmsRsa(string credentialsJsonFilePath, string kmsKeyNameString, X509Certificate2 publicCertificate)
        {
            CredentialsJsonFilePath = credentialsJsonFilePath;
            KmsKeyNameString = kmsKeyNameString;
            PublicCertificate = publicCertificate;

            // Initialize KMS client with credentials
            var credential = GoogleCredential.FromFile(credentialsJsonFilePath)
                .CreateScoped(KeyManagementServiceClient.DefaultScopes);
            var clientBuilder = new KeyManagementServiceClientBuilder
            {
                Credential = credential
            };
            _kmsClient = clientBuilder.Build();
            _keyName = kmsKeyNameString;

            // Extract RSA parameters from the public certificate
            ExtractPublicKeyParameters();
        }

        public string CredentialsJsonFilePath { get; set; }
        public string KmsKeyNameString { get; set; }
        public X509Certificate2 PublicCertificate { get; }

        private void ExtractPublicKeyParameters()
        {
            using (var rsa = PublicCertificate.GetRSAPublicKey())
            {
                _publicKeyParameters = rsa.ExportParameters(false);
            }
        }

        public override RSAParameters ExportParameters(bool includePrivateParameters)
        {
            if (includePrivateParameters)
            {
                throw new CryptographicException("Cannot export private key parameters from Google KMS.");
            }
            return _publicKeyParameters;
        }

        public override void ImportParameters(RSAParameters parameters)
        {
            throw new NotSupportedException("Cannot import parameters into Google KMS RSA instance.");
        }

        public override byte[] SignHash(byte[] hash, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding)
        {
            if (padding != RSASignaturePadding.Pkcs1)
            {
                throw new NotSupportedException("Only PKCS#1 padding is supported.");
            }

            // Create the digest based on hash algorithm
            var digest = new Digest();
            switch (hashAlgorithm.Name)
            {
                case "SHA256":
                    digest.Sha256 = Google.Protobuf.ByteString.CopyFrom(hash);
                    break;
                case "SHA384":
                    digest.Sha384 = Google.Protobuf.ByteString.CopyFrom(hash);
                    break;
                case "SHA512":
                    digest.Sha512 = Google.Protobuf.ByteString.CopyFrom(hash);
                    break;
                default:
                    throw new NotSupportedException($"Hash algorithm {hashAlgorithm.Name} is not supported.");
            }

            // Create the asymmetric sign request
            var request = new AsymmetricSignRequest
            {
                Name = _keyName,
                Digest = digest
            };

            // Sign using Google KMS
            var response = _kmsClient.AsymmetricSign(request);
            return response.Signature.ToByteArray();
        }

        public override bool VerifyHash(byte[] hash, byte[] signature, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding)
        {
            // Use the public key from the certificate for verification
            using (var rsa = PublicCertificate.GetRSAPublicKey())
            {
                return rsa.VerifyHash(hash, signature, hashAlgorithm, padding);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // KeyManagementServiceClient implements IDisposable in newer versions
                // For older versions, we can safely ignore disposal as gRPC handles cleanup
                try
                {
                    (_kmsClient as IDisposable)?.Dispose();
                }
                catch
                {
                    // Ignore disposal errors
                }
            }
            base.Dispose(disposing);
        }

        // Override other required abstract methods with appropriate implementations or exceptions
        public override byte[] Decrypt(byte[] data, RSAEncryptionPadding padding)
        {
            throw new NotSupportedException("Decryption is not supported with Google KMS signing keys.");
        }

        public override byte[] Encrypt(byte[] data, RSAEncryptionPadding padding)
        {
            throw new NotSupportedException("Encryption is not supported with Google KMS signing keys.");
        }

        public override int KeySize => _publicKeyParameters.Modulus?.Length * 8 ?? 0;
    }
}
