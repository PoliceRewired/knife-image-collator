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
dotnet run <environment> <username>
```

This application is a work in progress. At the moment it will retrieve and summarise all tweets posted by that username today.

## Environment variables

Provide the following environment variables:

* `TWITTER_CONSUMER_KEY`
* `TWITTER_CONSUMER_KEY_SECRET`
* `TWITTER_ACCESS_TOKEN`
* `TWITTER_ACCESS_TOKEN_SECRET`
