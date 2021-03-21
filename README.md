# Knife Image Collator

A tool to scan twitter accounts for images of knives and collate them for further classification.

This project is under development. Please check back for updates and more information.

## Automation

This tool is intended to run as an AWS lambda. Its tasks are:

* Retrieve tweets with images from specified github accounts (filtered for certain features)
* Summarise everything found into a CSV
* Download and store the images from those tweets in the specified collation medium

## Run the lambda

Test the lambda as it stands with `dotnet lambda invoke-function`, provide the local profile and a payload.

You can use `run-lambda.sh` to see some simple sample output listed (drawn from last week's [MetTaskforce](https://twitter.com/mettaskforce) twitter account), or provide your own values:

```
dotnet lambda invoke-function ImageCollatorFunction --region eu-west-2 --profile sa-image-collator --payload '{ "collation": "", "accounts": [], "filter": "", "keywords_list": [], "keywords_list_url": "", "period": "", "group": "" }'
```

The payload contains a number of instructions for the collator:

`collation` - Where to collate images. Choices are:

  * `list` (just lists the content of tweets chosen for inclusion)
  * `download` (downloads tweets and images to local file system)
  * `s3` (stores tweets and images in an AWS S3 bucket)
  * `github` (stores tweets and images in a github repository)

`accounts` - A list of the accounts to inspect.

`filter` - A filter to appy to these accounts. Choices are:

  * `images` (all tweets with images are retrieved)
  * `keywords` (all tweets that contain any of the keywords are retrieved)

`keywords_list` - All keywords to consider for the `keywords` filter.

`keywords_list_url` - A URL to retrieve the keywords list from instead.

  * eg. https://raw.githubusercontent.com/PoliceRewired/image-collations/main/keywords-list-knife-tweets.txt

`period` - The date range to search. Choices are:

* `today`
* `yesterday`
* `thisweek`
* `lastweek`
* `dd-MM-yyyy:dd-MM-yyyy` (a range of days starting at the beginning of the day of the first date, ending at the beginning of the day of the second date)

`group` - Filename to group results by (eg. `test-group-mps-taskforce`) - this forms a part of the directory structure.

## Manual testing

You can test the tool locally using the associated command line app `KnifeImageCollatorApp`.

* Enter the directory: `cd KnifeImageCollator/KnifeImageCollatorApp`
* Pick a name for your environment, eg. `prod`
* Create an env file in the same directory as your binary, eg. `.env.prod`
* Place your environment variables in the `.env.prod` file, as `KEY=value`, one per line.
* Configure that file in your project: Build action: `Content`, Copy to output directory: `Always copy`
* Build and run the app:

```
dotnet run <environment> <username> <period> <filter> <collation> <group> <keywords-list-url>
```

It will retrieve and summarise the tweet media posted by that username, placing it all the following folder structure:

```
<group>/collated-media.csv
<group>/<yyyy-MM-dd>/<timestamp>-<username>-<index>.<png|jpg>
```

eg.

```bash
dotnet run prod mettaskforce lastweek images download test-group-met-taskforce
```

or, to retrieve a recent range:

```bash
dotnet run prod mettaskforce 2021-01-01:2021-03-21 keywords download test-group-met-taskforce https://raw.githubusercontent.com/PoliceRewired/image-collations/main/keywords-list-knife-tweets.txt
```


## The lambda

### Set up AWS CLI

Create a service account IAM user. In our case: `sa-image-collator`

You'll need to safely store the access key id and secret access key.

Configure a local profile to match, and provide the access key id, and secret access key.

```
aws configure --profile sa-image-collator
```

### Template

To create a template lambda project:

```
dotnet new -i Amazon.Lambda.Templates
dotnet new lambda.EmptyFunction --help
dotnet new lambda.EmptyFunction --name DistributeSocialLambda
```

### Deploy

The Amazon lambda tools are required (installed by `init.sh`):

```
dotnet tool install -g Amazon.Lambda.Tools
dotnet tool update -g Amazon.Lambda.Tools
```

Use `deploy.sh` or the dotnet lambda to deploy the function naming the profile and role:

```
dotnet lambda deploy-function --profile sa-image-collator ImageCollatorFunction --function-role role-image-collator
```

If the `role-image-collator` role doesn't exist, you'll need to create it first. You'll also have to attach an IAM Policy to the role. `AWSLambdaExecute`, looks about right.

#### Deployment environment

You can provide the environment variables to the lambda through AWS web interface.

If you'd rather do it from the command line, you can use the `--environment-variables` option to provide the various secrets, as: `<key1>=<value1>;<key2>=<value2>` etc.

You could also add an `environment-variables` key in the `aws-lambda-tools-default.json` file, but be careful not to include your secrets in a public github repo.

## Environment variables

Provide the following environment variables:

* `TWITTER_CONSUMER_KEY`
* `TWITTER_CONSUMER_KEY_SECRET`
* `TWITTER_ACCESS_TOKEN`
* `TWITTER_ACCESS_TOKEN_SECRET`
* `AWS_S3_BUCKET` (if storing data in an S3 bucket)
* `GITHUB_OWNER` (if storing data in github)
* `GITHUB_REPOSITORY` (if storing data in github)
* `GITHUB_TOKEN` (if storing data in github)