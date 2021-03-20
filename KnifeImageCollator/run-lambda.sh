#!/bin/sh

 dotnet lambda invoke-function ImageCollatorFunction --region eu-west-2 --profile sa-image-collator --payload '{ "collation": "list", "accounts": "instantiator", "period": "2021-03-19:2021-03-20", "group": "test-group" }'