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

TODO: Add note about triangulation.

## Systems

### Constraints

#### Position Based Dynamics

#### Edge Length Constraint System

#### Triangle Area Constraint System

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
