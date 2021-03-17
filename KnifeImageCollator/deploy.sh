#!/bin/sh

pushd ImageCollatorFunction
dotnet build
dotnet lambda deploy-function --region eu-west-2 --profile sa-image-collator ImageCollatorFunction --function-role role-image-collator
popd
