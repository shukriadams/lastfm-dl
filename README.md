# Last FM downloader

Terminal app that downloads scrobbles from Last.fm - use this to download your play history for posterity. Data is stored as a JSON file.

## Features

Supports limited download resumption - if your download gets broken or interrupted, you can resume later. Note that
resumption is limited by  Last.fm's paging and data system :

- pages list user scrobbles newest to oldest
- total page count changes as user scrobble count increases.
- pages do not support any kind of fixed/absolute delimiter like time stamp or song index. If you go back far enough, Last.fm scrobbles can have 
duplicate timestamps or scrobble ids, so these are not reliable.

## Install

Download the latest build of lastfm download from [github.com.](https://github.com/shukriadams/lastfm-data-downloader/releases) for your OS.
No dependencies are required. If necessary, mark the binary as executable, move the binary where convenient, and add that location to your system path.

To use, open a terminal and run

    lastfmdl

## Requirements

You need a valid Lastfm cookie bound to your user. This part might be a bit tricky for non-technical people, apologies but this is how it is for now. There are many ways to get your authentiaction cookie from Last.fm, your browser might have a plugin that makes it easy. To get it directly

- Log in to last.fm on a browser like Firefox or Chrome, 
- Right mouse click on any last.fm page and "inspect" the page
- This brings up the common dev console, go to the "Network" tab. 
- Press "F5" to reload the page, scroll to the top of the all list of all the transfers, to the first successful GET transfer (status code 200), click it.
- Find the Headers tab for that items's transfer, then the Request section under that. Copy the entire cookie value (it's a long string)
- Create an empty text file anywhere convenient on your system, and paste the cookie string into it and save. Remember the path to this file, you'll need it below.

## Use

### Download

    lastfmdl --user <youruser> --cookie <Path-to-cookie-file>

### Collate

When you're done processing all your pages run

    lastfmdl --collate

This combines all pages into a single json file.

## Common issues

If you have a lot of data, you will likely hit a short-term rate limit on your requests - this takes the form of an error 600. Wait a few hours, try again. 


