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
      - [Shape Matching System](#shape-matching-system)
    - [Collisions](#collisions)
      - [Point Line Collision System](#point-line-collision-system)
    - [Debug](#debug)
      - [Mouse Interaction System](#mouse-interaction-system)
    - [Graphics](#graphics)
  - [Components](#components)
    - [TriMesh](#trimesh)
    - [Ground](#ground)
  - [Dependencies](#dependencies)
  - [Contributors](#contributors)

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

#### Shape Matching System

[paper][muller.2005][^2]

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
- Triangle area constraint (see [system](#triangle-area-constraint-system))

### Ground

As the name suggests, the component represent the plane surface.
**TODO:**...

Implemented collisions:

- with [TriMesh](#trimesh).

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

[muller.2007]:https://doi.org/10.1016/j.jvcir.2007.01.005
[^1]:M.Müller, B.Heidelberger, M.Hennix, and J.Ratcliff, "Position based dynamics," [J. Vis. Commun. Image Represent., **18**, 2 (2007)][muller.2007].

[muller.2005]:https://doi.org/10.1145/1073204.1073216
[^2]:M.Müller, B.Heidelberger, M.Teschner, and M.Gros, "Meshless deformations based on shape matching," [ACM Trans. Graph. **24**, 3 (2005)][muller.2005].

[bender.2017]:https://doi.org/10.2312/egt.20171034
[^3]:J.Bender, M.Müller, and M.Macklin, "A Survey on Position Based Dynamics," [EG '17: Proceedings of the European Association for Computer Graphics: Tutorials (2017)][bender.2017].
