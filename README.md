# Shannon's Switching Game

## What is Shannon's Switching Game?

The Shannon switching game belongs to a class of games known as Maker-Breaker games.
These are two-player games in which one player wins if he succeeds in holding every
element of a winning-set, whereas the other wins if he succeeds in preventing this,
that is, by holding at least one element in each winning-set.

## Overview

This game, as mentioned before, is intended for two players.
The game consists of a finite multigraph G (undirected), with two distinct vertices A and B.
There are two roles that the players can choose from -
one player must play the role of Short, while the other assumes the role of Cut.

### Short

The goal of this player is to fix edges in G such that there is a path from A to B in G that
only utilizes the fixed edges.
Each turn, Short chooses to fix an edge in G that was not removed by Cut or fixed by
Short earlier.

### Cut

The goal of this player is to prevent Short from winning, by removing edges from G so that
it is impossible for Short to fix a path from A to B.
Each turn, Cut chooses to remove an edge in G that was not removed by Cut or fixed
by Short earlier.

## Video

This demo video provides a glimpse into the exciting gameplay and features of our implementation
of Shannon Switching Game.

[![Shannon's Switching Game Demo](https://img.youtube.com/vi/OIEMVKvCT9c/0.jpg)](https://www.youtube.com/watch?v=OIEMVKvCT9c)
