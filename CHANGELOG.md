# Change log

## [0.2.0] – 2022-12-19

### Added

- New entity `EdgeMesh` one dim structure for rods/ropes/trees/etc. simulation (including implementation of basic constraints and components).
- New entity `Points Locker` for locking the bodies. It can be used to lock points' positions during runtime (statically or dynamically).
- New entity `Point Point Connector` to connect entities that implement `IPointsProvider`. The implementation includes basic *generator*s and a few utilities.
- New constraint `Stencil Bending Constraint` preserving rest angle.
- New constraint `Position Constraint` (two variants: *soft* and *hard* one) preserving rest position.
- New constraint `Point Point Connector Constraint` connecting the bodies.
- A few minor utilities related to `AABB` were added.
- `RegeneratePositionConstraintsSystem` responsible for the regeneration of the constraints on transform change added.

### Fixed

- Resolves the bug related to the bad transformation of the trimesh renderer. For now, `TriMesh` as well as its parent can be scaled, translated, and rotated, and mesh will be calculated properly.

## [0.1.0] – 2022-09-10

### Added

- The first package release, which contains the following features:
  - `TriMesh` entity
  - `Ground` entity
  - (Extended) position based dynamics
  - Edge length constraints
  - Triangle area constraints
  - Shape matching constraints
  - point-line collision system
  - capsule-capsule collision system
  - point-trifield collision system
  - generating simulation bodies using sprites
  - mouse interaction (translation, rotation)
