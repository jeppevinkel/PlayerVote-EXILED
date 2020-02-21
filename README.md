# PlayerVote-EXILED
## Description
This is a port of the [callvote](https://github.com/PatPeter/callvote) plugin from Smod2, to support EXILED.

## Configuration Settings
Setting Key | Value Type | Default Value | Description
--- | --- | --- | ---
playervote_enable | boolean | true | Whether or not the plugin should be enabled on the server.
playervote_vote_duration | integer | 30 | Number of seconds for a vote to last for.

## Commands
Server Command | Client Command | Parameters | Description
--- | --- | --- | ---
callvote | .callvote | "Custom Question" | Vote on a custom yes/no question.
callvote | .callvote | "Custom Question" "First Option" "Second Option" ... | Vote on a question with multiple options
stopvote | .stopvote | [none] | Stops a vote currently in progress
yes | .yes | [none] | Alias for 1
no | .no | [none] | Alias for 2
1 | .1 | [none] | Vote for the 1st option
2 | .2 | [none] | Vote for the 2nd option
3 | .3 | [none] | Vote for the 3rd option
4 | .4 | [none] | Vote for the 4th option
5 | .5 | [none] | Vote for the 5th option
6 | .6 | [none] | Vote for the 6th option
7 | .7 | [none] | Vote for the 7th option
8 | .8 | [none] | Vote for the 8th option
9 | .9 | [none] | Vote for the 9th option
0 | .0 | [none] | Vote for the 10th option

## Permissions
Permission | Description
--- | ---
playervote.custom | Permission to start custom votes.
playervote.stopvote | Permission to stop votes.
playervote.vote | Permission to vote.
