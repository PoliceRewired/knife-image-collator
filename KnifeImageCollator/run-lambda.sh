#!/bin/sh

dotnet lambda invoke-function ImageCollatorFunction --region eu-west-2 --profile sa-image-collator --payload '{ "collation": "list", "accounts": ["instantiator"], "period": "lastweek", "group": "test-group-mps-taskforce" }'