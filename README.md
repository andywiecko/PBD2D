# PBD2D

Unity package for Position Based Dynamics in two dimensions.

> **Warning**
>
> The package is in a preview state.
> The API may change without advance notice.
> Production usage is not recommended.

The package provides implementation of _position based dynamics_ (PBD) [^1] 
[^3] [^5] using HPC# (via Burst compiler) 
including extended position based dynamics (xPBD) [^4], 
shape-matching [^2], 
various collision algorithms with basic friction model [^6].
There are also upcoming features regarding one-dimensional structures (rods) [^7] [^8]
and fluids [^9] with softbodies coupling.

Check out Matthias Müller's YouTube channel [**10 Minute Physics**](https://www.youtube.com/channel/UCTG_vrRdKYfrpqCv_WV4eyA) for great tutorial about PBD related topics!

**Package summary:**

- (Extended) position based dynamics, including:
  - edge length constraint
  - triangle area constraint
  - shape matching constraint
- Collision systems:
  - point-line
  - capsule-capsule
  - point-trifield
- Generating simulation bodies using sprites.
- Mouse interaction (translation, rotation)
- Entities:
  - trimesh (triangle mesh)
  - ground

## Gallery

TODO IMG

## Table od Contents

- [PBD2D](#pbd2d)
  - [Gallery](#gallery)
  - [Table od Contents](#table-od-contents)
  - [Getting started](#getting-started)
  - [Architecture](#architecture)
  - [Preview for upcoming features](#preview-for-upcoming-features)
  - [Roadmap](#roadmap)
  - [Dependencies](#dependencies)
  - [Bibliography](#bibliography)

## Getting started

Currently, Unity Engine does not supported git dependencies for custom git pacakges.
To install the PBD2D, one has to include dependencies manually.
The easiest way to do it is by coping the following includes into project manifest:

```json
TODO
```

See the example project [manifest.json](TODO).

## Architecture

The project architecture is based on the custom [ECS](https://en.wikipedia.org/wiki/Entity_component_system) pattern and it uses [andywiecko.ECS](https://github.com/andywiecko/ECS) as implementation of the core engine.

The package consist of three main assemblies:

- [`andywiecko.PBD2D.Core`](Runtime/Core) contains all contracts, common structs, and all required abstractions.
- [`andywiecko.PBD2D.Components`](Runtime/Components) which consists of components implementations.
- [`andywiecko.PBD2D.Systems`](Runtime/Systems) contains all available systems for components, i.e. all logic can be found here.

Below one can find a dependency graph for the main project assemblies.

```mermaid
%%{init: {"theme": "neutral", "flowchart": {"curve": "stepBefore", "useMaxwidth": false}}}%%

graph TB
b <--- a
c <--- a

a[Core]
b[Components]
c[Systems]

click a href "andywiecko/PBD2D/Runtime/Core"
click b href "andywiecko/PBD2D/Runtime/Components"
click c href "andywiecko/PBD2D/Runtime/Systems"
```

## Preview for upcoming features

- **Elastic rods**

![elastic-rod-pendulom](Documentation~/elastic-rod-pendulum.gif)
![elastic-rod-tree](Documentation~/elastic-rod-tree.gif)

- **Position based fluid**

![fluid-one-sided](Documentation~/fluid-one-sided.gif)
![fluid-two-sided](Documentation~/fluid-two-sided.gif)

## Roadmap

**v0.1.0**

- [ ] Sample scenes + build online.
    (Demos)
  - [ ] Washing machine scene (collisions)
  - [ ] Ramp pendulum / box pile collision
  - [ ] Duck + heavy weight scene
  - [ ] Shape matching stars
- [ ] CI/CD, git dependencies for unity-test-runner?
- [ ] Add imgs in readmes

**v1.0.0**

- [ ] Reimport and refactor rod structure.
- [ ] Reimport and refactor position based fluid.
- [ ] Destructible bodies (removing points/triangles during runtime).
- [ ] Refactor collision component and introduce collision layers.
- [ ] GC alloc free component iterators for system scheduling.
- [ ] TriMesh self collisions (external points/bvt/collisions).
- [ ] Connectors and lockers.
- [ ] Shape matching clusters.
- [ ] Use **dynamic** bounding volume tree for scheduling the collision pairs. 
- [ ] Investigate performance with combined dependencies.
- [ ] Fluid "fancy" shader.

**v2.0.0**

- [ ] (Smooth) cuttable bodies (adding points/triangles during runtime).
- [ ] Continous collisions.
- [ ] Position based rigid bodies
- [ ] Sign distance field collisions.
- [ ] GPU fluids.
- [ ] Position based "smoke".

## Dependencies

- [`Unity.Burst`](https://docs.unity3d.com/Packages/com.unity.burst@1.6/manual/index.html)
- [`Unity.Mathematics`](https://docs.unity3d.com/Packages/com.unity.mathematics@1.2/manual/index.html)
- [`Unity.Collections`](https://docs.unity3d.com/Packages/com.unity.collections@1.0/manual/index.html)
- [`Unity.Jobs`](https://docs.unity3d.com/Manual/JobSystem.html)
- [`andywiecko.ECS`](https://github.com/andywiecko/ECS)
- [`andywiecko.BurstTriangulator`](https://github.com/andywiecko/BurstTriangulator)
- [`andywiecko.BurstCollections`](https://github.com/andywiecko/BurstCollections)
- [`andywiecko.BurstMathUtils`](https://github.com/andywiecko/BurstMathUtils)

## Bibliography

[^1]:M.Müller, B.Heidelberger, M.Hennix, and J.Ratcliff, "Position based dynamics," [J. Vis. Commun. Image Represent., **18**, 2 (2007)](https://doi.org/10.1016/j.jvcir.2007.01.005).
[^2]:M.Müller, B.Heidelberger, M.Teschner, and M.Gros, "Meshless deformations based on shape matching," [ACM Trans. Graph. **24**, 3 (2005)](https://doi.org/10.1145/1073204.1073216).
[^3]:J.Bender, M.Müller, and M.Macklin, "A Survey on Position Based Dynamics," [EG '17: Proceedings of the European Association for Computer Graphics: Tutorials (2017)](https://doi.org/10.2312/egt.20171034).
[^4]:M.Macklin, M.Müller, and N.Chentanez, "XPBD: position-based simulation of compliant constrained dynamics," [Proceedings of the 9th International Conference on Motion in Games (2016)](https://doi.org/10.1145/2994258.2994272).
[^5]:J.Bender, M.Müller, M.A.Otaduy, M.Teschner, and M.Macklin, "A survey on position‐based simulation methods in computer graphics," [Computer Graphics Forum, **33**:228-251 (2014)](https://doi.org/10.1111/cgf.12346).
[^6]:M.Macklin, M.Müller, N.Chentanez, and T.-Y.Kim, "Unified particle physics for real-time applications," [ACM Trans. Graph. **33**, 4 (2014)](https://doi.org/10.1145/2601097.2601152).
[^7]:U.Nobuyuki, R.Schmidt, and J.Stam, "Position-based elastic rods," [SIGGRAPH '14: ACM SIGGRAPH 2014 Talks, **47**, 1 (2014)](https://doi.org/10.1145/2614106.2614158).
[^8]:K.Tassilo, and E.Schömer, "Position and orientation based Cosserat rods," [Eurographics/ ACM SIGGRAPH Symposium on Computer Animation, (2016)](http://doi.org/10.2312/sca.20161234).
[^9]:M.Macklin, and M.Müller, "Position based fluids," [ACM Trans. Graph. **32**, 4, 104 (2013)](https://doi.org/10.1145/2461912.2461984).
