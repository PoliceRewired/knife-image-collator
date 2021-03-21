#!/bin/sh

dotnet lambda invoke-function ImageCollatorFunction --region eu-west-2 --profile sa-image-collator --payload '{ "collation": "list", "accounts": ["mettaskforce"], "filter": "keywords", "keywords_list" : [], "keywords_list_url": "https://raw.githubusercontent.com/PoliceRewired/image-collations/main/keywords-list-knife-tweets.txt", "period": "2020-01-01:2021-03-21", "group": "test-group-met-taskforce" }'
