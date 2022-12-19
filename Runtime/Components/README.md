# PBD2D.Components

- [PBD2D.Components](#pbd2dcomponents)
  - [Entities](#entities)
    - [EdgeMesh](#edgemesh)
    - [TriMesh](#trimesh)
    - [Ground](#ground)
    - [Points Locker](#points-locker)
    - [Point Point Connector](#point-point-connector)
  - [Tuples](#tuples)

## Entities

### EdgeMesh

A (quasi) one-dimensional structure made of edges. One of the basic entities used in the PBD2D simulation engine.
The structure can be used for simulation:

- rods,
- ropes,
- trees,
- bridges,
- any structure made of edges.

The entity requires `EdgeMeshSerializedData`.
One can create an instance of one of the selected implementations in `Project` from `Create/PBD2D/EdgeMesh/.` or one can derive a new type from `EdgeMeshSerializedData`.

**Supported features:**

- position based dynamics,
- edge length constraint,
- stencil bending constraint.

### TriMesh

A two-dimensional structure made of triangles.
This is the basic entity used in the PBD2D simulation engine.

One can convert sprites into simulated objects using a built-in triangulator and scriptable objects (in the project right click and then select `Create/PBD2D/TriMesh/Serialized Data (Texture2d)`) or use predefined shapes.
Below one can find an editor preview of the given scriptable object

![trimesh-serialized-data-texture2d](../../Documentation~/trimesh-serialized-data-texture-2d.png)

> **Note**
>
> Derive from `TriMeshSerializedData` to implement custom data.

**Supported features:**

- position based dynamics
- edge length constraint,
- triangle area constraint,
- shape matching constraint.
- collisions
  - capsule-capsule (with TriMesh)
  - point-line (with Ground)
  - point-trifield (with TriMesh)
- mouse interaction

### Ground

The `Ground` corresponds to an infinite line.
The line provides position and normal vectors and is used for collisions with points.
Translation of the component is included in friction calculations, however, transform for calculating the translation can be overridden.

**Supported features:**

- collisions
  - point-line (with TriMesh)

### Points Locker

The entity is related to the position constraints of another entity.
It requires target which implements `IPointsProvider` interface.
One can select _generator_ which generates the given constraints.

**Supported generators:**

- `Generator AABB` classifies point as constrained depending on the collection of `AABB`,
- `Generator User Defined Point` classifies points using user input.

### Point Point Connector

The entity which is used for marking two bodies, which implement `IPointsProvider` interface, as connected via point-point.

**Supported generators:**

- `Generator Points In Radius` adds constraint for points in given radius.

## Tuples

Except entities, `PBD2D.Components` contains the implementation of tuples which are required for the interaction between entities, e.g. collisions.
