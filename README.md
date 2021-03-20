# Knife Image Collator

A tool to scan twitter accounts for images of knives and collate them for further classification.

This project is under development. Please check back for updates and more information.

## Usage

### Automation

This tool will eventually run as an AWS lambda or batch task.

### Manual testing

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

#### period

Choices for **period** are:

* `today`
* `yesterday`
* `thisweek`
* `lastweek`
* a range, specifying start:end dates in this format: `dd-MM-yyyy:dd-MM-yyyy`

(NB. the first date is inclusive, but the second is not)

#### filter

Choices for **filter** are:

* `images`

#### collation

Choices for **collation** are:

* `list`
* `download`
* `s3`
* `github` (coming soon)

For the `s3` collation, also provide `S3_BUCKET` environment variable, and run in an environment with permission to acccess the S3 bucket.

For the `github` collation, also provide `GITHUB_TOKEN` and `GITHUB_REPOSITORY` environment variables.

#### group

For **group**, provide a name to group all results by.

## Environment variables

Provide the following environment variables:

* `TWITTER_CONSUMER_KEY`
* `TWITTER_CONSUMER_KEY_SECRET`
* `TWITTER_ACCESS_TOKEN`
* `TWITTER_ACCESS_TOKEN_SECRET`
* `AWS_S3_BUCKET` (optional)
* `GITHUB_REPOSITORY` (optional)
* `GITHUB_TOKEN` (optional)
