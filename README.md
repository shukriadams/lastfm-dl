# Lastfm-dl

App for downloading scrobbles from Last.fm - save your play history for posterity. Data is stored as a JSON file. Works on large histories - 900K+ scrobbles, going back to 2004.

## Features

- Download interruption - stop and resume downloads until complete.
- Updated downloads - completed downloads can be updated with latest plays
- Will not hammer last.fm, mostly avoiding rate limits.

## Install

Download the latest build of lastfm-dl from [releases](https://github.com/shukriadams/lastfm-dl/releases). Linux and Windows, x86 and arm64 are supported. No dependencies are required. Save to wherever you normally store binaries on your system. If on Linux make the file executable (chmod +x). Add the file location to your system PATH if desired.

This is a terminal app, so run it in a terminal with

    lastfm-dl 

By default, lastfm-dl uses the path ~/lastfmdl, and your scrobbles are saved to ~/lastfmdl/scrobbles.json  You can manually overwrite this location, but all examples below assume the default location.

It's a good idea to initialize with

    lastfm-dl --init

This will automatically create your save location, and also warn you that you need a cookie.

## Authentication

To access scrobble date you need a valid browser cookie, for any last.fm user. If you unsure about using your main user, create a throwaway profile and use that instead. A cookie is some text stored in your browser, and there are several ways to get it. The easiest is using a cookie reading plugin. All examples below are for Firefox. 

### Plugin 

- install [cookie-editor](https://addons.mozilla.org/en-US/firefox/addon/cookie-editor), allow cookie-editor to pin to your taskbar.
- log in to last.fm.
- while viewing any last.fm page click the cookie-editor icon in your taskbar.
- in the lower right corner of the cookie-editor panel click the "export" button, and select "header string".
- create a text file at `~/lastfmdl/cookie` (no extension), paste the cookie string into it and save. It will most likely be a single long line of text.

### Manually

If you don't want to use a plugin, you can get the cookie string as follows

- Log in to Last.fm in your browser, go to https://last.fm, your current page address should be https://last.fm/home
- Access the develop console (Ctrl-Shift-I), and go to the network tab
- Click F5 to reload the page. You'll see a lot of items on this list. 
- Scroll to the top of the list, the second item's "File" column should be "home". Click this, it will open a panel with
details for this item.
- Under `Headers`, scroll down to the `Request` section, right-click on the `Cookie` entry, and `Copy value`.
- create a text file at `~/lastfmdl/cookie` (no extension), paste the cookie string into it and save. It will most likely be a single long line of text.

### Verifying cookie

Once you save your cookie reinitialize with
    
    lastfm-dl --init

If you did everything correctly, this should be report that access works.

## Use

The simplest method to save your scrobbles is

    lastfm-dl --download --user <lastfm-username>

Scrobbles are downloaded newest first, one page of scrobbles at a time. When your entire history has been downloaded, all scrobbles are saved in a single file `~/lastfmdl/scrobbles.json`. You can interrupt downloads at any time with CTRL-C or CTRL-Z depending your operating system. You internet connection can also die, your system can restart etc. To resume simply rerun the command above, download will always resume until your entire history has been processed.

### Download new scrobbles

After successfully downloading your history, you can append new scrobbles to it as well. As long as `~/lastfmdl/scrobbles.json` is left in place, rerun the command above, A new session will start, you can interrupt and resume this too if necessary, and when complete `scrobbles.json` will be updated. An adjacent backup of `scrobbles.json` will also be created, just in case.

## Additional arguments

There are several optional arguments for downloading.

### Save path

By default lastfm-dl works in your home directory, where it will create a directory `lastfmdl`. Your scrobbles and temporary files will be created here. 

To use another path, use the `--save` parameter.

    lastfm-dl --save <path> ...<other args>...

### New session

By default, lastfm-dl supports download resumption, this is done as part of a session, and a session persists until all your existing scrobbles have been successfuly downloaded. If for some reason you want to abandon an existing session, or suspect a session is corrupt, you can force clear it with 

    lastfm-dl --clear ...<other args>...

### Session and page count

Lastfm-dl download resumption relies on lastfm's scrobble paging system, but page content changes as you scrobble new plays. If you try to resume a download while scrobbling, you could get a warning that your page count has changed since you started the session. You can ignore this warning with the ignore flag.

    lastfm-dl --ignore ...<other args>...

Note that you risk losing some of your history this way, the amount lost depends on how many plays have been scrobbled since the session started. The alternative to ignoring the error is to start a new session from scratch (using the --clear flag), or not to scrobble at all while you are downloading your history. There is no satisfying workaround for this, it's just how last.fm works. 


## Common issues

### Rate limits

If you have a lot of data, you may hit a short-term rate limit on your requests - this takes the form of an error 600. By default lastfm-dl will automatically retry a few times before giving up. If the you encounter a serious outtage, try again later.

### Timestamps and lost scrobbles

Lastfm scrobbles are not uniquely identifiable - they have a timestamp, but this timestamp is not completely accurate. Older scrobbles (from 2004) often share timestamps, and it seems that over time, Lastfm improved timestamp accuracy. 

Additionally, on very old profiles, it seems the total scrobble count displayed for a profile doesn't always match the number of scrobbles downloaded, though the differences seem to be very small (fractions of a percent).

### Multiple users

All scrobbles are saved to a single path. lastfm-dl does not differentiate between users. If you want to download data for multiple users, use the `--save` argument and save to a different location for each user.

## License

GPL 3.0 (see license file for more information)