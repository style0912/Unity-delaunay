# Fork of Unity-Delaunay

This fork exists to add:

 - Issue-tracking (missing from master project)
 - More interactivity in the demo project / add missing docs to source code

I hate forking projects - big waste of time - but with no issue-tracking on upstream, I really had no choice.

## Current features

Upgrades compared to upstream:

 - Demo project lets you enable/disable rendering of individual features
  - Helps you understand the definitions (undocumented in source)
  - Helps you experiment with the data structures before embedding in your own projects
 - Usage instructions!

## Instructions

 - Open the "demo" folder in Unity as a new Project
 - Open the "Demo" scene inside Unity (otherwise nothing will happen when you run)
 - Run the project
 - The main camera is blank, but in the Editor view you'll now see a coloured diagram of your data
 - Select the "Extended demo" object, and you have tickboxes to show/hide various features of the output data

# Unity-Delaunay

Voronoi diagrams, Delaunay triangulation, minimum spanning graphs, convex hull and more. Ported to C# for use in the Unity game engine from https://github.com/nodename/as3delaunay.

![Delaunay Triangulation](triangulation.png)
![Spanning Graph Example](spanning_graph.png)

### Features: ###

 - [Voronoi diagram](http://en.wikipedia.org/wiki/Voronoi)
 - [Delaunay triangulation](http://en.wikipedia.org/wiki/Delaunay_triangulation)
 - [Convex hull](http://en.wikipedia.org/wiki/Convex_hull)
 - [Minimum spanning tree](http://en.wikipedia.org/wiki/Euclidean_minimum_spanning_tree)
 - [Onion](http://cgm.cs.mcgill.ca/~orm/ontri.html)

MIT licensed, like the original.
Check out the original project page [here](http://nodename.github.com/as3delaunay/).
