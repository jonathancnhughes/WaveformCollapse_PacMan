# Waveform Collapse Pacman


An experiment learning to use the waveform collapse algorithm to generate Pacman level layouts.
Note: The actual game of pacman is not implemented (yet! Perhaps one day)

## Generation Steps:
- Set the desired height and width and also pick a palette, then click "Generate" to generate the pacman level.

- Before the waveform collapse algorithm can run, the grid is updated to make room for the ghost box in the center of the grid and a tunnel on the left hand-side. Currently only one tunnel is ever added half-way up the height of the grid. A possible extension is to randomly add one or two, depending on if the desired height of the grid will accommodate them.

- The left half of the level will be generated using the waveform collapse algorithm. Cells will show the possible number of tiles that could fit. As the algorithm runs the possibilities reduce as the grid is filled in. The algorithm starts with the cell with the lowest possibility of tile options and picks one at random. By picking a tile that will reduce the number of possibilities that the cell's neighbours can be. This is repeated until either the algorithm reaches a cell that has no possibilities or the left half of the grid is complete. If the algorithm cannot place a valid tile it will restart.

- Once the left half of the grid is complete it will be checked that it is valid  i.e. there are no disconnected loops which are very possible as waveform collapse is not enforcing any global rules on what tiles should be positioned.
This is done via a breadth first search on a graph that is built up as the tiles are placed. If the left half of the grid is found to be invalid, the waveform collapse algorithm will restart.

- If the left half of the grid is proved to be valid it will then be mirrored to the right hand side of the grid. Each tile on the left is iterated over and its mirrored tile is placed.

- Once the grid's tiles have been completed, the grid needs to be finalised by adding the border to the tiles on the outside of the grid. This is done with a maze-following algorithm that starts in the bottom left corner and walks the perimeter counter-clockwise by keeping the outside of the grid to the right of the direction it walks.

- After the outside border has been added, the sprites for the ghost box are updated to finish the grid.
