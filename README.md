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

* Choices for **period**: `today`, `yesterday`, `thisweek`, `lastweek`
* Choices for **filter**: `images`
* Choices for **collation**: `list`, `download`, `s3`, `github`*
* For **group**, provide a name to group all results by

It will retrieve and summarise the tweet media posted by that username, placing it all the following folder structure:

```
<group>/collated-media.csv
<group>/<yyyy-MM-dd>/<timestamp>-<username>-<index>.<png|jpg>
```

eg.

```bash
dotnet run prod instantiator today images download test-group
```

## Environment variables

Provide the following environment variables:

* `TWITTER_CONSUMER_KEY`
* `TWITTER_CONSUMER_KEY_SECRET`
* `TWITTER_ACCESS_TOKEN`
* `TWITTER_ACCESS_TOKEN_SECRET`
