#!/bin/sh

 dotnet lambda invoke-function ImageCollatorFunction --region eu-west-2 --profile sa-image-collator --payload 'some stuff'