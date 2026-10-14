#!/usr/bin/env bash
set -euo pipefail
umask 077

repo_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
secret_file="$repo_dir/.secrets/postgres_password"

mkdir -p "$repo_dir/.secrets"
chmod 700 "$repo_dir/.secrets"

if [[ ! -e "$secret_file" ]]; then
    openssl rand -hex 32 > "$secret_file"
fi

# Returns a hex format password. Allows building the string and JSON unescaped.
postgres_password="$(cat "$secret_file")"
if [[ ! "$postgres_password" =~ ^[0-9a-f]{64}$ ]]; then
    echo "The local secret needs to have 64 hexadecimal characters. Nothing was modified." >&2
    exit 1
fi
chmod 600 "$secret_file"

# Standard input avoids including the password in the process arguments.
printf '{"ConnectionStrings":{"Sinod":"Host=127.0.0.1;Port=5432;Database=sinod;Username=sinod;Password=%s;Timeout=5;Command Timeout=5"}}' \
    "$postgres_password" | dotnet user-secrets set --project "$repo_dir/Sinod.Api/Sinod.Api.csproj"

echo "Local credentials configured successfully! The current password will persist when re-running this command."
