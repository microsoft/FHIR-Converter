# FHIR-Converter

## Before workers can communicate with the FHIR-Converter

>Deploy API keys to the GKE clusters
```sh
./bin/create_secrets.sh -v 'Staging Secrets' -s staging-kokko -e staging
./bin/create_secrets.sh -v 'Staging Secrets' -s staging-kokko -e demo
./bin/create_secrets.sh -v 'Production Secrets' -s production-kokko -e production-de
```

## Endpoints

```sh
# UI (Kaiku VPN)    = https://fhir-converter.kaikuhealth.com
# API for workers   = http://fhir-converter.kaikuhealth.private/api/
```

