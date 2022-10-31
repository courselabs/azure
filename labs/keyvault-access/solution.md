# Lab Solution

Browse to your Key Vault in the Portal and open _Secrets_. You'll see a warning saying your client IP does not have access - add it in the _Networking_ tab (you can find it at https://ifconfig.me).

Open `secret01` on the _Secrets_ page and select _Delete_ - you'll see a message about soft-delete being enabled.

Click on _Generate/Import_ and create a new secret:

- Name: `secret01`
- Secret value: `updated`

> You'll see an unhelpful error about a conflict

Try from the CLI instead:

```
az keyvault secret set --name secret01 --value azure-labs --vault-name <kv-name>
```

Now the error is more sensible:

_Secret secret01 is currently in a deleted but recoverable state, and its name cannot be reused; in this state, the secret can only be recovered or purged._

Back in the Portal click on _Manage deleted secrets_ from the _Secrets_ tab. Here you can _Purge_ the secret which deletes it permanently. 

> But you don't have permission!

No-one gets purge permissions by default, you need to add it to your account in _Access Policies_. Then you can purge the secret and recreate it.

As soon as the new secret is created, you can read it in the VM.