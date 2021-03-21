# Knife Image Collator

A tool to scan twitter accounts for images of knives and collate them for further classification.

This project is under development. Please check back for updates and more information.

## Automation

This tool is intended to run as an AWS lambda. Its tasks are:

* Retrieve tweets with images from specified github accounts
* 

## Manual testing

You can test the tool locally using the associated command line app `KnifeImageCollatorApp`.

* Enter the directory: `cd KnifeImageCollator/KnifeImageCollatorApp`
* Pick a name for your environment, eg. `prod`
* Create an env file in the same directory as your binary, eg. `.env.prod`
* Place your environment variables in the `.env.prod` file, as `KEY=value`, one per line.
* Configure that file in your project: Build action: `Content`, Copy to output directory: `Always copy`
* Build and run the app:

```
dotnet run <environment> <username> <period> <filter> <collation> <group>
```

It will retrieve and summarise the tweet media posted by that username, placing it all the following folder structure:

```
<group>/collated-media.csv
<group>/<yyyy-MM-dd>/<timestamp>-<username>-<index>.<png|jpg>
```

eg.

```bash
dotnet run prod instantiator today images download test-group
```

### period

Choices for **period** are:

* `today`
* `yesterday`
* `thisweek`
* `lastweek`
* a range, specifying start:end dates in this format: `dd-MM-yyyy:dd-MM-yyyy`

(NB. the first date is inclusive, but the second is not)

### filter

Choices for **filter** are:

* `images` - all images found are collated

### collation

Choices for **collation** are:

* `list` - logs all filtered images
* `download` - retrieves filtered images, places into a local folder structure
* `s3` - retrieves filtered images, transfers them to an s3 bucket
* `github` - retrieves filtered images, transfers them to a github repository

For the `s3` collation, also provide `S3_BUCKET` environment variable, and run in an environment with permission to acccess the S3 bucket (eg. a lambda function).

For the `github` collation, also provide `GITHUB_TOKEN` and `GITHUB_REPOSITORY` environment variables.

### group

For **group**, provide a name to group all results by.

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

### Test

Test the lambda as it stands with `dotnet lambda invoke-function`, provide the local profile and a payload.

You can use `run-lambda.sh`, or:

```
dotnet lambda invoke-function ImageCollatorFunction --region eu-west-2 --profile sa-image-collator --payload '{ "collation": "list", "accounts": "instantiator", "period": "2021-03-19:2021-03-20", "group": "test-group" }'
```

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