# PBD2D

Unity Position Based Dynamics in two dimensions

## Table od Contents

- [PBD2D](#pbd2d)
  - [Table od Contents](#table-od-contents)
  - [Getting started](#getting-started)
  - [Introduction](#introduction)
  - [Systems](#systems)
    - [Constraints](#constraints)
      - [Position Based Dynamics](#position-based-dynamics)
      - [Edge Length Constraint System](#edge-length-constraint-system)
      - [Triangle Area Constraint System](#triangle-area-constraint-system)
      - [Shape Matching Constraint System](#shape-matching-constraint-system)
    - [Collisions](#collisions)
      - [Point Line Collision System](#point-line-collision-system)
    - [Debug](#debug)
      - [Mouse Interaction System](#mouse-interaction-system)
    - [Graphics](#graphics)
  - [Components](#components)
    - [TriMesh](#trimesh)
    - [Ground](#ground)
  - [Architecture](#architecture)
  - [Roadmap](#roadmap)
    - [v0.1.0](#v010)
    - [v1.0.0](#v100)
    - [v2.0.0](#v200)
  - [Dependencies](#dependencies)
  - [Contributors](#contributors)
  - [Bibliography](#bibliography)

## Getting started

TODO: Add note about packages/and possible upm registry

## Introduction

Position based dynamics is a great method for interactive physics simulation.
For the newcomers the [paper][muller.2007][^1] is highly recommended.

... or [survey][bender.2017][^3]


TODO: Add note about triangulation.

## Systems

### Constraints

#### Position Based Dynamics

#### Edge Length Constraint System

System responsible for resolving edge length constraint, the most common constraint in PBD simulation.
Constraint is defined in the following way

![Figure](https://latex.codecogs.com/png.image?\dpi{150}&space;\bg_white&space;C(p_1,p_2)=\|\vec&space;p_1-\vec&space;p_2\|-\ell)

where _l_ is rest length of the edge (_p₁, p₂_).

#### Triangle Area Constraint System

System responsible for resolving triangle area (signed) constraint.
Constraint enforces that triangle area is conserved during the simulation.
The constraint function is defined in the following way

![Equation](https://latex.codecogs.com/png.image?\dpi{150}&space;\bg_white&space;C(p_1,p_2,p_3)=\vec&space;p_{12}\times&space;\vec&space;p_{13}-A)

where _pᵢⱼ_=_pⱼ_-_pᵢ_ and _A_ is 2 times rest area of the triangle (_p₁_,	_p₂_,	_p₃_).

#### Shape Matching Constraint System

The system is responsible for resolving the shape matching constraint.
Shape matching formulation can be found in the
[paper][muller.2005][^2].
The method has many advantages:

- can be easly embeded withing PBD framework,
- can be used for simulation _meshless_ objects,
- it is computationally cheap,
- can be used for simulation (quasi) rigid bodies.

Shape matching problem can be formulated using the following function

![Equation](https://latex.codecogs.com/png.image?\dpi{150}&space;\bg_white&space;f(\vec&space;p,\vec&space;q)=\sum_i&space;w_i\left(R\vec&space;q_i-\vec&space;p_i\right)^2)

where weight _wᵢ_ corresponds to inverse mass,
_qᵢ_ to the initial relative position with respect to the initial center of mass _t₀_,
and _pᵢ_ to the current relative position with respect to the current center of mass _t_.

The goal is to find rotation _R_, and translation _t_ that minimizes the function _f(p,q)_.
It can be shown analytically that such rotation _R_ can be found as a rotational part of the given matrix

![Equation](https://latex.codecogs.com/png.image?\dpi{150}&space;\bg_white&space;A_{pq}=\sum_im_i\vec&space;p_i\vec&space;q_i^\mathsf{T})

System supports the linear deformation model (see [^2] for more details).

**TODO:...**

### Collisions

#### Point Line Collision System

System responsible for detecting and resolving collisions between bodies which provide point and line (infinite).
It works on components which implement the `IPointLineCollisionTuple`.
For example system is responsible for resolving [TriMesh](#trimesh)-[Ground](#ground) collisions.

### Debug

#### Mouse Interaction System

System used for interacting mouse pointer with the simulated object, rather for debug purposes.
It works on components which implement the `IMouseInteractionComponent`.

### Graphics

## Components

### TriMesh

The simplest two-dimensional structure made of triangles.
One can convert sprites to simulated objects using built-in triangulator and scriptable objects.
**TODO**: tutorial how to.

Supported constraints:

- Edge length constraint (see [system](#edge-length-constraint-system)),
- Triangle area constraint (see [system](#triangle-area-constraint-system)),
- Shape matching constraint (see [system](#shape-matching-constraint-system))

### Ground

As the name suggests, the component represent the plane surface.
**TODO:**...

Implemented collisions:

- with [TriMesh](#trimesh).

## Architecture

The project architecture is based on the custom [ECS](https://en.wikipedia.org/wiki/Entity_component_system) pattern.
The package is divided into three separate assemblies and the _Core_ one, which defines relations between them:

- `andywiecko.PBD2D.Core` contains all contracts, common structs, and all required abstractions.
- `andywiecko.PBD2D.Components` which consists of components contracts implementations.
- `andywiecko.PBD2D.Systems` contains all available systems for components, i.e. all logic can be found here.
- `andywiecko.PBD2D.Solver` where one can find logic for system scheduling and execution order.

Below one can find a dependency graph for the main project assemblies.
Click on the selected graph element to see the details.

```mermaid
%%{init: {"theme": "neutral", "flowchart": {"curve": "stepBefore", "useMaxwidth": false}}}%%

graph TB
b --> a
c --> a
d --> a

a[Core]
b[Components]
c[Solver]
d[Systems]

click a href "https://github.com/andywiecko/PBD2D/tree/main/Runtime/Core"
click b href "https://github.com/andywiecko/PBD2D/tree/main/Runtime/Components"
click c href "https://github.com/andywiecko/PBD2D/tree/main/Runtime/Solver"
click d href "https://github.com/andywiecko/PBD2D/tree/main/Runtime/Systems"
```

## Roadmap

### v0.1.0

- [X] ~~Reimport and refactor tri mesh structure.~~
- [X] ~~Introduce list of constraints.~~
- [X] ~~Unify Bvt Systems.~~
- [X] ~~Remove static from `ComponentsSystemsRegistry`, by introducing the `World` class.~~
- [ ] Add Cheap collisions edge-edge
- [ ] Test for most of the systems.
- [ ] Investigate performance with combined dependencies.
- [ ] Add friction for trifield-trifield collisions
- [ ] [!] **Restitution**
- [ ] Sample scenes + build online.
- [ ] Move math/collection related parts into custom packages.
- [ ] Docs.
- [ ] CI/CD, git dependencies for unity-test-runner?
- [ ] Static box?
- [ ] Static circle?

### v1.0.0

- [ ] Reimport and refactor rod structure.
- [ ] Refactor system class to not be derived from `MonoBehaviour`. 
- [ ] Reimport and refactor position based fluid.
- [ ] Fluid "fancy" shader.
- [ ] Use **dynamic** bounding volume tree for scheduling the collision pairs. 

### v2.0.0

- [ ] Destructible bodies.
- [ ] Continous collisions.
- [ ] GPU fluids.
- [ ] Sign distance field collisions.

## Dependencies

- [`Unity.Burst`](https://docs.unity3d.com/Packages/com.unity.burst@1.6/manual/index.html)
- [`Unity.Mathematics`](https://docs.unity3d.com/Packages/com.unity.mathematics@1.2/manual/index.html)
- [`Unity.Collections`](https://docs.unity3d.com/Packages/com.unity.collections@1.0/manual/index.html)
- [`Unity.Jobs`](https://docs.unity3d.com/Manual/JobSystem.html)
- [`andywiecko.BurstTriangulator`](https://github.com/andywiecko/BurstTriangulator)
- [`andywiecko.BurstCollections`](https://github.com/andywiecko/BurstCollections)
- [`andywiecko.BurstMathUtils`](https://github.com/andywiecko/BurstMathUtils)

## Contributors

- [Andrzej Więckowski, Ph.D](https://andywiecko.github.io/).

## Bibliography

[muller.2007]:https://doi.org/10.1016/j.jvcir.2007.01.005
[^1]:M.Müller, B.Heidelberger, M.Hennix, and J.Ratcliff, "Position based dynamics," [J. Vis. Commun. Image Represent., **18**, 2 (2007)][muller.2007].

[muller.2005]:https://doi.org/10.1145/1073204.1073216
[^2]:M.Müller, B.Heidelberger, M.Teschner, and M.Gros, "Meshless deformations based on shape matching," [ACM Trans. Graph. **24**, 3 (2005)][muller.2005].

[bender.2017]:https://doi.org/10.2312/egt.20171034
[^3]:J.Bender, M.Müller, and M.Macklin, "A Survey on Position Based Dynamics," [EG '17: Proceedings of the European Association for Computer Graphics: Tutorials (2017)][bender.2017].
