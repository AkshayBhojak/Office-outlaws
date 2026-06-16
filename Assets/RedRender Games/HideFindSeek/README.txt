
Check out this video for how to add new level:

https://youtu.be/d-CBF8sgaus

Or:

1. Clone Level3 scene.
2. Rename this scene to Level4.
3. Add this scene to the BuildSettings.
4. Add/Replace Walls, Coins, GoldBars, Units or another geometry with colliders to Level4.
	a) Walls must have colliders attached.
	b) One of the units must be in 0,0,0 position.
5. Bake nav mesh in Window -> AI -> Navigaton -> Object -> Mesh Renderers -> Hierarchy -> Select Walls and FloorMesh only -> Navigaton -> Navigation Static checkbox -> Bake -> Bake.
6. Save scene.
7. Press Play In Editor.



To control player you can use:

1. Keyboard (WASD).
2. Mouse(Joystick).
3. Joystick.



Contact me if you have any question:
aleksandr.gorodiski@gmail.com
