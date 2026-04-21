# Lastfm-dl

App for downloading scrobbles from Last.fm - save your play history for posterity. Data is stored as a JSON file. Works on large histories - 900K+ scrobbles, going back to 2004.

Available for Linux and Windows, x86/x64 and arm64.

## Features

- Supports download resumption
- Supports updating your existing download with newer scrobbles
- Will not hammer last.fm, mostly avoiding rate limits.

## Install

Download the latest build of lastfm-dl download from [github.com.](https://github.com/shukriadams/lastfm-dl/releases) for your OS. No dependencies are required. Place wherever you normally store binaries on your system, and add to your system PATH if desired.

To use, invoke directly from the terminal.

    lastfm-dl

## Requirements

You need a valid cookie bound to your Last.fm user. You can copy this from any modern browser when you login to Lastfm. There are several browser plugins that can help you do this, but if you want to get the cookie directly, in your browser

- Right mouse click on any Last.fm page and "inspect" the page
- This brings up the common dev console, go to the "Network" tab. 
- Press "F5" to reload the page, scroll to the top of the list of transfers (the first successful GET transfer status code 200), click it.
- Find the Headers tab for that item's transfer, then the Request section under that. Copy the entire cookie string
- Create an empty text file anywhere convenient on your system, and paste the cookie string into it and save. Remember the path to this file, you'll need it below.

## Use

The simplest method to save your scrobbles is

    lastfm-dl --download --user <lastfm-username> --cookie <path-to-cookie-file>

This requires your lastfm user name, and a valid login cookie save to a file on your pc. This will download all your scrobbles to `lastfm-dl/all-scrobbles.json` in your PC's default home directory. 

### Resume broken downloads

If your download gets interrupted, or if lastfm blocks you because you've hit a rate limit, you can resume the download by rerunning lastfm-dl with the same arguments you used previously. It will try to resume where it left off.

### Download new scrobbles

After completely downloading your history, you can download new scrobbles generated after that last download. Simply run the downloader again, using the same save directory as before, leaving the existing files in place. Or move your saves files to a new location and update the `--save` argument value. The file `lastfm-dl/all-scrobbles.json` is all that is needed. Lastfm-dl will update the `all-scrobbles.json` file, but will also generate an adjacent backup of this file before changing it.

## Additional arguments

There are several optional arguments for downloading.

### Save path

Set another path to save your scrobbles to with

    lastfm-dl --save <path> ...

### New session

By default, lastfm-dl supports download resumption, this is done as part of a session, and a session persists until all your existing scrobbles have been successfuly downloaded. If for some reason you want to abandon an existing session and start from scratch, use the clear switch

    lastfm-dl --clear ...

### Session and page count

Lastfm-dl download resumption relies on lastfm's scrobble paging system, but page content changes as you scrobble new plays. If you try to resume a download while scrobbling, you could get a warning that your page count has changed since you started the session. You can ignore this warning with the ignore flag.

    lastfm-dl --ignore ...

Note that you risk losing some of your history this way, the amount lost dependent on how many plays saved since last time. The alternative to ignoring the error is to start a new session from scratch (using the --clear flag), or not to scrobble at all while you are downloading your history.


## Common issues

### Rate limits

If you have a lot of data, you may hit a short-term rate limit on your requests - this takes the form of an error 600. Retry a few times, if it perists wait a few hours then try again.

### Timestamps and lost scrobbles

Lastfm scrobbles are not uniquely identifiable - they have a timestamp, but this timestamp is not completely accurate. Older scrobbles (from 2004) often share timestamps, and it seems that over time, Lastfm changed how timestamps were generated. 

Additionally, on very old profiles, it seems the total scrobble count displayed for a profile doesn't always match the number of scrobbles downloaded. On very old profiles with many plays (close to one million plays), total scrobble count and actual downloaded scrobbles are known to be off by a few hundred plays.

## License

GPL 3.0 (see license file for more information)