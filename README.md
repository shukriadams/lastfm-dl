# Lastfm-dl

App for downloading scrobbles from Last.fm - use this to download your play history for posterity. Data is stored as a JSON file. Works on large histories - 900K+ scrobbles, going back to 2004.

## Features

- Allows some degree of download resumption (*)
- Will not hammer last.fm, mostly avoiding rate limits.

## Install

Download the latest build of lastfm download from [github.com.](https://github.com/shukriadams/lastfm-dl/releases) for your OS. No dependencies are required. Place wherever you normally store binaries on your system.

To use, invoke the app directly from the terminal.

    lastfm-dl

## Requirements

You need a valid cookie bound to your Lastfm user. Login to Lastfm in your browser - your browser now has a cookie to Lastfm. You need a copy of that cookie. There are probably browser plugins to help you do that, but you can the cookie directly in a Chromium or Firefox-based browser by

- Right mouse click on any last.fm page and "inspect" the page
- This brings up the common dev console, go to the "Network" tab. 
- Press "F5" to reload the page, scroll to the top of the list of transfers, to the first successful GET transfer (status code 200), click it.
- Find the Headers tab for that item's transfer, then the Request section under that. Copy the entire cookie value (it's a long string)
- Create an empty text file anywhere convenient on your system, and paste the cookie string into it and save. Remember the path to this file, you'll need it below.

## Use

    lastfm-dl --download --user <lastfm-username> --cookie <path-to-cookie-file>

## Common issues

If you have a lot of data, you may hit a short-term rate limit on your requests - this takes the form of an error 600. Retry a few times, if it perists wait a few hours then try again.


