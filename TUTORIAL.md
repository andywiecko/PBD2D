# PBD2D Tutorial

1. Add `Simulation Template` in project hierarchy (right click then `PBD2D/Simulation Template`).

![tutorial-1](Documentation~/tutorial-1.png)

2. Add entity `TriMesh` in project hierarchy (right click then `PBD2D/TriMesh`).

![tutorial-2](Documentation~/tutorial-2.png)

3. Connect entity with the `World` from `Simulation Template`.

![tutorial-3](Documentation~/tutorial-3.png)

4. Create serialized data in project assets.

![tutorial-4](Documentation~/tutorial-4.png)

5. Configure serialized data: select texture for triangulation, and hit "Triangulate" button. 

![tutorial-5](Documentation~/tutorial-5.png)

6. Select created serialized data in created entity. Note: turn on the gizmos to see the `TriMesh` preview. One can return to point 5. and play with the triangulation settings to produce different meshes.

![tutorial-6](Documentation~/tutorial-6.png)

7. Attach components required for your setup. Minimal setup for simulated softbody, collidable with ground and other `TriMesh` can be found below.

![tutorial-7](Documentation~/tutorial-7.png)

8. To control the behavior of the body, i.e. friction or collisions, create `Physical Material` in project assets and attach it the selected entity. 

![tutorial-8a](Documentation~/tutorial-8a.png)

![tutorial-8b](Documentation~/tutorial-8b.png)
