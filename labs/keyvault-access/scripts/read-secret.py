import warnings
warnings.filterwarnings("ignore")

from azure.keyvault.secrets import SecretClient
from azure.identity import ManagedIdentityCredential

keyVaultName = input("Enter Key Vault name:")
KVUri = f"https://{keyVaultName}.vault.azure.net"
secretName = "secret01"

credential = ManagedIdentityCredential()
client = SecretClient(vault_url=KVUri, credential=credential)
retrieved_secret = client.get_secret(secretName)

print(f"Secret: '{secretName}' value    : '{retrieved_secret.value}'")