# PBD2D.Systems

### Edge Length Constraint

System responsible for resolving edge length constraint, the most common constraint in PBD simulation.
Constraint is defined in the following way

$$ C(p_1, p_2) = \| \vec p_1 - \vec p_2 \| - \ell $$

where $\ell$ is rest length of the edge $(p_1, p_2)$.

### Triangle Area Constraint

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

### Collisions

### Point Line Collision

System responsible for detecting and resolving collisions between bodies which provide point and line (infinite).
It works on components which implement the `IPointLineCollisionTuple`.
For example system is responsible for resolving [TriMesh](#trimesh)-[Ground](#ground) collisions.

### Mouse Interaction

System used for interacting mouse pointer with the simulated object, rather for debug purposes.
It works on components which implement the `IMouseInteractionComponent`.
