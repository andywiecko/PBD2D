# PBD2D.Systems

- [PBD2D.Systems](#pbd2dsystems)
  - [Constraints](#constraints)
    - [Edge Length Constraint System](#edge-length-constraint-system)
    - [Triangle Area Constraint System](#triangle-area-constraint-system)
    - [Shape Matching Constraint System](#shape-matching-constraint-system)
  - [Collisions](#collisions)
    - [Point Line Collision System](#point-line-collision-system)
    - [Point TriField Collision System](#point-trifield-collision-system)
    - [Capsule Capsule Collision System](#capsule-capsule-collision-system)
  - [Debug](#debug)
    - [Mouse Interaction System](#mouse-interaction-system)
  - [Extended data](#extended-data)
    - [Bounding Volume Tree System](#bounding-volume-tree-system)
    - [Bounding Volume Tree External Edges System](#bounding-volume-tree-external-edges-system)
    - [Bounding Volume Tree Points System](#bounding-volume-tree-points-system)
    - [Bounding Volume Tree Triangles System](#bounding-volume-tree-triangles-system)
    - [Bounding Volume Trees Intersections System](#bounding-volume-trees-intersections-system)
    - [Bounds System](#bounds-system)
  - [Position based dynamics](#position-based-dynamics)
    - [Position Based Dynamics Step Start System](#position-based-dynamics-step-start-system)
    - [Position Based Dynamics Step End System](#position-based-dynamics-step-end-system)
  - [Graphics](#graphics)
    - [TriMesh Renderer System](#trimesh-renderer-system)
  - [Bibliography](#bibliography)

## Constraints

### Edge Length Constraint System

System responsible for resolving edge length constraint, the most common constraint in PBD simulation.
Constraint is defined in the following way

$$ C(p_1, p_2) = \| \vec p_1 - \vec p_2 \| - \ell $$

where $\ell$ is rest length of the edge $(p_1, p_2)$.

### Triangle Area Constraint System

System responsible for resolving triangle area (signed) constraint.
Constraint enforces that triangle area is conserved during the simulation.
The constraint function is defined in the following way

$$ C(p_1, p_2, p_3) = \vec p_{12} \times \vec p_{13} - A$$

where $p_{ij} = p_j - p_i$ and $A$ is 2 times rest area of the triangle $(p_1, p_2, p_3)$.

### Shape Matching Constraint System

The system is responsible for resolving the shape matching constraint.
Shape matching formulation can be found in the
[paper][muller.2005][^2].
The method has many advantages:

- can be easly embeded withing PBD framework,
- can be used for simulation _meshless_ objects,
- it is computationally cheap,
- can be used for simulation (quasi) rigid bodies.

Shape matching problem can be formulated using the following function

$$ f(\vec p, \vec q) = \sum_i w_i (R\vec q_i - \vec p_i)^2$$

where weight $w_i$ corresponds to inverse mass,
$q_i$ to the initial relative position with respect to the initial center of mass $t_0$,
and $p_i$ to the current relative position with respect to the current center of mass $t$.

The goal is to find rotation $R$, and translation $t$ that minimizes the function $f(\vec p, \vec q)$.
It can be shown analytically that such rotation $R$ can be found as a rotational part of the given matrix

$$A_{pq} = \sum_i m_i \vec p_i \vec q_i^{\mathsf T}$$

System supports the linear deformation model (see [^2] for more details).

**TODO:...**

## Collisions

### Point Line Collision System

System responsible for detecting and resolving collisions between bodies which provide point and line (infinite).
It works on components which implement the `IPointLineCollisionTuple`.
For example system is responsible for resolving [TriMesh](#trimesh)-[Ground](#ground) collisions.

### Point TriField Collision System

**TODO**

### Capsule Capsule Collision System

**todo**

## Debug

### Mouse Interaction System

The system used for interacting mouse pointer with the simulated object (currently rather for debugging purposes).
It supports translation and rotation of multiple points.
To grab a body use left mouse click, when draging a body use mouse scroll to rotate a body around current mouse position.
For additinal settings or debugging attach `MouseInteractionConfiguration` for your `World`.

## Extended data

### Bounding Volume Tree System

A generic abstract system `BoundingVolumeTreeSystem<T>` responsible for updating the `NativeBoundingVolumeTree<AABB>`, where `T` is `IConvertableToAABB`.

### Bounding Volume Tree External Edges System

Implementation of [Bounding Volume Tree System](#bounding-volume-tree-system) for `T` equals `ExternalEdge`.

### Bounding Volume Tree Points System

Implementation of [Bounding Volume Tree System](#bounding-volume-tree-system) for `T` equals `Point`.

### Bounding Volume Tree Triangles System

Implementation of [Bounding Volume Tree System](#bounding-volume-tree-system) for `T` equals `Triangle`.

### Bounding Volume Trees Intersections System

System gathers the results of `NativeBoundingVolumeTree<AABB>` intersections.
The result of intersection is stored in list of `int2` structs.

### Bounds System

Action only system responsible for updating bounds of the object.
Bounds are used in collision algorithms scheduling.

## Position based dynamics

### Position Based Dynamics Step Start System

The system implements the first step of PBD algorighm.
It updates velocity $\vec v$ using acceleration $\vec a$ and damping $\gamma$, solves position $\vec p$ using the updated velocity, and copies previous position $\vec q$

$$
\begin{cases}
\vec v \to \vec v + (\vec a - \gamma \vec v) \,\Delta t\\
\vec q \to \vec p\\
\vec p \to \vec p + \vec v \,\Delta t,
\end{cases}
$$

where $\Delta t = \text dt / n$ is reduced time step size.

### Position Based Dynamics Step End System

System updates the velocity $\vec v$ for position based dynamics body

$$
\vec v = \frac{\vec p - \vec q}{\Delta t},
$$

where $\vec p$ is position, $\vec q$ is previous position, and $\Delta t = \text dt / n$ is reduced time step size.

## Graphics

### TriMesh Renderer System

System responsible for updating the `Mesh`.
It sync mesh verticies with simulation positions and updates the bounds.

## Bibliography

[muller.2007]:https://doi.org/10.1016/j.jvcir.2007.01.005
[^1]:M.Müller, B.Heidelberger, M.Hennix, and J.Ratcliff, "Position based dynamics," [J. Vis. Commun. Image Represent., **18**, 2 (2007)][muller.2007].

[muller.2005]:https://doi.org/10.1145/1073204.1073216
[^2]:M.Müller, B.Heidelberger, M.Teschner, and M.Gros, "Meshless deformations based on shape matching," [ACM Trans. Graph. **24**, 3 (2005)][muller.2005].

[bender.2017]:https://doi.org/10.2312/egt.20171034
[^3]:J.Bender, M.Müller, and M.Macklin, "A Survey on Position Based Dynamics," [EG '17: Proceedings of the European Association for Computer Graphics: Tutorials (2017)][bender.2017].
