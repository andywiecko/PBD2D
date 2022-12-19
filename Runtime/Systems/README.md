# PBD2D.Systems

- [PBD2D.Systems](#pbd2dsystems)
  - [Collisions](#collisions)
    - [Capsule Capsule Collision System](#capsule-capsule-collision-system)
    - [Point Line Collision System](#point-line-collision-system)
    - [Point TriField Collision System](#point-trifield-collision-system)
  - [Constraints](#constraints)
    - [Edge Length Constraints System](#edge-length-constraints-system)
    - [Shape Matching Constraint System](#shape-matching-constraint-system)
    - [Triangle Area Constraints System](#triangle-area-constraints-system)
    - [Point Point Connector Constraints System](#point-point-connector-constraints-system)
    - [Position Hard Constraints System](#position-hard-constraints-system)
    - [Regenerate Position Constraints System](#regenerate-position-constraints-system)
    - [Stencil Bending Constraints System](#stencil-bending-constraints-system)
    - [Position Soft Constraints System](#position-soft-constraints-system)
  - [Debug](#debug)
    - [Mouse Interaction System](#mouse-interaction-system)
  - [Extended data](#extended-data)
    - [Bounding Volume Tree System](#bounding-volume-tree-system)
    - [Bounding Volume Tree External Edges System](#bounding-volume-tree-external-edges-system)
    - [Bounding Volume Tree Points System](#bounding-volume-tree-points-system)
    - [Bounding Volume Trees Intersections System](#bounding-volume-trees-intersections-system)
    - [Bounding Volume Tree Triangles System](#bounding-volume-tree-triangles-system)
    - [Bounds System](#bounds-system)
  - [Graphics](#graphics)
    - [TriMesh Renderer System](#trimesh-renderer-system)
    - [EdgeMesh Renderer System](#edgemesh-renderer-system)
  - [Position based dynamics](#position-based-dynamics)
    - [Position Based Dynamics Step End System](#position-based-dynamics-step-end-system)
    - [Position Based Dynamics Step Start System](#position-based-dynamics-step-start-system)

## Collisions

### Capsule Capsule Collision System

The experimental system which implements a capsule-capsule collision algorithm.
It resolves capsule distance constraints.
The system supports friction (use `PhysicalMaterial` to control the behavior).
Currently, algorithm implementation does not resolve the case for capsules segments intersection.

### Point Line Collision System

The system is responsible for resolving collisions between bodies, which provide points, and lines (unbounded).
The system supports friction (use `PhysicalMaterial` to control the behavior) for both, i.e. body and a line can be translated.

### Point TriField Collision System

The system implements a collision algorithm between points and `TriField`.
The algorithm prevents from bodies stacking and since it's "volumetrical" can be a good competitor with continuous collision algorithms which are more costly.
The system supports friction (use `PhysicalMaterial` to control the behavior).

## Constraints

### Edge Length Constraints System

The system is responsible for resolving edge length constraints, the most common constraint in PBD simulation.
The constraint is defined in the following way

$$ C(\vec p_1, \vec p_2) = \| \vec p_1 - \vec p_2 \| - \ell $$

where $\ell$ is rest length of the edge $(\vec p_1, \vec p_2)$.

The system supports XPBD compliance and PBD stiffness.

### Shape Matching Constraint System

The system is responsible for resolving the shape matching constraint.
The method has many advantages:

- can be easily embedded withing PBD framework,
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

The system supports the linear deformation model.

### Triangle Area Constraints System

The system is responsible for resolving triangle area (signed) constraints.
Constraint enforces that triangle area is conserved during the simulation.
The constraint function is defined in the following way

$$ C(p_1, p_2, p_3) = \vec p_{12} \times \vec p_{13} - A$$

where $p_{ij} = p_j - p_i$ and $A$ is 2 times rest area of the triangle $(p_1, p_2, p_3)$.

The system supports XPBD compliance and PBD stiffness.

### Point Point Connector Constraints System

The system is responsible for solving point-point connector constraints.
The constraint is used for connecting two separate bodies.
The constraint function is defined in a similar way as for edge length constraint (with zero rest length)

$$C(\vec p_1, \vec p_2) = \| \vec p_1 - \vec p_2\|.$$

### Position Hard Constraints System

The system is responsible for maintaining the rest position.
An applied constraint is _hard_, i.e. weight of constrained points is set to zero.
Regarding the interaction, point position should be always preserved.

### Regenerate Position Constraints System

The system is responsible for position constraint regeneration.
The rest position of the constraints is calculated using given `Transform`

> **Note**
>
> `Transform.rotation` is not supported.

### Stencil Bending Constraints System

The system is responsible for resolving bending constraints for angles made of three points $\vec p_1, \vec p_2, \vec p_3$.
The constraint enforces that the angle is conserved during the simulation.
The constraint function is defined in the following way

$$ C(p_1, p_2, p_3) = \arctan\frac{(\vec p_2 - \vec p_1)\times(\vec p_3 - \vec p_2)}
{(\vec p_2 - \vec p_1)\cdot(\vec p_3 - \vec p_2)} - \varphi_0,$$

where $\varphi$ corresponds to rest angle.

The system supports XPBD compliance and PBD stiffness.

> **Warning**
>
> Strong stiffness/compliance setting currently leads to simulation instabilities. Use the constraint with caution. 

### Position Soft Constraints System

The system is responsible for maintaining the rest position.
The constraint function is defined in the following way

$$ C(\vec p) = \| \vec p - \vec p_0 \|,$$

where $p_0$ is rest position of the constraint.

The system supports XPBD compliance and PBD stiffness.

## Debug

### Mouse Interaction System

The system is used for interacting mouse pointer with the simulated object (currently rather for debugging purposes).
It supports translation and rotation of multiple points.
To grab a body use the left mouse click when dragging a body use the mouse scroll to rotate a body around the current mouse position.
For additional settings or debugging attach `MouseInteractionConfiguration` for your `World`.

## Extended data

### Bounding Volume Tree System

The generic abstract system `BoundingVolumeTreeSystem<T>` responsible for updating the `NativeBoundingVolumeTree<AABB>`, where `T` is `IConvertableToAABB`.

### Bounding Volume Tree External Edges System

Implementation of [Bounding Volume Tree System](#bounding-volume-tree-system) for `T` equals `ExternalEdge`.

### Bounding Volume Tree Points System

Implementation of [Bounding Volume Tree System](#bounding-volume-tree-system) for `T` equals `Point`.

### Bounding Volume Trees Intersections System

The system gathers the results of the `NativeBoundingVolumeTree<AABB>` intersections.
The result of intersection is stored in a list of `int2` structs.

### Bounding Volume Tree Triangles System

Implementation of [Bounding Volume Tree System](#bounding-volume-tree-system) for `T` equals `Triangle`.

### Bounds System

The action-only system is responsible for updating bounds of the object.
Bounds are used in collision algorithms scheduling.

## Graphics

### TriMesh Renderer System

The system is responsible for updating the `Mesh`.
It syncs mesh vertices with simulation positions and updates the bounds.

### EdgeMesh Renderer System

The system is responsible for updating the `EdgeMesh` `LineRenderer`.
It syncs segments vertices with simulation positions.

## Position based dynamics

### Position Based Dynamics Step End System

The system updates the velocity $\vec v$ for position based dynamics body

$$
\vec v = \frac{\vec p - \vec q}{\Delta t},
$$

where $\vec p$ is position, $\vec q$ is previous position, and $\Delta t = \text dt / n$ is reduced time step size.

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
