# particle life  
this project was inspired by a youtube video i watched about the exact same project, and i wanted to see if i could make it from scratch without help  
the idea is that different types of particles (determined by their colour) apply specific forces to other types  
the part that makes cool stuff happen is that the forces are asymmetric, which violates one of newtons laws i dont remember which one (the one about equal and opposite forces), so this simulation isnt in any way a representation of real life  
even then, the patterns that emerge out of the simulation are really cool and look "life-like"  
  
without any optimization, this simulation started lagging hard at ~400 particles  
ive made quite a few optimizations, starting with gpu instancing.   
rather than having individual gameobjects with their own monobehaviour scripts, all particles are instead just turned into an array of a class containing 3 main properties - position, velocity, and type (which also determines its colour).  
i simply just take the position of all the particles in an array, and use manual gpu instancing to render their meshes and colours (materials), making the overall batches sent to the gpu each frame pretty low and optimized. this didnt really make much of a difference in the computation though, only increasing the passable particle count upto ~500 particles.  
next up, i added spatial partiotioning. this means that there is a grid on the entire board (the area the particles can move in), and rather than doing n^2 calculations, each particle only looks at the particles within nearby grid cells. the grid is made of board_size/radius grid cells, so there is the restriction of the radius needing to be a factor of the board size for it to not be weird with floats and stuff. another limitation is that given the nature of this optimization, if too many particles clump up close together, it approaches n^2 calculation which nothing can be done about. this increased the passable particle count upto ~2k.  
finally, i just called it a day and slapped on multithreaded processing. this was really easy to implement with unity, i just had to change a for loop to a Parallel.For loop. this increased the passable particle count all the way up to 10k given appropriate radius/dimension sizes and proximity repulsion distance values.

since the project relies on parallel processing and stuff, it doesnt run on the browser very well. i think its really fun to play around with and would suggest people to download and run the exe to play with it.  
  
[build](https://github.com/longestcow/particleLife/releases/tag/v1.0) [main code](https://github.com/longestcow/particleLife/blob/main/Assets/GameManager.cs) 
